using SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la entidad OrdenPagoLiquidacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public interface IOrdenPagoLiquidacionService
{
    Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetAllAsync();
    Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetAllActiveAsync();
    Task<OrdenPagoLiquidacionResponseDto?> GetByIdAsync(int id);
    Task<OrdenPagoLiquidacionResponseDto?> GetByGuidAsync(string guid);
    Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago);
    Task<OrdenPagoLiquidacionResponseDto> CreateAsync(CreateOrdenPagoLiquidacionDto dto, int idCreador);
    Task<OrdenPagoLiquidacionResponseDto?> UpdateAsync(UpdateOrdenPagoLiquidacionDto dto, int idModificador);
    Task<bool> DeleteAsync(string guid, int idModificador);
    Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador);
}
