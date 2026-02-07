using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad OrdenPagoProduccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoProduccionRepository
{
    Task<IEnumerable<OrdenPagoProduccion>> GetAllAsync();
    Task<IEnumerable<OrdenPagoProduccion>> GetAllActiveAsync();
    Task<OrdenPagoProduccion?> GetByIdAsync(int id);
    Task<OrdenPagoProduccion?> GetByGuidAsync(string guid);
    Task<IEnumerable<OrdenPagoProduccion>> GetByOrdenPagoIdAsync(int idOrdenPago);
    Task<IEnumerable<OrdenPagoProduccion>> GetByProduccionIdAsync(int idProduccion);
    Task<OrdenPagoProduccion?> GetByOrdenPagoAndProduccionAsync(int idOrdenPago, int idProduccion);
    Task<int> CreateAsync(OrdenPagoProduccion ordenPagoProduccion);
    Task<int> CreateBulkAsync(IEnumerable<OrdenPagoProduccion> producciones);
    Task<bool> UpdateAsync(OrdenPagoProduccion ordenPagoProduccion);
    Task<bool> DeleteAsync(int id, int idModificador);
    Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador);
}
