using SHM.AppDomain.DTOs.PerfilAprobacionUsuario;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la entidad PerfilAprobacionUsuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public interface IPerfilAprobacionUsuarioService
{
    Task<IEnumerable<PerfilAprobacionUsuarioResponseDto>> GetAllAsync();
    Task<PerfilAprobacionUsuarioResponseDto?> GetByIdAsync(int idPerfilAprobacion, int idUsuario);
    Task<IEnumerable<PerfilAprobacionUsuarioResponseDto>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion);
    Task<IEnumerable<PerfilAprobacionUsuarioResponseDto>> GetByUsuarioIdAsync(int idUsuario);
    Task<PerfilAprobacionUsuarioResponseDto> CreateAsync(CreatePerfilAprobacionUsuarioDto dto);
    Task<bool> DeleteAsync(int idPerfilAprobacion, int idUsuario);
}
