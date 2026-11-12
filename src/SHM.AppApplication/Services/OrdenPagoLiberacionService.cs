using System.Globalization;
using Microsoft.Extensions.Logging;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.DTOs.OrdenPagoLiquidacion;
using SHM.AppDomain.DTOs.SapApi;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Libera en SAP los comprobantes de una orden de pago aprobada (LIBERA_PAGO_CABSet).
/// Arma un solo envio por orden con todos sus comprobantes, valida los datos antes de
/// llamar a SAP y guarda el resultado por comprobante en SHM_ORDEN_PAGO_LIBERACION.
/// Ver docs/requerimientos/02_liberacion_pagos/propuesta_solucion.md
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public class OrdenPagoLiberacionService : IOrdenPagoLiberacionService
{
    private const int MaxIdOrden = 20;
    private const int MaxDocumento = 10;
    private const int MaxReferencia = 16;
    private const int MaxRuc = 20;
    private const int MaxProveedor = 35;
    private const int MaxUsuario = 12;
    private const int MaxMensaje = 2000;
    private const int MaxRespuesta = 4000;

    private readonly IOrdenPagoRepository _ordenPagoRepository;
    private readonly IOrdenPagoLiquidacionRepository _ordenPagoLiquidacionRepository;
    private readonly IOrdenPagoAprobacionRepository _aprobacionRepository;
    private readonly IOrdenPagoLiberacionRepository _liberacionRepository;
    private readonly ISedeRepository _sedeRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISapApiService _sapApiService;
    private readonly IBitacoraService _bitacoraService;
    private readonly ILogger<OrdenPagoLiberacionService> _logger;

    public OrdenPagoLiberacionService(
        IOrdenPagoRepository ordenPagoRepository,
        IOrdenPagoLiquidacionRepository ordenPagoLiquidacionRepository,
        IOrdenPagoAprobacionRepository aprobacionRepository,
        IOrdenPagoLiberacionRepository liberacionRepository,
        ISedeRepository sedeRepository,
        IUsuarioRepository usuarioRepository,
        ISapApiService sapApiService,
        IBitacoraService bitacoraService,
        ILogger<OrdenPagoLiberacionService> logger)
    {
        _ordenPagoRepository = ordenPagoRepository;
        _ordenPagoLiquidacionRepository = ordenPagoLiquidacionRepository;
        _aprobacionRepository = aprobacionRepository;
        _liberacionRepository = liberacionRepository;
        _sedeRepository = sedeRepository;
        _usuarioRepository = usuarioRepository;
        _sapApiService = sapApiService;
        _bitacoraService = bitacoraService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el resultado de liberacion de los comprobantes de una orden de pago.
    /// </summary>
    public Task<IEnumerable<OrdenPagoLiberacion>> GetByOrdenPagoIdAsync(int idOrdenPago)
        => _liberacionRepository.GetByOrdenPagoIdAsync(idOrdenPago);

    /// <summary>
    /// Libera en SAP todos los comprobantes de la orden de pago.
    /// </summary>
    public async Task<(bool Exito, string Mensaje)> LiberarAsync(int idOrdenPago, int idUsuario)
    {
        OrdenPago? ordenPago = null;
        try
        {
            ordenPago = await _ordenPagoRepository.GetByIdAsync(idOrdenPago);
            if (ordenPago == null)
                return (false, "Orden de pago no encontrada.");

            if (ordenPago.Estado != EstadoDescripcion.OrdenPago.Aprobado)
                return (false, "La orden de pago no esta aprobada.");

            var comprobantes = (await _ordenPagoLiquidacionRepository
                .GetDetalleLiquidacionesByOrdenPagoIdAsync(idOrdenPago)).ToList();
            if (comprobantes.Count == 0)
                return (false, "La orden de pago no tiene comprobantes.");

            // Si todos ya estan liberados no se vuelve a enviar
            var previos = (await _liberacionRepository.GetByOrdenPagoIdAsync(idOrdenPago)).ToList();
            if (previos.Count >= comprobantes.Count &&
                comprobantes.All(c => previos.Any(p => p.IdProduccion == c.IdProduccion &&
                                                        p.EstadoLiberacion == EstadoLiberacion.Liberado)))
            {
                return (true, "Los pagos de la orden ya estan liberados en SAP.");
            }

            var sede = await _sedeRepository.GetByIdAsync(ordenPago.IdSede);
            var sociedad = sede?.CodigoSociedadSap?.Trim();

            var (loginAprobador, fechaAprobacion) = await ObtenerUltimaAprobacionAsync(idOrdenPago, ordenPago);

            // 1. Validar y armar los documentos
            var errores = new Dictionary<int, string>();
            var documentos = new List<SapLiberaPagoDocumentoDto>();
            var idOrdenSap = (ordenPago.NumeroOrdenPago ?? string.Empty).Trim();

            var errorGeneral = new List<string>();
            if (string.IsNullOrEmpty(idOrdenSap))
                errorGeneral.Add("La orden de pago no tiene numero");
            else if (idOrdenSap.Length > MaxIdOrden)
                errorGeneral.Add($"El numero de orden excede {MaxIdOrden} caracteres");
            if (string.IsNullOrEmpty(sociedad))
                errorGeneral.Add($"La sede '{sede?.Nombre ?? ordenPago.NombreSede}' no tiene Sociedad SAP configurada");
            else if (sociedad.Length > 4)
                errorGeneral.Add("La Sociedad SAP de la sede excede 4 caracteres");
            if (string.IsNullOrEmpty(loginAprobador))
                errorGeneral.Add("No se pudo determinar el usuario del ultimo aprobador");

            foreach (var c in comprobantes)
            {
                var errorComp = ValidarComprobante(c);
                if (errorComp != null)
                {
                    errores[c.IdProduccion] = errorComp;
                    continue;
                }

                documentos.Add(ConstruirDocumento(c, idOrdenSap, sociedad ?? string.Empty,
                    loginAprobador ?? string.Empty, fechaAprobacion));
            }

            if (errorGeneral.Count > 0 || errores.Count > 0)
            {
                return await RegistrarFalloValidacionAsync(ordenPago, comprobantes, errores, errorGeneral,
                    sociedad, idUsuario);
            }

            // 2. Enviar a SAP (un solo envio con todos los comprobantes)
            var request = new SapLiberaPagoRequestDto
            {
                IdOrden = idOrdenSap,
                Fecha = fechaAprobacion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Documentos = documentos
            };

            var resultado = await _sapApiService.LiberarPagoAsync(request);

            // 3. Guardar el resultado en cada comprobante
            var estado = resultado.Exito ? EstadoLiberacion.Liberado : EstadoLiberacion.Error;
            var mensaje = Truncar(resultado.Mensaje, MaxMensaje) ??
                          (resultado.Exito ? "Documentos recibidos" : "SAP no confirmo la liberacion");
            var respuesta = Truncar(resultado.RespuestaRaw, MaxRespuesta);

            foreach (var c in comprobantes)
            {
                await _liberacionRepository.SaveResultadoAsync(new OrdenPagoLiberacion
                {
                    IdOrdenPago = idOrdenPago,
                    IdProduccion = c.IdProduccion,
                    Sociedad = sociedad,
                    Documento = c.CodSapCpm?.Trim(),
                    Ejercicio = c.FechaEmision?.Year.ToString(CultureInfo.InvariantCulture),
                    DocumentoDt = string.IsNullOrWhiteSpace(c.CodSapDetrac) ? null : c.CodSapDetrac.Trim(),
                    EstadoLiberacion = estado,
                    FechaLiberacion = resultado.Exito ? DateTime.Now : null,
                    MensajeSap = mensaje,
                    RespuestaSap = respuesta
                }, 1, idUsuario);
            }

            await RegistrarBitacoraAsync(ordenPago, idUsuario, resultado.Exito, mensaje);

            _logger.LogInformation(
                "Liberacion SAP orden {NumeroOP}: {Estado}. Comprobantes: {Cantidad}. Mensaje: {Mensaje}",
                ordenPago.NumeroOrdenPago, estado, comprobantes.Count, mensaje);

            return resultado.Exito
                ? (true, $"Pagos liberados en SAP ({comprobantes.Count} comprobante(s)).")
                : (false, $"SAP no libero los pagos: {mensaje}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al liberar pagos en SAP. IdOrdenPago: {IdOrdenPago}", idOrdenPago);
            return (false, "Error inesperado al liberar los pagos en SAP.");
        }
    }

    /// <summary>
    /// Devuelve el login (truncado a 12) y la fecha del ultimo nivel de aprobacion aprobado.
    /// </summary>
    private async Task<(string? Login, DateTime Fecha)> ObtenerUltimaAprobacionAsync(int idOrdenPago, OrdenPago ordenPago)
    {
        var aprobaciones = await _aprobacionRepository.GetByOrdenPagoIdAsync(idOrdenPago);
        var ultima = aprobaciones
            .Where(a => a.Estado == EstadoDescripcion.Aprobacion.Aprobado && a.IdUsuarioAprobador.HasValue)
            .OrderByDescending(a => a.FechaAprobacion ?? DateTime.MinValue)
            .ThenByDescending(a => a.Orden ?? 0)
            .FirstOrDefault();

        string? login = null;
        if (ultima?.IdUsuarioAprobador != null)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(ultima.IdUsuarioAprobador.Value);
            login = usuario?.Login?.Trim();
            if (login != null && login.Length > MaxUsuario)
                login = login[..MaxUsuario];
        }

        var fecha = ultima?.FechaAprobacion ?? ordenPago.FechaAprobacion ?? DateTime.Now;
        return (login, fecha);
    }

    /// <summary>
    /// Valida los datos que SAP exige de un comprobante. Devuelve el detalle del error o null.
    /// </summary>
    private static string? ValidarComprobante(DetalleLiquidacionItemDto c)
    {
        var faltantes = new List<string>();
        var etiqueta = $"{c.Serie}-{c.Numero}";

        if (string.IsNullOrWhiteSpace(c.CodSapCpm))
            faltantes.Add("no tiene Codigo SAP de factura (CODSAP_CPM)");
        else if (c.CodSapCpm.Trim().Length > MaxDocumento)
            faltantes.Add($"Codigo SAP de factura excede {MaxDocumento} caracteres");

        if (!string.IsNullOrWhiteSpace(c.CodSapDetrac) && c.CodSapDetrac.Trim().Length > MaxDocumento)
            faltantes.Add($"Codigo SAP de detraccion excede {MaxDocumento} caracteres");

        if (c.FechaEmision == null)
            faltantes.Add("no tiene fecha de emision");

        if (MapearTipoComprobante(c.TipoComprobante) == null)
            faltantes.Add($"tipo de comprobante '{c.TipoComprobante}' no soportado (solo 1 y 22)");

        if (string.IsNullOrWhiteSpace(c.Serie) || string.IsNullOrWhiteSpace(c.Numero))
            faltantes.Add("no tiene serie o numero");
        else if (ConstruirReferencia(c).Length > MaxReferencia)
            faltantes.Add($"la referencia '{ConstruirReferencia(c)}' excede {MaxReferencia} caracteres");

        if (c.MtoTotal == null)
            faltantes.Add("no tiene monto total");

        if (c.Moneda != "PEN" && c.Moneda != "USD")
            faltantes.Add($"moneda '{c.Moneda ?? "(vacia)"}' no valida (PEN o USD)");

        return faltantes.Count == 0 ? null : $"{etiqueta}: {string.Join("; ", faltantes)}";
    }

    /// <summary>
    /// Tipo de comprobante interno a codigo SAP: 1 -> 01 (factura), 22 -> 03.
    /// </summary>
    private static string? MapearTipoComprobante(string? tipo) => tipo?.Trim() switch
    {
        "1" => "01",
        "22" => "03",
        _ => null
    };

    /// <summary>
    /// Referencia: {tipo}-{serie}-{numero con ceros a la izquierda hasta 7 digitos}.
    /// </summary>
    private static string ConstruirReferencia(DetalleLiquidacionItemDto c)
    {
        var numero = (c.Numero ?? string.Empty).Trim().TrimStart('0').PadLeft(7, '0');
        return $"{MapearTipoComprobante(c.TipoComprobante)}-{c.Serie?.Trim()}-{numero}";
    }

    private static SapLiberaPagoDocumentoDto ConstruirDocumento(
        DetalleLiquidacionItemDto c, string idOrden, string sociedad, string usuario, DateTime fechaAprobacion)
    {
        return new SapLiberaPagoDocumentoDto
        {
            IdOrden = idOrden,
            Sociedad = sociedad,
            Documento = c.CodSapCpm!.Trim(),
            Ejercicio = c.FechaEmision!.Value.Year.ToString(CultureInfo.InvariantCulture),
            DocumentoDt = c.CodSapDetrac?.Trim() ?? string.Empty,
            Referencia = ConstruirReferencia(c),
            Ruc = Truncar(c.Ruc?.Trim(), MaxRuc) ?? string.Empty,
            ProveedorName = Truncar(c.RazonSocial?.Trim(), MaxProveedor) ?? string.Empty,
            TipoComprobante = MapearTipoComprobante(c.TipoComprobante)!,
            FechaEmision = c.FechaEmision.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Importe = c.MtoTotal!.Value.ToString("0.000", CultureInfo.InvariantCulture),
            Moneda = c.Moneda!,
            UserLiberador = usuario,
            FechaLibera = fechaAprobacion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            HoraLibera = fechaAprobacion.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            Estado = "APROBADO"
        };
    }

    /// <summary>
    /// Marca todos los comprobantes en ERROR cuando falla la validacion previa (no se llama a SAP).
    /// </summary>
    private async Task<(bool, string)> RegistrarFalloValidacionAsync(
        OrdenPago ordenPago,
        List<DetalleLiquidacionItemDto> comprobantes,
        Dictionary<int, string> errores,
        List<string> errorGeneral,
        string? sociedad,
        int idUsuario)
    {
        var general = errorGeneral.Count > 0 ? string.Join("; ", errorGeneral) : null;

        foreach (var c in comprobantes)
        {
            errores.TryGetValue(c.IdProduccion, out var propio);
            var mensaje = propio ?? general ?? "No enviado: otros comprobantes de la orden tienen errores";
            if (propio != null && general != null)
                mensaje = $"{general}. {propio}";

            await _liberacionRepository.SaveResultadoAsync(new OrdenPagoLiberacion
            {
                IdOrdenPago = ordenPago.IdOrdenPago,
                IdProduccion = c.IdProduccion,
                Sociedad = sociedad,
                Documento = c.CodSapCpm?.Trim(),
                Ejercicio = c.FechaEmision?.Year.ToString(CultureInfo.InvariantCulture),
                DocumentoDt = string.IsNullOrWhiteSpace(c.CodSapDetrac) ? null : c.CodSapDetrac.Trim(),
                EstadoLiberacion = EstadoLiberacion.Error,
                MensajeSap = Truncar(mensaje, MaxMensaje)
            }, 0, idUsuario);
        }

        var resumen = string.Join(" | ", new[] { general }
            .Concat(errores.Values)
            .Where(m => !string.IsNullOrEmpty(m)));

        await RegistrarBitacoraAsync(ordenPago, idUsuario, false, "Validacion previa: " + resumen);

        _logger.LogWarning("Liberacion SAP no enviada, validacion fallida. Orden {NumeroOP}: {Detalle}",
            ordenPago.NumeroOrdenPago, resumen);

        return (false, "No se envio a SAP: " + resumen);
    }

    private async Task RegistrarBitacoraAsync(OrdenPago ordenPago, int idUsuario, bool exito, string detalle)
    {
        try
        {
            await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
            {
                Entidad = "SHM_ORDEN_PAGO",
                IdEntidad = ordenPago.IdOrdenPago,
                Accion = exito ? "LIBERACION_SAP" : "LIBERACION_SAP_ERROR",
                Descripcion = Truncar($"Orden {ordenPago.NumeroOrdenPago}: {detalle}", 500),
                FechaAccion = DateTime.Now
            }, idUsuario);
        }
        catch (Exception ex)
        {
            // La bitacora no debe afectar el resultado de la liberacion
            _logger.LogError(ex, "No se pudo registrar la bitacora de liberacion SAP. Orden {IdOrdenPago}", ordenPago.IdOrdenPago);
        }
    }

    private static string? Truncar(string? valor, int max)
        => valor != null && valor.Length > max ? valor[..max] : valor;
}
