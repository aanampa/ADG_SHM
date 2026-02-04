using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad OrdenPagoLiquidacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoLiquidacionRepository
{
    Task<IEnumerable<OrdenPagoLiquidacion>> GetAllAsync();
    Task<IEnumerable<OrdenPagoLiquidacion>> GetAllActiveAsync();
    Task<OrdenPagoLiquidacion?> GetByIdAsync(int id);
    Task<OrdenPagoLiquidacion?> GetByGuidAsync(string guid);
    Task<IEnumerable<OrdenPagoLiquidacion>> GetByOrdenPagoIdAsync(int idOrdenPago);
    Task<IEnumerable<OrdenPagoLiquidacion>> GetByProduccionIdAsync(int idProduccion);
    Task<OrdenPagoLiquidacion?> GetByOrdenPagoAndProduccionAsync(int idOrdenPago, int idProduccion);
    Task<int> CreateAsync(OrdenPagoLiquidacion ordenPagoLiquidacion);
    Task<int> CreateBulkAsync(IEnumerable<OrdenPagoLiquidacion> liquidaciones);
    Task<bool> UpdateAsync(OrdenPagoLiquidacion ordenPagoLiquidacion);
    Task<bool> DeleteAsync(int id, int idModificador);
    Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador);
}
