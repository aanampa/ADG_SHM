using Microsoft.Extensions.Logging;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de producciones medicas, incluyendo operaciones CRUD y consultas por sede, entidad medica y periodo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-20 - Agregado metodo de listado paginado con filtros</modified>
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos de fechas de factura</modified>
/// </summary>
public class ProduccionService : IProduccionService
{
    private readonly IProduccionRepository _produccionRepository;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<ProduccionService> _logger;

    public ProduccionService(
        IProduccionRepository produccionRepository,
        IEmailService emailService,
        IUsuarioRepository usuarioRepository,
        ILogger<ProduccionService> logger)
    {
        _produccionRepository = produccionRepository;
        _emailService = emailService;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las producciones del sistema
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetAllProduccionesAsync()
    {
        var producciones = await _produccionRepository.GetAllAsync();
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una produccion por su identificador unico
    /// </summary>
    public async Task<ProduccionResponseDto?> GetProduccionByIdAsync(int id)
    {
        var produccion = await _produccionRepository.GetByIdAsync(id);
        return produccion != null ? MapToResponseDto(produccion) : null;
    }

    /// <summary>
    /// Obtiene una produccion por su codigo
    /// </summary>
    public async Task<ProduccionResponseDto?> GetProduccionByCodigoAsync(string codigo)
    {
        var produccion = await _produccionRepository.GetByCodigoAsync(codigo);
        return produccion != null ? MapToResponseDto(produccion) : null;
    }

    /// <summary>
    /// Obtiene una produccion por su GUID de registro con datos relacionados
    ///
    /// <modified>ADG Vladimir D - 2025-01-21 - Cambiado para devolver ProduccionListaResponseDto con JOINs</modified>
    /// </summary>
    public async Task<ProduccionListaResponseDto?> GetProduccionByGuidAsync(string guidRegistro)
    {
        return await _produccionRepository.GetByGuidWithDetailsAsync(guidRegistro);
    }

    /// <summary>
    /// Obtiene las producciones de una sede especifica
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesBySedeAsync(int idSede)
    {
        var producciones = await _produccionRepository.GetBySedeAsync(idSede);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene las producciones de una entidad medica especifica
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByEntidadMedicaAsync(int idEntidadMedica)
    {
        var producciones = await _produccionRepository.GetByEntidadMedicaAsync(idEntidadMedica);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene producciones de una entidad medica filtradas por estado comprobante en Oracle.
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByEntidadMedicaYEstadoComprobanteAsync(int idEntidadMedica, string estadoComprobante)
    {
        var producciones = await _produccionRepository.GetByEntidadMedicaYEstadoComprobanteAsync(idEntidadMedica, estadoComprobante);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene las producciones de un periodo especifico
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByPeriodoAsync(string periodo)
    {
        var producciones = await _produccionRepository.GetByPeriodoAsync(periodo);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva produccion en el sistema
    /// </summary>
    public async Task<ProduccionResponseDto> CreateProduccionAsync(CreateProduccionDto createDto, int idCreador)
    {
        var produccion = new Produccion
        {
            IdSede = createDto.IdSede,
            IdEntidadMedica = createDto.IdEntidadMedica,
            CodigoProduccion = createDto.CodigoProduccion,
            TipoProduccion = createDto.TipoProduccion,
            TipoMedico = createDto.TipoMedico,
            TipoRubro = createDto.TipoRubro,
            Descripcion = createDto.Descripcion,
            Periodo = createDto.Periodo,
            EstadoProduccion = createDto.EstadoProduccion,
            MtoConsumo = createDto.MtoConsumo,
            MtoDescuento = createDto.MtoDescuento,
            MtoSubtotal = createDto.MtoSubtotal,
            MtoRenta = createDto.MtoRenta,
            MtoIgv = createDto.MtoIgv,
            MtoTotal = createDto.MtoTotal,
            MtoDetraccion = createDto.MtoDetraccion,
            PorcDetraccion = createDto.PorcDetraccion,
            TipoComprobante = createDto.TipoComprobante,
            Serie = createDto.Serie,
            Numero = createDto.Numero,
            FechaEmision = createDto.FechaEmision,
            Glosa = createDto.Glosa,
            EstadoComprobante = createDto.EstadoComprobante,
            Concepto = createDto.Concepto,
            FechaLimite = createDto.FechaLimite,
            Estado = createDto.Estado,
            FacturaFechaSolicitud = createDto.FacturaFechaSolicitud,
            FacturaFechaEnvio = createDto.FacturaFechaEnvio,
            FacturaFechaAceptacion = createDto.FacturaFechaAceptacion,
            FacturaFechaPago = createDto.FacturaFechaPago,
            FacturaFechaVencimiento = createDto.FacturaFechaVencimiento,
            IdCreador = idCreador,
            Activo = 1
        };

        var idProduccion = await _produccionRepository.CreateAsync(produccion);
        var createdProduccion = await _produccionRepository.GetByIdAsync(idProduccion);

        return MapToResponseDto(createdProduccion!);
    }

    /// <summary>
    /// Actualiza los datos de una produccion existente
    /// </summary>
    public async Task<bool> UpdateProduccionAsync(int id, UpdateProduccionDto updateDto, int idModificador)
    {
        var produccionExistente = await _produccionRepository.GetByIdAsync(id);
        if (produccionExistente == null)
            return false;

        if (updateDto.IdSede.HasValue)
            produccionExistente.IdSede = updateDto.IdSede.Value;

        if (updateDto.IdEntidadMedica.HasValue)
            produccionExistente.IdEntidadMedica = updateDto.IdEntidadMedica.Value;

        if (updateDto.IdCuentaBanco.HasValue)
            produccionExistente.IdCuentaBanco = updateDto.IdCuentaBanco.Value;

        if (updateDto.CodigoProduccion != null)
            produccionExistente.CodigoProduccion = updateDto.CodigoProduccion;

        if (updateDto.TipoProduccion != null)
            produccionExistente.TipoProduccion = updateDto.TipoProduccion;

        if (updateDto.TipoMedico != null)
            produccionExistente.TipoMedico = updateDto.TipoMedico;

        if (updateDto.TipoRubro != null)
            produccionExistente.TipoRubro = updateDto.TipoRubro;

        if (updateDto.Descripcion != null)
            produccionExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Periodo != null)
            produccionExistente.Periodo = updateDto.Periodo;

        if (updateDto.EstadoProduccion != null)
            produccionExistente.EstadoProduccion = updateDto.EstadoProduccion;

        if (updateDto.MtoConsumo.HasValue)
            produccionExistente.MtoConsumo = updateDto.MtoConsumo;

        if (updateDto.MtoDescuento.HasValue)
            produccionExistente.MtoDescuento = updateDto.MtoDescuento;

        if (updateDto.MtoSubtotal.HasValue)
            produccionExistente.MtoSubtotal = updateDto.MtoSubtotal;

        if (updateDto.MtoRenta.HasValue)
            produccionExistente.MtoRenta = updateDto.MtoRenta;

        if (updateDto.MtoIgv.HasValue)
            produccionExistente.MtoIgv = updateDto.MtoIgv;

        if (updateDto.MtoTotal.HasValue)
            produccionExistente.MtoTotal = updateDto.MtoTotal;

        if (updateDto.MtoDetraccion.HasValue)
            produccionExistente.MtoDetraccion = updateDto.MtoDetraccion;

        if (updateDto.PorcDetraccion.HasValue)
            produccionExistente.PorcDetraccion = updateDto.PorcDetraccion;

        if (updateDto.TipoComprobante != null)
            produccionExistente.TipoComprobante = updateDto.TipoComprobante;

        if (updateDto.Serie != null)
            produccionExistente.Serie = updateDto.Serie;

        if (updateDto.Numero != null)
            produccionExistente.Numero = updateDto.Numero;

        if (updateDto.FechaEmision.HasValue)
            produccionExistente.FechaEmision = updateDto.FechaEmision;

        if (updateDto.Glosa != null)
            produccionExistente.Glosa = updateDto.Glosa;

        if (updateDto.EstadoComprobante != null)
            produccionExistente.EstadoComprobante = updateDto.EstadoComprobante;

        if (updateDto.Concepto != null)
            produccionExistente.Concepto = updateDto.Concepto;

        if (updateDto.FechaLimite.HasValue)
            produccionExistente.FechaLimite = updateDto.FechaLimite;

        if (updateDto.Estado != null)
            produccionExistente.Estado = updateDto.Estado;

        if (updateDto.FacturaFechaSolicitud.HasValue)
            produccionExistente.FacturaFechaSolicitud = updateDto.FacturaFechaSolicitud;

        if (updateDto.FacturaFechaEnvio.HasValue)
            produccionExistente.FacturaFechaEnvio = updateDto.FacturaFechaEnvio;

        if (updateDto.FacturaFechaAceptacion.HasValue)
            produccionExistente.FacturaFechaAceptacion = updateDto.FacturaFechaAceptacion;

        if (updateDto.FacturaFechaPago.HasValue)
            produccionExistente.FacturaFechaPago = updateDto.FacturaFechaPago;

        if (updateDto.FacturaFechaVencimiento.HasValue)
            produccionExistente.FacturaFechaVencimiento = updateDto.FacturaFechaVencimiento;

        if (updateDto.TipoLiquidacion != null)
            produccionExistente.TipoLiquidacion = updateDto.TipoLiquidacion;

        if (updateDto.Activo.HasValue)
            produccionExistente.Activo = updateDto.Activo.Value;

        produccionExistente.IdModificador = idModificador;

        return await _produccionRepository.UpdateAsync(id, produccionExistente);
    }

    /// <summary>
    /// Elimina una produccion por su identificador
    /// </summary>
    public async Task<bool> DeleteProduccionAsync(int id, int idModificador)
    {
        var exists = await _produccionRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _produccionRepository.DeleteAsync(id, idModificador);
    }

    /// <summary>
    /// Revierte los datos de comprobante de una produccion al estado indicado.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-03-01</created>
    public async Task<bool> RevertComprobanteAsync(int idProduccion, string estado, int idModificador)
    {
        return await _produccionRepository.RevertComprobanteByIdAsync(idProduccion, estado, idModificador);
    }

    /// <summary>
    /// Obtiene el listado paginado de producciones con datos relacionados y filtros.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// <modified>ADG Vladimir D - 2026-01-24 - Agregado filtro por codigo de produccion</modified>
    /// <modified>ADG Vladimir D - 2026-01-24 - Agregado filtro por Cia Medica</modified>
    /// <modified>ADG Vladimir D - 2026-02-02 - Agregado filtro por IdSede del usuario logueado</modified>
    /// </summary>
    public async Task<(IEnumerable<ProduccionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        string? produccion, string? estado, int? idEntidadMedica, int? idSede, int pageNumber, int pageSize)
    {
        return await _produccionRepository.GetPaginatedListAsync(produccion, estado, idEntidadMedica, idSede, pageNumber, pageSize);
    }

    public async Task<IEnumerable<ProduccionListaResponseDto>> GetListSolicitudMasivaAsync(int? idSede)
    {
        return await _produccionRepository.GetListSolicitudMasivaAsync(idSede);
    }

    public async Task<ProduccionListaResponseDto?> GetSolicitudMasivaByGuidAsync(string guidRegistro)
    {
        return await _produccionRepository.GetSolicitudMasivaByGuidAsync(guidRegistro);
    }

    public async Task<bool> GrabarFechasProduccionAsync(string guidRegistro, DateTime fechaLimite, DateTime fechaVencimiento, int idModificador)
    {
        return await _produccionRepository.UpdateFechasProduccionAsync(guidRegistro, fechaLimite, fechaVencimiento, idModificador);
    }

    /// <summary>
    /// Solicita factura actualizando la fecha limite y cambiando el estado a FACTURA_SOLICITADA.
    /// Envia notificacion por correo a los usuarios de la Cia Medica asociada.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-21</created>
    /// <modified>ADG Vladimir D - 2026-01-26 - Agregado envio de notificacion por correo a Cia Medica</modified>
    /// </summary>
    public async Task<bool> SolicitarFacturaAsync(SolicitarFacturaDto solicitudDto, int idModificador)
    {
        // Combinar fecha y hora en un DateTime
        if (!DateTime.TryParse($"{solicitudDto.Fecha} {solicitudDto.Hora}", out DateTime fechaLimite))
        {
            return false;
        }

        const string nuevoEstado = EstadoDescripcion.Produccion.FacturaSolicitada;

        DateTime? fechaVencimiento = null;
        if (!string.IsNullOrEmpty(solicitudDto.FechaVencimiento) &&
            DateTime.TryParse(solicitudDto.FechaVencimiento, out var fv))
        {
            fechaVencimiento = fv;
        }

        var resultado = await _produccionRepository.UpdateFechaLimiteEstadoAsync(
            solicitudDto.GuidRegistro,
            fechaLimite,
            nuevoEstado,
            idModificador,
            fechaVencimiento);

        // Enviar notificacion por correo a la Cia Medica
        if (resultado)
        {
            try
            {
                var produccion = await _produccionRepository.GetByGuidWithDetailsAsync(solicitudDto.GuidRegistro);
                if (produccion != null && produccion.IdEntidadMedica.HasValue)
                {
                    var usuarios = await _usuarioRepository.GetByIdEntidadMedicaAsync(produccion.IdEntidadMedica.Value);
                    foreach (var usuario in usuarios)
                    {
                        if (!string.IsNullOrEmpty(usuario.Email))
                        {
                            var nombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();

                            await _emailService.EnviarEmailSolicitudFacturaAsync(
                                email:              usuario.Email,
                                nombreDestinatario: nombreCompleto,
                                codigoProduccion:   produccion.NumeroProduccion ?? "",
                                razonSocial:        produccion.RazonSocial ?? "",
                                mtoTotal:           produccion.MtoTotal,
                                fechaLimite:        fechaLimite,
                                idEntidadMedica:    produccion.IdEntidadMedica,
                                idProduccion:       produccion.IdProduccion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificacion de solicitud de factura. GUID: {Guid}", solicitudDto.GuidRegistro);
            }
        }

        return resultado;
    }

    /// <summary>
    /// Renotifica la solicitud de factura reenviando el correo con la fecha limite ya establecida.
    /// No modifica el estado ni la fecha limite. Solo aplica a registros en FACTURA_SOLICITADA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-17</created>
    /// </summary>
    public async Task<bool> RenotificarFacturaAsync(string guidRegistro, int idModificador)
    {
        var produccion = await _produccionRepository.GetByGuidWithDetailsAsync(guidRegistro);

        if (produccion == null
            || (produccion.Estado != EstadoDescripcion.Produccion.FacturaSolicitada &&
                produccion.Estado != EstadoDescripcion.Produccion.FacturaDevuelta)
            || !produccion.FechaLimite.HasValue
            || !produccion.IdEntidadMedica.HasValue)
        {
            return false;
        }

        var usuarios = await _usuarioRepository.GetByIdEntidadMedicaAsync(produccion.IdEntidadMedica.Value);
        foreach (var usuario in usuarios)
        {
            if (!string.IsNullOrEmpty(usuario.Email))
            {
                var nombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();

                if (produccion.Estado == EstadoDescripcion.Produccion.FacturaDevuelta)
                {
                    await _emailService.EnviarEmailFacturaDevueltaAsync(
                        email:              usuario.Email,
                        nombreDestinatario: nombreCompleto,
                        codigoProduccion:   produccion.NumeroProduccion ?? "",
                        razonSocial:        produccion.RazonSocial ?? "",
                        mtoTotal:           produccion.MtoTotal,
                        fechaLimite:        produccion.FechaLimite.Value,
                        idEntidadMedica:    produccion.IdEntidadMedica,
                        idProduccion:       produccion.IdProduccion);
                }
                else
                {
                    await _emailService.EnviarEmailSolicitudFacturaAsync(
                        email:              usuario.Email,
                        nombreDestinatario: nombreCompleto,
                        codigoProduccion:   produccion.NumeroProduccion ?? "",
                        razonSocial:        produccion.RazonSocial ?? "",
                        mtoTotal:           produccion.MtoTotal,
                        fechaLimite:        produccion.FechaLimite.Value,
                        idEntidadMedica:    produccion.IdEntidadMedica,
                        idProduccion:       produccion.IdProduccion);
                }
            }
        }

        _logger.LogInformation("Renotificacion de solicitud de factura enviada. GUID: {Guid}, Usuario: {Usuario}",
            guidRegistro, idModificador);

        return true;
    }

    /// <summary>
    /// Renotifica todos los registros en estado FACTURA_SOLICITADA de una sede,
    /// reenviando el correo con la fecha limite ya establecida en cada uno.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-17</created>
    /// </summary>
    public async Task<(int Enviados, int Errores)> RenotificarTodasSolicitadasAsync(int? idSede, int idModificador)
    {
        var todos = await _produccionRepository.GetListSolicitudMasivaAsync(idSede);
        var solicitadas = todos
            .Where(p => p.Estado == EstadoDescripcion.Produccion.FacturaSolicitada
                        && !string.IsNullOrEmpty(p.GuidRegistro))
            .ToList();

        int enviados = 0;
        int errores = 0;

        foreach (var item in solicitadas)
        {
            try
            {
                var resultado = await RenotificarFacturaAsync(item.GuidRegistro!, idModificador);
                if (resultado) enviados++;
                else errores++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en renotificacion masiva para GUID: {Guid}", item.GuidRegistro);
                errores++;
            }
        }

        return (enviados, errores);
    }

    /// <summary>
    /// Devuelve una factura cambiando el estado a FACTURA_DEVUELTA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-22</created>
    /// </summary>
    public async Task<bool> DevolverFacturaAsync(string guidRegistro, int idModificador)
    {
        var resultado = await _produccionRepository.DevolverFacturaAsync(guidRegistro, idModificador);

        if (resultado)
        {
            try
            {
                var produccion = await _produccionRepository.GetByGuidWithDetailsAsync(guidRegistro);
                if (produccion != null && produccion.IdEntidadMedica.HasValue && produccion.FechaLimite.HasValue)
                {
                    var usuarios = await _usuarioRepository.GetByIdEntidadMedicaAsync(produccion.IdEntidadMedica.Value);
                    foreach (var usuario in usuarios)
                    {
                        if (!string.IsNullOrEmpty(usuario.Email))
                        {
                            var nombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();
                            await _emailService.EnviarEmailFacturaDevueltaAsync(
                                email:               usuario.Email,
                                nombreDestinatario:  nombreCompleto,
                                codigoProduccion:    produccion.NumeroProduccion ?? "",
                                razonSocial:         produccion.RazonSocial ?? "",
                                mtoTotal:            produccion.MtoTotal,
                                fechaLimite:         produccion.FechaLimite.Value,
                                idEntidadMedica:     produccion.IdEntidadMedica,
                                idProduccion:        produccion.IdProduccion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificacion de factura devuelta. GUID: {Guid}", guidRegistro);
            }
        }

        return resultado;
    }

    /// <summary>
    /// Acepta una factura cambiando el estado a FACTURA_ACEPTADA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-22</created>
    /// </summary>
    public async Task<bool> AceptarFacturaAsync(string guidRegistro, int idModificador)
    {
        const string nuevoEstado = EstadoDescripcion.Produccion.FacturaAceptada;
        return await _produccionRepository.UpdateEstadoAsync(guidRegistro, nuevoEstado, idModificador);
    }

    /// <summary>
    /// Transiciona el estado de una produccion a FACTURA_ENVIADA_HHMM.
    /// Se invoca automaticamente tras aceptar una factura cuando el parametro SHM_COMPROBANTE_ENVIA_HHMM = SI.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-03-29</created>
    /// </summary>
    public async Task<bool> EnviarAHhmmAsync(string guidRegistro, int idModificador)
    {
        const string nuevoEstado = EstadoDescripcion.Produccion.FacturaEnviadaHhmm;
        return await _produccionRepository.UpdateEstadoAsync(guidRegistro, nuevoEstado, idModificador);
    }

    public async Task<bool> UpdateEstadoAsync(string guidRegistro, string estado, int idModificador)
    {
        return await _produccionRepository.UpdateEstadoAsync(guidRegistro, estado, idModificador);
    }
    /// Obtiene estadisticas del dashboard para una entidad medica.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-24</created>
    /// </summary>
    public async Task<(decimal TotalPorFacturar, int Pendientes, int Enviadas, int EnviadasHHMM, int Pagadas)> GetDashboardStatsAsync(int idEntidadMedica)
    {
        return await _produccionRepository.GetDashboardStatsAsync(idEntidadMedica);
    }

    /// <summary>
    /// Obtiene el conteo de facturas enviadas en el mes actual para una entidad medica.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-24</created>
    /// </summary>
    public async Task<int> GetFacturasEnviadasMesActualAsync(int idEntidadMedica)
    {
        return await _produccionRepository.GetFacturasEnviadasMesActualAsync(idEntidadMedica);
    }

    /// <summary>
    /// Obtiene el resumen del mes actual: total facturado, cantidad procesada y tiempo promedio.
    ///
    /// <author>ADG Vladimir</author>
    /// <created>2026-04-11</created>
    /// </summary>
    public async Task<(decimal TotalFacturado, int FacturasProcesadas, decimal TiempoPromedioDias)> GetResumenMesActualAsync(int idEntidadMedica)
    {
        return await _produccionRepository.GetResumenMesActualAsync(idEntidadMedica);
    }

    /// <summary>
    /// Obtiene datos de facturas por mes para los ultimos 6 meses.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-24</created>
    /// </summary>
    public async Task<IEnumerable<(int Anio, int Mes, int Enviadas, int Pendientes)>> GetFacturasPorMesAsync(int idEntidadMedica)
    {
        return await _produccionRepository.GetFacturasPorMesAsync(idEntidadMedica);
    }

    /// <summary>
    /// Actualiza los campos de estado de pago SAP de una produccion por su ID.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-04-15</created>
    /// </summary>
    public async Task<bool> UpdateEstadoPagoAsync(
        int idProduccion,
        string? pagoEstado,
        DateTime? pagoFecha,
        string? pagoNumeroOperacion,
        string? pagoBanco,
        string? pagoCuentaDeposito,
        decimal? pagoMontoPagado,
        int idModificador)
    {
        return await _produccionRepository.UpdateEstadoPagoAsync(
            idProduccion,
            pagoEstado,
            pagoFecha,
            pagoNumeroOperacion,
            pagoBanco,
            pagoCuentaDeposito,
            pagoMontoPagado,
            idModificador);
    }

    private static ProduccionResponseDto MapToResponseDto(Produccion produccion)
    {
        return new ProduccionResponseDto
        {
            IdProduccion = produccion.IdProduccion,
            IdSede = produccion.IdSede,
            IdEntidadMedica = produccion.IdEntidadMedica,
            CodigoProduccion = produccion.CodigoProduccion,
            NumeroProduccion = produccion.NumeroProduccion,
            TipoProduccion = produccion.TipoProduccion,
            TipoMedico = produccion.TipoMedico,
            TipoRubro = produccion.TipoRubro,
            Descripcion = produccion.Descripcion,
            Periodo = produccion.Periodo,
            EstadoProduccion = produccion.EstadoProduccion,
            MtoConsumo = produccion.MtoConsumo,
            MtoDescuento = produccion.MtoDescuento,
            MtoSubtotal = produccion.MtoSubtotal,
            MtoRenta = produccion.MtoRenta,
            MtoIgv = produccion.MtoIgv,
            MtoTotal = produccion.MtoTotal,
            MtoDetraccion = produccion.MtoDetraccion,
            PorcDetraccion = produccion.PorcDetraccion,
            PagoEstado = produccion.PagoEstado,
            PagoFecha = produccion.PagoFecha,
            PagoNumeroOperacion = produccion.PagoNumeroOperacion,
            PagoBanco = produccion.PagoBanco,
            PagoCuentaDeposito = produccion.PagoCuentaDeposito,
            PagoMontoPagado = produccion.PagoMontoPagado,
            TipoComprobante = produccion.TipoComprobante,
            Serie = produccion.Serie,
            Numero = produccion.Numero,
            FechaEmision = produccion.FechaEmision,
            Glosa = produccion.Glosa,
            EstadoComprobante = produccion.EstadoComprobante,
            Concepto = produccion.Concepto,
            FechaLimite = produccion.FechaLimite,
            Estado = produccion.Estado,
            FacturaFechaSolicitud = produccion.FacturaFechaSolicitud,
            FacturaFechaEnvio = produccion.FacturaFechaEnvio,
            FacturaFechaAceptacion = produccion.FacturaFechaAceptacion,
            FacturaFechaPago = produccion.FacturaFechaPago,
            FacturaFechaVencimiento = produccion.FacturaFechaVencimiento,
            NumeroLiquidacion = produccion.NumeroLiquidacion,
            CodigoLiquidacion = produccion.CodigoLiquidacion,
            PeriodoLiquidacion = produccion.PeriodoLiquidacion,
            EstadoLiquidacion = produccion.EstadoLiquidacion,
            FechaLiquidacion = produccion.FechaLiquidacion,
            DescripcionLiquidacion = produccion.DescripcionLiquidacion,
            TipoLiquidacion = produccion.TipoLiquidacion,
            Activo = produccion.Activo,
            GuidRegistro = produccion.GuidRegistro,
            FechaCreacion = produccion.FechaCreacion,
            FechaModificacion = produccion.FechaModificacion
        };
    }
}
