using SHM.AppDomain.DTOs.OrdenPago;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la entidad OrdenPago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoService
{
    Task<IEnumerable<OrdenPagoResponseDto>> GetAllAsync();
    Task<IEnumerable<OrdenPagoResponseDto>> GetAllActiveAsync();
    Task<OrdenPagoResponseDto?> GetByIdAsync(int id);
    Task<OrdenPagoResponseDto?> GetByGuidAsync(string guid);
    Task<OrdenPagoResponseDto?> GetByNumeroOrdenPagoAsync(string numeroOrdenPago);
    Task<IEnumerable<OrdenPagoResponseDto>> GetByBancoAsync(int idBanco);
    Task<IEnumerable<OrdenPagoResponseDto>> GetByEstadoAsync(string estado);
    Task<IEnumerable<OrdenPagoResponseDto>> GetByFechaGeneracionAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<OrdenPagoResponseDto>> GetPendingForApprovalByUserAsync(int idUsuario);
    Task<OrdenPagoResponseDto> CreateAsync(CreateOrdenPagoDto dto, int idCreador);
    Task<OrdenPagoResponseDto?> UpdateAsync(UpdateOrdenPagoDto dto, int idModificador);
    Task<bool> DeleteAsync(string guid, int idModificador);

    /// <summary>
    /// Obtiene el listado paginado de ordenes de pago con filtros aplicados en BD.
    /// </summary>
    Task<(IEnumerable<OrdenPagoResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        int? idBanco, string? estado, int? idSede, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene las ordenes de pago que el usuario ya aprobo.
    /// </summary>
    Task<IEnumerable<OrdenPagoResponseDto>> GetApprovedByUserAsync(int idUsuario);
}
