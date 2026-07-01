using SHM.AppDomain.DTOs.PerfilAprobacion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la entidad PerfilAprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public interface IPerfilAprobacionService
{
    Task<IEnumerable<PerfilAprobacionResponseDto>> GetAllAsync();
    Task<IEnumerable<PerfilAprobacionResponseDto>> GetAllActiveAsync();
    Task<PerfilAprobacionResponseDto?> GetByIdAsync(int id);
    Task<PerfilAprobacionResponseDto?> GetByGuidAsync(string guid);
    Task<PerfilAprobacionResponseDto?> GetByCodigoAsync(string codigo);
    Task<PerfilAprobacionResponseDto> CreateAsync(CreatePerfilAprobacionDto dto, int idCreador);
    Task<PerfilAprobacionResponseDto?> UpdateAsync(UpdatePerfilAprobacionDto dto, int idModificador);
    Task<bool> DeleteAsync(string guid, int idModificador);
}
