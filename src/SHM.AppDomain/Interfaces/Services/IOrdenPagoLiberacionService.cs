using SHM.AppDomain.DTOs.SapApi;
using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Servicio que libera en SAP los comprobantes de una orden de pago aprobada.
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// <modified>ADG Antonio - 2026-11-14 - Reenvio forzado, regeneracion de la trama y busqueda por numero de orden</modified>
/// </summary>
public interface IOrdenPagoLiberacionService
{
    /// <summary>
    /// Libera en SAP todos los comprobantes de la orden de pago (un solo envio).
    /// Valida los datos antes de llamar a SAP y guarda el resultado por comprobante.
    /// No lanza excepciones por fallos de SAP: devuelve Exito = false con el detalle.
    /// Si todos los comprobantes ya estan liberados no vuelve a llamar a SAP, salvo que
    /// <paramref name="forzar"/> sea true (reenvio manual para casos puntuales).
    /// </summary>
    /// <param name="idOrdenPago">Orden de pago aprobada.</param>
    /// <param name="idUsuario">Usuario que dispara la liberacion (auditoria y bitacora).</param>
    /// <param name="forzar">Reenvia aunque todos los comprobantes figuren como liberados.</param>
    Task<(bool Exito, string Mensaje)> LiberarAsync(int idOrdenPago, int idUsuario, bool forzar = false);

    /// <summary>
    /// Igual que <see cref="LiberarAsync"/>, buscando la orden por su numero (ej. OP-09-20261104).
    /// </summary>
    Task<(bool Exito, string Mensaje)> LiberarPorNumeroAsync(string numeroOrdenPago, int idUsuario, bool forzar = false);

    /// <summary>
    /// Regenera la trama que se enviaria a SAP para la orden, con los datos actuales y sin enviarla.
    /// </summary>
    Task<LiberacionTramaDto> GenerarTramaAsync(string numeroOrdenPago);

    /// <summary>
    /// Obtiene el resultado de liberacion de los comprobantes de una orden de pago.
    /// </summary>
    Task<IEnumerable<OrdenPagoLiberacion>> GetByOrdenPagoIdAsync(int idOrdenPago);

    /// <summary>
    /// Obtiene el resultado de liberacion de los comprobantes de una orden, por su numero.
    /// </summary>
    Task<IEnumerable<OrdenPagoLiberacion>> GetByNumeroOrdenPagoAsync(string numeroOrdenPago);
}
