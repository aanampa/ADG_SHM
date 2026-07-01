using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad PerfilAprobacionUsuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public interface IPerfilAprobacionUsuarioRepository
{
    Task<IEnumerable<PerfilAprobacionUsuario>> GetAllAsync();
    Task<PerfilAprobacionUsuario?> GetByIdAsync(int idPerfilAprobacion, int idUsuario);
    Task<IEnumerable<PerfilAprobacionUsuario>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion);
    Task<IEnumerable<PerfilAprobacionUsuario>> GetByUsuarioIdAsync(int idUsuario);
    Task<bool> CreateAsync(PerfilAprobacionUsuario perfilAprobacionUsuario);
    Task<bool> DeleteAsync(int idPerfilAprobacion, int idUsuario);
    Task<bool> ExistsAsync(int idPerfilAprobacion, int idUsuario);
}
