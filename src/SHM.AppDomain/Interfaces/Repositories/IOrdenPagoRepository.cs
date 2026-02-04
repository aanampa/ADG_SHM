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
    Task<int> CreateAsync(OrdenPago ordenPago);
    Task<bool> UpdateAsync(OrdenPago ordenPago);
    Task<bool> DeleteAsync(int id, int idModificador);
}
