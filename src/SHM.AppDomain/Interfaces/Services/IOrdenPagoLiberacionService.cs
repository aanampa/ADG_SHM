using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Servicio que libera en SAP los comprobantes de una orden de pago aprobada.
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public interface IOrdenPagoLiberacionService
{
    /// <summary>
    /// Libera en SAP todos los comprobantes de la orden de pago (un solo envio).
    /// Valida los datos antes de llamar a SAP y guarda el resultado por comprobante.
    /// No lanza excepciones por fallos de SAP: devuelve Exito = false con el detalle.
    /// Si todos los comprobantes ya estan liberados no vuelve a llamar a SAP.
    /// </summary>
    /// <param name="idOrdenPago">Orden de pago aprobada.</param>
    /// <param name="idUsuario">Usuario que dispara la liberacion (auditoria y bitacora).</param>
    Task<(bool Exito, string Mensaje)> LiberarAsync(int idOrdenPago, int idUsuario);

    /// <summary>
    /// Obtiene el resultado de liberacion de los comprobantes de una orden de pago.
    /// </summary>
    Task<IEnumerable<OrdenPagoLiberacion>> GetByOrdenPagoIdAsync(int idOrdenPago);
}
