using SHM.AppDomain.DTOs.Produccion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de producciones en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-20 - Agregado metodo de listado paginado con filtros</modified>
/// <modified>ADG Antonio - 2026-04-15 - Agregado UpdateEstadoPagoAsync</modified>
/// </summary>
public interface IProduccionService
{
    /// <summary>
    /// Obtiene todas las producciones registradas en el sistema.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetAllProduccionesAsync();

    /// <summary>
    /// Obtiene una produccion por su identificador unico.
    /// </summary>
    Task<ProduccionResponseDto?> GetProduccionByIdAsync(int id);

    /// <summary>
    /// Obtiene una produccion por su codigo.
    /// </summary>
    Task<ProduccionResponseDto?> GetProduccionByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una produccion por su GUID de registro con datos relacionados.
    /// </summary>
    Task<ProduccionListaResponseDto?> GetProduccionByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una sede especifica.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesBySedeAsync(int idSede);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una entidad medica especifica.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByEntidadMedicaAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene producciones de una entidad medica filtradas por estado comprobante en Oracle.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByEntidadMedicaYEstadoComprobanteAsync(int idEntidadMedica, string estadoComprobante);

    /// <summary>
    /// Obtiene todas las producciones de un periodo especifico.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByPeriodoAsync(string periodo);

    /// <summary>
    /// Crea una nueva produccion en el sistema.
    /// </summary>
    Task<ProduccionResponseDto> CreateProduccionAsync(CreateProduccionDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una produccion existente.
    /// </summary>
    Task<bool> UpdateProduccionAsync(int id, UpdateProduccionDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una produccion registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteProduccionAsync(int id, int idModificador);

    /// <summary>
    /// Revierte los datos de comprobante de una produccion al estado indicado.
    /// Limpia Serie, Numero, FechaEmision, Glosa, EstadoComprobante, FacturaFechaEnvio.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-03-01</created>
    Task<bool> RevertComprobanteAsync(int idProduccion, string estado, int idModificador);

    /// <summary>
    /// Obtiene el listado paginado de producciones con datos relacionados y filtros.
    /// </summary>
    /// <param name="produccion">Filtro por codigo de produccion (opcional)</param>
    /// <param name="estado">Filtro por estado del proceso (opcional)</param>
    /// <param name="idEntidadMedica">Filtro por ID de Cia Medica (opcional)</param>
    /// <param name="idSede">Filtro por ID de Sede del usuario logueado (opcional)</param>
    /// <param name="pageNumber">Numero de pagina</param>
    /// <param name="pageSize">Tamaño de pagina</param>
    /// <returns>Tupla con lista de producciones y total de registros</returns>
    Task<(IEnumerable<ProduccionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        string? produccion, string? estado, int? idEntidadMedica, int? idSede, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene producciones FACTURA_PENDIENTE y FACTURA_SOLICITADA para el modal de solicitud masiva,
    /// con datos de validacion (usuarios externos y cuentas bancarias) en una sola query.
    /// </summary>
    Task<IEnumerable<ProduccionListaResponseDto>> GetListSolicitudMasivaAsync(int? idSede);

    /// <summary>
    /// Solicita factura actualizando la fecha limite y cambiando el estado a FACTURA_SOLICITADA.
    /// </summary>
    /// <param name="solicitudDto">Datos de la solicitud (GUID, fecha y hora)</param>
    /// <param name="idModificador">ID del usuario que realiza la solicitud</param>
    /// <returns>True si se proceso correctamente</returns>
    Task<bool> SolicitarFacturaAsync(SolicitarFacturaDto solicitudDto, int idModificador);

    /// <summary>
    /// Renotifica la solicitud de factura enviando nuevamente el correo con la fecha limite ya establecida.
    /// No modifica el estado ni la fecha limite del registro. Solo aplica a FACTURA_SOLICITADA.
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <param name="idModificador">ID del usuario que solicita la renotificacion</param>
    /// <returns>True si el correo fue enviado correctamente</returns>
    Task<bool> RenotificarFacturaAsync(string guidRegistro, int idModificador);

    /// <summary>
    /// Renotifica todos los registros en estado FACTURA_SOLICITADA de una sede,
    /// reenviando el correo con la fecha limite ya establecida en cada uno.
    /// </summary>
    /// <param name="idSede">ID de la sede del usuario logueado</param>
    /// <param name="idModificador">ID del usuario que solicita la renotificacion</param>
    /// <returns>Tupla con cantidad de correos enviados y cantidad de errores</returns>
    Task<(int Enviados, int Errores)> RenotificarTodasSolicitadasAsync(int? idSede, int idModificador);

    /// <summary>
    /// Devuelve una factura cambiando el estado a FACTURA_DEVUELTA.
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <param name="idModificador">ID del usuario que realiza la devolucion</param>
    /// <returns>True si se proceso correctamente</returns>
    Task<bool> DevolverFacturaAsync(string guidRegistro, int idModificador);

    /// <summary>
    /// Acepta una factura cambiando el estado a FACTURA_ACEPTADA.
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <param name="idModificador">ID del usuario que acepta la factura</param>
    /// <returns>True si se proceso correctamente</returns>
    Task<bool> AceptarFacturaAsync(string guidRegistro, int idModificador);

    /// <summary>
    /// Transiciona el estado de una produccion a FACTURA_ENVIADA_HHMM.
    /// Se invoca automaticamente tras aceptar una factura cuando el parametro SHM_COMPROBANTE_ENVIA_HHMM = SI.
    /// </summary>
    Task<bool> EnviarAHhmmAsync(string guidRegistro, int idModificador);

    /// <summary>
    /// Actualiza el estado de una produccion por GUID.
    /// </summary>
    Task<bool> UpdateEstadoAsync(string guidRegistro, string estado, int idModificador);
    /// Obtiene estadisticas del dashboard para una entidad medica.
    /// </summary>
    /// <param name="idEntidadMedica">ID de la entidad medica</param>
    /// <returns>Tupla con estadisticas: TotalPorFacturar, conteo por estados</returns>
    Task<(decimal TotalPorFacturar, int Pendientes, int Enviadas, int EnviadasHHMM, int Pagadas)> GetDashboardStatsAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene el conteo de facturas enviadas en el mes actual para una entidad medica.
    /// </summary>
    /// <param name="idEntidadMedica">ID de la entidad medica</param>
    /// <returns>Cantidad de facturas enviadas en el mes</returns>
    Task<int> GetFacturasEnviadasMesActualAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene el resumen del mes actual: total facturado, cantidad procesada y tiempo promedio.
    /// </summary>
    Task<(decimal TotalFacturado, int FacturasProcesadas, decimal TiempoPromedioDias)> GetResumenMesActualAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene datos de facturas por mes para los ultimos 6 meses.
    /// </summary>
    /// <param name="idEntidadMedica">ID de la entidad medica</param>
    /// <returns>Lista de datos por mes</returns>
    Task<IEnumerable<(int Anio, int Mes, int Enviadas, int Pendientes)>> GetFacturasPorMesAsync(int idEntidadMedica);

    /// <summary>
    /// Actualiza los campos de estado de pago SAP de una produccion por su ID.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-04-15</created>
    Task<bool> UpdateEstadoPagoAsync(
        int idProduccion,
        string? pagoEstado,
        DateTime? pagoFecha,
        string? pagoNumeroOperacion,
        string? pagoBanco,
        string? pagoCuentaDeposito,
        decimal? pagoMontoPagado,
        int idModificador);
}
