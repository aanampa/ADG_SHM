using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad OrdenPago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoRepository
{
    Task<IEnumerable<OrdenPago>> GetAllAsync();
    Task<IEnumerable<OrdenPago>> GetAllActiveAsync();
    Task<OrdenPago?> GetByIdAsync(int id);
    Task<OrdenPago?> GetByGuidAsync(string guid);
    Task<OrdenPago?> GetByNumeroOrdenPagoAsync(string numeroOrdenPago);
    Task<IEnumerable<OrdenPago>> GetByBancoAsync(int idBanco);
    Task<IEnumerable<OrdenPago>> GetByEstadoAsync(string estado);
    Task<IEnumerable<OrdenPago>> GetByFechaGeneracionAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<OrdenPago>> GetPendingForApprovalByUserAsync(int idUsuario);
    Task<bool> UpdateEstadoAsync(int idOrdenPago, string estado, int idModificador);
    Task<int> CreateAsync(OrdenPago ordenPago);
    Task<bool> UpdateAsync(OrdenPago ordenPago);
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Obtiene el siguiente correlativo de orden de pago para una sede, anio y mes dados.
    /// </summary>
    Task<int> GetSiguienteCorrelativoAsync(int idSede, int anio, int mes);

    /// <summary>
    /// Obtiene el listado paginado de ordenes de pago con filtros aplicados en BD.
    /// </summary>
    Task<(IEnumerable<OrdenPago> Items, int TotalCount)> GetPaginatedListAsync(
        int? idBanco, string? estado, int? idSede, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene las ordenes de pago que el usuario ya aprobo.
    /// </summary>
    Task<IEnumerable<OrdenPago>> GetApprovedByUserAsync(int idUsuario);

    /// <summary>
    /// Anula una orden de pago y revierte las producciones asociadas a FACTURA_LIQUIDADA.
    /// </summary>
    Task<bool> AnularAsync(int idOrdenPago, int idModificador);

    /// <summary>
    /// Cuenta las producciones de una orden de pago que NO estan en estado FACTURA_PAGADA.
    /// Usado para determinar si la orden debe pasar a estado PAGADO.
    /// </summary>
    Task<int> GetCountProduccionesNotPagadasAsync(int idOrdenPago);
}
