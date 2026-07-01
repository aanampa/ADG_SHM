using SHM.AppDomain.DTOs.OrdenPago;
using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la entidad OrdenPagoProduccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// <modified>ADG Antonio - 2026-04-15 - Agregado GetComprobantesParaSapByOrdenPagoGuidAsync</modified>
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

    /// <summary>
    /// Obtiene los datos de comprobante de todas las producciones activas de una orden de pago,
    /// incluyendo los campos necesarios para consultar el estado de pago en SAP.
    /// </summary>
    Task<IEnumerable<OrdenPagoComprobanteQueryDto>> GetComprobantesParaSapByOrdenPagoGuidAsync(string guidOrdenPago);
}
