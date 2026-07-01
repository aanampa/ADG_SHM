using SHM.AppDomain.DTOs.OrdenPagoProduccion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la entidad OrdenPagoProduccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoProduccionService
{
    Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetAllAsync();
    Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetAllActiveAsync();
    Task<OrdenPagoProduccionResponseDto?> GetByIdAsync(int id);
    Task<OrdenPagoProduccionResponseDto?> GetByGuidAsync(string guid);
    Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago);
    Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetByProduccionIdAsync(int idProduccion);
    Task<OrdenPagoProduccionResponseDto> CreateAsync(CreateOrdenPagoProduccionDto dto, int idCreador);
    Task<IEnumerable<OrdenPagoProduccionResponseDto>> CreateBulkAsync(IEnumerable<CreateOrdenPagoProduccionDto> dtos, int idCreador);
    Task<OrdenPagoProduccionResponseDto?> UpdateAsync(UpdateOrdenPagoProduccionDto dto, int idModificador);
    Task<bool> DeleteAsync(string guid, int idModificador);
    Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador);
}
