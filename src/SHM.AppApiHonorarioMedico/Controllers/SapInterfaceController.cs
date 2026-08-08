using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Banco;
using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.DTOs.Common;
using SHM.AppDomain.DTOs.SapApi;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using System.Globalization;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la sincronizacion de datos desde SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-07</created>
/// <modified>ADG Antonio - 2026-04-15 - Agregado endpoint estado-pago</modified>
/// <modified>ADG Antonio - 2026-07-18 - Agregado endpoint actualizar-estado-pago-masivo</modified>
/// <modified>ADG Antonio - 2026-07-21 - Registro en bitacora al actualizar estado de pago</modified>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SapInterfaceController : ControllerBase
{
    /// <summary>
    /// Id de usuario "sistema" usado como creador de los registros de bitacora
    /// generados por procesos automaticos (tarea programada / interfaz SAP).
    /// </summary>
    private const int IdCreadorSistema = 1;

    private readonly ISapApiService _sapApiService;
    private readonly IBancoService _bancoService;
    private readonly IOrdenPagoProduccionRepository _ordenPagoProduccionRepository;
    private readonly IProduccionService _produccionService;
    private readonly IBitacoraService _bitacoraService;
    private readonly ILogger<SapInterfaceController> _logger;

    public SapInterfaceController(
        ISapApiService sapApiService,
        IBancoService bancoService,
        IOrdenPagoProduccionRepository ordenPagoProduccionRepository,
        IProduccionService produccionService,
        IBitacoraService bitacoraService,
        ILogger<SapInterfaceController> logger)
    {
        _sapApiService = sapApiService;
        _bancoService = bancoService;
        _ordenPagoProduccionRepository = ordenPagoProduccionRepository;
        _produccionService = produccionService;
        _bitacoraService = bitacoraService;
        _logger = logger;
    }

    /// <summary>
    /// Registra en bitacora la actualizacion del estado de pago de una produccion.
    /// </summary>
    private async Task RegistrarBitacoraEstadoPagoAsync(
        int idProduccion, string? tipoComprobante, string? serie, string? numero, SapDatosFacturaDto datos)
    {
        await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
        {
            Entidad     = "SHM_PRODUCCION",
            IdEntidad   = idProduccion,
            Accion      = "ACTUALIZAR_ESTADO_PAGO",
            Descripcion = $"Estado de pago actualizado desde SAP. Comprobante {tipoComprobante} {serie}-{numero}: " +
                          $"EstadoPago={datos.EstadoPago}, FechaPago={datos.FechaPago}, Monto={datos.MontoPagado}, " +
                          $"Operacion={datos.NumeroOperacion}, Banco={datos.Banco}",
            FechaAccion = DateTime.Now
        }, IdCreadorSistema);
    }

    /// <summary>
    /// Sincroniza los bancos desde SAP (COD_BANCOSet).
    /// Obtiene la lista de bancos de SAP y los inserta en SHM_BANCO si no existen.
    /// </summary>
    [HttpPost("sincronizar-bancos")]
    [ProducesResponseType(typeof(ApiResponseDto<SincronizarBancosResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<SincronizarBancosResultDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<SincronizarBancosResultDto>>> SincronizarBancos()
    {
        try
        {
            _logger.LogInformation("Inicio de sincronizacion de bancos desde SAP");

            var bancosSap = await _sapApiService.GetBancosAsync();

            if (bancosSap == null || bancosSap.Count == 0)
            {
                _logger.LogWarning("No se obtuvieron bancos desde SAP");
                return Ok(ApiResponseDto<SincronizarBancosResultDto>.Error(
                    "No se obtuvieron bancos desde SAP."));
            }

            _logger.LogInformation("Se obtuvieron {Count} bancos desde SAP", bancosSap.Count);

            var resultado = new SincronizarBancosResultDto();
            const int idCreador = 1;

            foreach (var bancoSap in bancosSap)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(bancoSap.CodigoBanco))
                    {
                        resultado.CantidadErrores++;
                        resultado.Detalle.Add(new SincronizarBancoDetalleDto
                        {
                            CodigoBanco = "(vacio)",
                            Estado = "ER",
                            Mensaje = "Codigo de banco vacio"
                        });
                        continue;
                    }

                    // Verificar si ya existe
                    var bancoExistente = await _bancoService.GetBancoByCodigoAsync(bancoSap.CodigoBanco);

                    if (bancoExistente != null)
                    {
                        resultado.CantidadObviados++;
                        resultado.Detalle.Add(new SincronizarBancoDetalleDto
                        {
                            CodigoBanco = bancoSap.CodigoBanco,
                            NombreBanco = bancoSap.DescripcionBanco,
                            Estado = "OK",
                            Mensaje = "Ya existe"
                        });
                        continue;
                    }

                    // Insertar nuevo banco
                    var createDto = new CreateBancoDto
                    {
                        CodigoBanco = bancoSap.CodigoBanco,
                        NombreBanco = bancoSap.DescripcionBanco ?? string.Empty
                    };

                    await _bancoService.CreateBancoAsync(createDto, idCreador);

                    resultado.CantidadCreados++;
                    resultado.Detalle.Add(new SincronizarBancoDetalleDto
                    {
                        CodigoBanco = bancoSap.CodigoBanco,
                        NombreBanco = bancoSap.DescripcionBanco,
                        Estado = "OK",
                        Mensaje = "Creado"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al sincronizar banco {CodigoBanco}", bancoSap.CodigoBanco);
                    resultado.CantidadErrores++;
                    resultado.Detalle.Add(new SincronizarBancoDetalleDto
                    {
                        CodigoBanco = bancoSap.CodigoBanco,
                        NombreBanco = bancoSap.DescripcionBanco,
                        Estado = "ER",
                        Mensaje = ex.Message
                    });
                }
            }

            resultado.TotalProcesados = bancosSap.Count;

            _logger.LogInformation(
                "Sincronizacion de bancos finalizada. Total: {Total}, Creados: {Creados}, Existentes: {Existentes}, Errores: {Errores}",
                resultado.TotalProcesados, resultado.CantidadCreados, resultado.CantidadObviados, resultado.CantidadErrores);

            return Ok(ApiResponseDto<SincronizarBancosResultDto>.Success(resultado, "Sincronizacion completada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar bancos desde SAP");
            return StatusCode(500, ApiResponseDto<SincronizarBancosResultDto>.Error(
                "Error interno del servidor.", ex.Message));
        }
    }

    /// <summary>
    /// Obtiene las cuentas bancarias de un acreedor desde SAP (CTA_ACREEDORSet).
    /// </summary>
    /// <param name="codAcreedor">Codigo del acreedor en SAP.</param>
    [HttpGet("cuentas-bancarias-acreedor/{codAcreedor}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>>> GetCuentasBancariasByAcreedor(string codAcreedor)
    {
        try
        {
            _logger.LogInformation("Consultando cuentas bancarias del acreedor {CodAcreedor} en SAP", codAcreedor);

            var cuentas = await _sapApiService.GetCuentasBancariasByAcreedorAsync(codAcreedor);

            if (cuentas == null || cuentas.Count == 0)
            {
                _logger.LogWarning("No se encontraron cuentas bancarias para el acreedor {CodAcreedor}", codAcreedor);
                return NotFound(ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>.Error(
                    $"No se encontraron cuentas bancarias para el acreedor '{codAcreedor}'."));
            }

            _logger.LogInformation("Se obtuvieron {Count} cuentas bancarias del acreedor {CodAcreedor}",
                cuentas.Count, codAcreedor);

            return Ok(ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>.Success(
                cuentas, $"Se encontraron {cuentas.Count} cuenta(s) bancaria(s)."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar cuentas bancarias del acreedor {CodAcreedor}", codAcreedor);
            return StatusCode(500, ApiResponseDto<List<SapAcreedorCuentaBancariaDto>>.Error(
                "Error interno del servidor.", ex.Message));
        }
    }

    /// <summary>
    /// Obtiene el estado de pago de un comprobante desde SAP (DatosFacturaSet).
    /// Transforma los parametros del sistema interno al formato SAP antes de consultar.
    /// </summary>
    /// <param name="codigoAcreedor">Codigo del acreedor en SAP (ej: 3000000305).</param>
    /// <param name="tipoComprobante">Tipo de comprobante en codigo interno: "1" (Factura) o "22" (RHE).</param>
    /// <param name="serie">Serie del comprobante sin prefijo (ej: E001). Se agrega "0" al inicio.</param>
    /// <param name="numero">Numero del comprobante. Se completa con ceros a la izquierda hasta 7 caracteres.</param>
    /// <param name="fechaEmision">Fecha de emision del comprobante. Se extrae el año para el parametro Anio.</param>
    [HttpGet("estado-pago/{codigoAcreedor}/{tipoComprobante}/{serie}/{numero}/{fechaEmision}")]
    [ProducesResponseType(typeof(ApiResponseDto<SapDatosFacturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<SapDatosFacturaDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<SapDatosFacturaDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<SapDatosFacturaDto>>> GetEstadoPago(
        string codigoAcreedor,
        string tipoComprobante,
        string serie,
        string numero,
        DateTime fechaEmision)
    {
        try
        {
            _logger.LogInformation(
                "Consultando estado de pago en SAP. Acreedor: {Acreedor}, Tipo: {Tipo}, Serie: {Serie}, Numero: {Numero}, FechaEmision: {FechaEmision}",
                codigoAcreedor, tipoComprobante, serie, numero, fechaEmision);

            var datos = await _sapApiService.GetDatosFacturaAsync(
                codigoAcreedor, tipoComprobante, serie, numero, fechaEmision);

            if (datos == null)
            {
                _logger.LogWarning(
                    "No se encontraron datos en SAP para el comprobante. Acreedor: {Acreedor}, Serie: {Serie}, Numero: {Numero}",
                    codigoAcreedor, serie, numero);
                return NotFound(ApiResponseDto<SapDatosFacturaDto>.Error(
                    "No se encontraron datos para el comprobante indicado."));
            }

            return Ok(ApiResponseDto<SapDatosFacturaDto>.Success(datos, "Datos obtenidos correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al consultar estado de pago en SAP. Acreedor: {Acreedor}, Serie: {Serie}, Numero: {Numero}",
                codigoAcreedor, serie, numero);
            return StatusCode(500, ApiResponseDto<SapDatosFacturaDto>.Error(
                "Error interno del servidor.", ex.Message));
        }
    }

    /// <summary>
    /// Obtiene el estado de pago de todos los comprobantes que componen una orden de pago.
    /// Consulta SAP de forma secuencial por cada comprobante con Serie, Numero y FechaEmision registrados.
    /// Los comprobantes sin datos suficientes (sin serie/numero/fecha) se marcan como SIN_COMPROBANTE.
    /// Los comprobantes sin CodigoAcreedor en la entidad medica se marcan como SIN_ACREEDOR.
    /// </summary>
    /// <param name="guidOrdenPago">GUID de la orden de pago.</param>
    [HttpGet("estado-pago-orden/{guidOrdenPago}")]
    [ProducesResponseType(typeof(ApiResponseDto<SapEstadoPagoOrdenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<SapEstadoPagoOrdenDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<SapEstadoPagoOrdenDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<SapEstadoPagoOrdenDto>>> GetEstadoPagoOrden(string guidOrdenPago)
    {
        try
        {
            _logger.LogInformation("Consultando estado de pago de orden {GuidOrdenPago} en SAP", guidOrdenPago);

            var comprobantes = (await _ordenPagoProduccionRepository
                .GetComprobantesParaSapByOrdenPagoGuidAsync(guidOrdenPago)).ToList();

            if (comprobantes.Count == 0)
            {
                return NotFound(ApiResponseDto<SapEstadoPagoOrdenDto>.Error(
                    $"No se encontro la orden de pago o no tiene producciones activas: {guidOrdenPago}"));
            }

            var resultado = new SapEstadoPagoOrdenDto
            {
                GuidOrdenPago    = guidOrdenPago,
                TotalComprobantes = comprobantes.Count
            };

            foreach (var comp in comprobantes)
            {
                var item = new SapEstadoPagoItemDto
                {
                    IdProduccion   = comp.IdProduccion,
                    GuidProduccion = comp.GuidProduccion,
                    TipoComprobante = comp.TipoComprobante,
                    Serie          = comp.Serie,
                    Numero         = comp.Numero,
                    RazonSocial    = comp.RazonSocial,
                    Ruc            = comp.Ruc
                };

                // Validar datos minimos para poder consultar SAP
                if (string.IsNullOrWhiteSpace(comp.CodigoAcreedor))
                {
                    item.Estado  = "SIN_ACREEDOR";
                    item.Mensaje = "La entidad medica no tiene CodigoAcreedor registrado";
                    resultado.Errores++;
                    resultado.Items.Add(item);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(comp.Serie) ||
                    string.IsNullOrWhiteSpace(comp.Numero) ||
                    comp.FechaEmision == null ||
                    string.IsNullOrWhiteSpace(comp.TipoComprobante))
                {
                    item.Estado  = "SIN_COMPROBANTE";
                    item.Mensaje = "La produccion no tiene comprobante emitido (Serie/Numero/FechaEmision/TipoComprobante)";
                    resultado.SinComprobante++;
                    resultado.Items.Add(item);
                    continue;
                }

                // Consulta secuencial a SAP
                var datos = await _sapApiService.GetDatosFacturaAsync(
                    comp.CodigoAcreedor,
                    comp.TipoComprobante,
                    comp.Serie,
                    comp.Numero,
                    comp.FechaEmision.Value);

                if (datos != null)
                {
                    item.Estado          = "OK";
                    item.EstadoPago      = datos.EstadoPago;
                    item.FechaPago       = datos.FechaPago;
                    item.MontoPagado     = datos.MontoPagado;
                    item.NumeroOperacion = datos.NumeroOperacion;
                    item.Banco           = datos.Banco;
                    item.CtaBanDeposito  = datos.CtaBanDeposito;
                    resultado.ConsultadosEnSap++;

                    // Parsear FechaPago y MontoPagado para persistir en DB
                    DateTime? pagoFecha = DateTime.TryParse(datos.FechaPago, out var fechaParsed)
                        ? fechaParsed : null;
                    decimal? pagoMonto = decimal.TryParse(
                        datos.MontoPagado,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var montoParsed)
                        ? montoParsed : null;

                    var actualizado = await _produccionService.UpdateEstadoPagoAsync(
                        idProduccion:        comp.IdProduccion,
                        pagoEstado:          datos.EstadoPago,
                        pagoFecha:           pagoFecha,
                        pagoNumeroOperacion: datos.NumeroOperacion,
                        pagoBanco:           datos.Banco,
                        pagoCuentaDeposito:  datos.CtaBanDeposito,
                        pagoMontoPagado:     pagoMonto,
                        idModificador:       IdCreadorSistema);

                    if (actualizado)
                    {
                        await RegistrarBitacoraEstadoPagoAsync(
                            comp.IdProduccion, comp.TipoComprobante, comp.Serie, comp.Numero, datos);
                    }
                }
                else
                {
                    item.Estado  = "ERROR_SAP";
                    item.Mensaje = "No se encontraron datos en SAP para este comprobante";
                    resultado.Errores++;
                }

                resultado.Items.Add(item);
            }

            _logger.LogInformation(
                "Estado de pago de orden {GuidOrdenPago} procesado. Total: {Total}, SAP: {Sap}, SinComprobante: {Sin}, Errores: {Err}",
                guidOrdenPago, resultado.TotalComprobantes, resultado.ConsultadosEnSap,
                resultado.SinComprobante, resultado.Errores);

            return Ok(ApiResponseDto<SapEstadoPagoOrdenDto>.Success(resultado, "Consulta completada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado de pago de orden {GuidOrdenPago}", guidOrdenPago);
            return StatusCode(500, ApiResponseDto<SapEstadoPagoOrdenDto>.Error(
                "Error interno del servidor.", ex.Message));
        }
    }

    /// <summary>
    /// Procesa y actualiza el estado de pago de todos los comprobantes pendientes:
    /// producciones de ordenes de pago con ESTADO = APROBADO cuyo PAGO_ESTADO aun es nulo.
    /// Consulta SAP secuencialmente por cada comprobante y persiste el resultado en SHM_PRODUCCION.
    /// Los comprobantes sin datos suficientes se marcan como SIN_COMPROBANTE y los que no
    /// tienen CodigoAcreedor en la entidad medica se marcan como SIN_ACREEDOR; ninguno de los
    /// dos casos detiene el proceso del resto de comprobantes.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-07-18</created>
    /// </summary>
    [HttpPost("actualizar-estado-pago-masivo")]
    [ProducesResponseType(typeof(ApiResponseDto<SapActualizarEstadoPagoMasivoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<SapActualizarEstadoPagoMasivoDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<SapActualizarEstadoPagoMasivoDto>>> ActualizarEstadoPagoMasivo()
    {
        try
        {
            _logger.LogInformation("Inicio de actualizacion masiva de estado de pago");

            var comprobantes = (await _ordenPagoProduccionRepository
                .GetComprobantesPendientesPagoAsync()).ToList();

            var resultado = new SapActualizarEstadoPagoMasivoDto
            {
                TotalComprobantes = comprobantes.Count
            };

            var ordenesPorId = comprobantes
                .GroupBy(c => new { c.IdOrdenPago, c.GuidOrdenPago, c.NumeroOrdenPago });

            foreach (var grupoOrden in ordenesPorId)
            {
                var ordenResultado = new SapEstadoPagoOrdenDto
                {
                    GuidOrdenPago      = grupoOrden.Key.GuidOrdenPago,
                    TotalComprobantes  = grupoOrden.Count()
                };

                foreach (var comp in grupoOrden)
                {
                    var item = new SapEstadoPagoItemDto
                    {
                        IdProduccion    = comp.IdProduccion,
                        GuidProduccion  = comp.GuidProduccion,
                        TipoComprobante = comp.TipoComprobante,
                        Serie           = comp.Serie,
                        Numero          = comp.Numero,
                        RazonSocial     = comp.RazonSocial,
                        Ruc             = comp.Ruc
                    };

                    if (string.IsNullOrWhiteSpace(comp.CodigoAcreedor))
                    {
                        item.Estado  = "SIN_ACREEDOR";
                        item.Mensaje = "La entidad medica no tiene CodigoAcreedor registrado";
                        ordenResultado.Errores++;
                        resultado.Errores++;
                        ordenResultado.Items.Add(item);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(comp.Serie) ||
                        string.IsNullOrWhiteSpace(comp.Numero) ||
                        comp.FechaEmision == null ||
                        string.IsNullOrWhiteSpace(comp.TipoComprobante))
                    {
                        item.Estado  = "SIN_COMPROBANTE";
                        item.Mensaje = "La produccion no tiene comprobante emitido (Serie/Numero/FechaEmision/TipoComprobante)";
                        ordenResultado.SinComprobante++;
                        resultado.SinComprobante++;
                        ordenResultado.Items.Add(item);
                        continue;
                    }

                    var datos = await _sapApiService.GetDatosFacturaAsync(
                        comp.CodigoAcreedor,
                        comp.TipoComprobante,
                        comp.Serie,
                        comp.Numero,
                        comp.FechaEmision.Value);

                    if (datos != null)
                    {
                        item.Estado          = "OK";
                        item.EstadoPago      = datos.EstadoPago;
                        item.FechaPago       = datos.FechaPago;
                        item.MontoPagado     = datos.MontoPagado;
                        item.NumeroOperacion = datos.NumeroOperacion;
                        item.Banco           = datos.Banco;
                        item.CtaBanDeposito  = datos.CtaBanDeposito;
                        ordenResultado.ConsultadosEnSap++;
                        resultado.ConsultadosEnSap++;

                        DateTime? pagoFecha = DateTime.TryParse(datos.FechaPago, out var fechaParsed)
                            ? fechaParsed : null;
                        decimal? pagoMonto = decimal.TryParse(
                            datos.MontoPagado,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var montoParsed)
                            ? montoParsed : null;

                        var actualizado = await _produccionService.UpdateEstadoPagoAsync(
                            idProduccion:        comp.IdProduccion,
                            pagoEstado:          datos.EstadoPago,
                            pagoFecha:           pagoFecha,
                            pagoNumeroOperacion: datos.NumeroOperacion,
                            pagoBanco:           datos.Banco,
                            pagoCuentaDeposito:  datos.CtaBanDeposito,
                            pagoMontoPagado:     pagoMonto,
                            idModificador:       IdCreadorSistema);

                        if (actualizado)
                        {
                            resultado.Actualizados++;
                            await RegistrarBitacoraEstadoPagoAsync(
                                comp.IdProduccion, comp.TipoComprobante, comp.Serie, comp.Numero, datos);
                        }
                    }
                    else
                    {
                        item.Estado  = "ERROR_SAP";
                        item.Mensaje = "No se encontraron datos en SAP para este comprobante";
                        ordenResultado.Errores++;
                        resultado.Errores++;
                    }

                    ordenResultado.Items.Add(item);
                }

                resultado.Ordenes.Add(ordenResultado);
            }

            resultado.TotalOrdenes = resultado.Ordenes.Count;

            _logger.LogInformation(
                "Actualizacion masiva de estado de pago finalizada. Ordenes: {Ordenes}, Comprobantes: {Total}, Actualizados: {Actualizados}, SinComprobante: {Sin}, Errores: {Err}",
                resultado.TotalOrdenes, resultado.TotalComprobantes, resultado.Actualizados,
                resultado.SinComprobante, resultado.Errores);

            return Ok(ApiResponseDto<SapActualizarEstadoPagoMasivoDto>.Success(resultado, "Proceso completado."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de pago masivo");
            return StatusCode(500, ApiResponseDto<SapActualizarEstadoPagoMasivoDto>.Error(
                "Error interno del servidor.", ex.Message));
        }
    }
}

/// <summary>
/// DTO con el resultado de la sincronizacion de bancos.
/// </summary>
public class SincronizarBancosResultDto
{
    public int TotalProcesados { get; set; }
    public int CantidadCreados { get; set; }
    public int CantidadObviados { get; set; }
    public int CantidadErrores { get; set; }
    public List<SincronizarBancoDetalleDto> Detalle { get; set; } = new();
}

/// <summary>
/// DTO con el detalle de cada banco procesado.
/// </summary>
public class SincronizarBancoDetalleDto
{
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
