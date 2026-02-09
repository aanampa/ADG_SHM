using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad OrdenPagoAprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoAprobacionRepository
{
    Task<IEnumerable<OrdenPagoAprobacion>> GetAllAsync();
    Task<IEnumerable<OrdenPagoAprobacion>> GetAllActiveAsync();
    Task<OrdenPagoAprobacion?> GetByIdAsync(int id);
    Task<OrdenPagoAprobacion?> GetByGuidAsync(string guid);
    Task<IEnumerable<OrdenPagoAprobacion>> GetByOrdenPagoIdAsync(int idOrdenPago);
    Task<IEnumerable<OrdenPagoAprobacion>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion);
    Task<OrdenPagoAprobacion?> GetByOrdenPagoAndPerfilAprobacionAsync(int idOrdenPago, int idPerfilAprobacion);
    Task<OrdenPagoAprobacion?> GetPendingByOrdenPagoForUserAsync(int idOrdenPago, int idUsuario);
    Task<bool> AprobarAsync(int idOrdenPagoAprobacion, int idUsuarioAprobador, int idModificador);
    Task<bool> RechazarAsync(int idOrdenPagoAprobacion, int idUsuarioAprobador, int idModificador);
    Task<int> CreateAsync(OrdenPagoAprobacion ordenPagoAprobacion);
    Task<bool> UpdateAsync(OrdenPagoAprobacion ordenPagoAprobacion);
    Task<bool> DeleteAsync(int id, int idModificador);
    Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador);
}
