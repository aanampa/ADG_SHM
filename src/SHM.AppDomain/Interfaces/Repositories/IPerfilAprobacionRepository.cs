using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad PerfilAprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public interface IPerfilAprobacionRepository
{
    Task<IEnumerable<PerfilAprobacion>> GetAllAsync();
    Task<IEnumerable<PerfilAprobacion>> GetAllActiveAsync();
    Task<PerfilAprobacion?> GetByIdAsync(int id);
    Task<PerfilAprobacion?> GetByGuidAsync(string guid);
    Task<PerfilAprobacion?> GetByCodigoAsync(string codigo);
    Task<int> CreateAsync(PerfilAprobacion perfilAprobacion);
    Task<bool> UpdateAsync(PerfilAprobacion perfilAprobacion);
    Task<bool> DeleteAsync(int id, int idModificador);
    Task<IEnumerable<PerfilAprobacion>> GetByGrupoFlujoTrabajoAsync(string grupoFlujoTrabajo);
}
