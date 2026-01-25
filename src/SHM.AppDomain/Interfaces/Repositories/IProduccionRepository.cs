using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de producciones en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-20 - Agregado metodo de listado paginado con filtros</modified>
/// </summary>
public interface IProduccionRepository
{
    /// <summary>
    /// Obtiene todas las producciones registradas en el sistema.
    /// </summary>
    Task<IEnumerable<Produccion>> GetAllAsync();

    /// <summary>
    /// Obtiene una produccion por su identificador unico.
    /// </summary>
    Task<Produccion?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene una produccion por su codigo.
    /// </summary>
    Task<Produccion?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una produccion por su GUID de registro.
    /// </summary>
    Task<Produccion?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una sede especifica.
    /// </summary>
    Task<IEnumerable<Produccion>> GetBySedeAsync(int idSede);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una entidad medica especifica.
    /// </summary>
    Task<IEnumerable<Produccion>> GetByEntidadMedicaAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene todas las producciones de un periodo especifico.
    /// </summary>
    Task<IEnumerable<Produccion>> GetByPeriodoAsync(string periodo);

    /// <summary>
    /// Crea una nueva produccion en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Produccion produccion);

    /// <summary>
    /// Actualiza los datos de una produccion existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Produccion produccion);

    /// <summary>
    /// Elimina una produccion registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una produccion con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Verifica si existe una produccion con la llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion).
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-19</created>
    /// </summary>
    Task<bool> ExistsByKeyAsync(int idSede, int idEntidadMedica, string codigoProduccion);

    /// <summary>
    /// Obtiene el listado paginado de producciones con datos relacionados y filtros.
    /// </summary>
    /// <param name="produccion">Filtro por codigo de produccion (opcional)</param>
    /// <param name="estado">Filtro por estado del proceso (opcional)</param>
    /// <param name="idEntidadMedica">Filtro por ID de Cia Medica (opcional)</param>
    /// <param name="pageNumber">Numero de pagina</param>
    /// <param name="pageSize">Tamaño de pagina</param>
    /// <returns>Tupla con lista de producciones y total de registros</returns>
    Task<(IEnumerable<ProduccionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        string? produccion, string? estado, int? idEntidadMedica, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene una produccion por su GUID con datos relacionados (sede, entidad medica, descripciones).
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <returns>Produccion con datos relacionados o null si no existe</returns>
    Task<ProduccionListaResponseDto?> GetByGuidWithDetailsAsync(string guidRegistro);

    /// <summary>
    /// Actualiza la fecha limite y estado de una produccion para solicitud de factura.
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <param name="fechaLimite">Fecha y hora limite para entrega de factura</param>
    /// <param name="estado">Nuevo estado de la produccion</param>
    /// <param name="idModificador">ID del usuario que realiza la modificacion</param>
    /// <returns>True si se actualizo correctamente</returns>
    Task<bool> UpdateFechaLimiteEstadoAsync(string guidRegistro, DateTime fechaLimite, string estado, int idModificador);

    /// <summary>
    /// Actualiza solo el estado de una produccion.
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <param name="estado">Nuevo estado de la produccion</param>
    /// <param name="idModificador">ID del usuario que realiza la modificacion</param>
    /// <returns>True si se actualizo correctamente</returns>
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
    /// Obtiene datos de facturas por mes para los ultimos 6 meses.
    /// </summary>
    /// <param name="idEntidadMedica">ID de la entidad medica</param>
    /// <returns>Lista de datos por mes</returns>
    Task<IEnumerable<(int Anio, int Mes, int Enviadas, int Pendientes)>> GetFacturasPorMesAsync(int idEntidadMedica);

}
