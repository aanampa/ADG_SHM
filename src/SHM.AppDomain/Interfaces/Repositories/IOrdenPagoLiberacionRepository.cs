using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para el resultado de la liberacion de pagos en SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public interface IOrdenPagoLiberacionRepository
{
    /// <summary>
    /// Obtiene el resultado de liberacion de todos los comprobantes de una orden de pago.
    /// </summary>
    Task<IEnumerable<OrdenPagoLiberacion>> GetByOrdenPagoIdAsync(int idOrdenPago);

    /// <summary>
    /// Inserta o actualiza (por orden de pago y produccion) el resultado de la liberacion.
    /// Si el registro existe, suma <paramref name="intento"/> a INTENTOS.
    /// </summary>
    /// <param name="item">Datos a guardar.</param>
    /// <param name="intento">1 si se envio a SAP en esta operacion, 0 si no.</param>
    /// <param name="idUsuario">Usuario que dispara la operacion.</param>
    Task SaveResultadoAsync(OrdenPagoLiberacion item, int intento, int idUsuario);
}
