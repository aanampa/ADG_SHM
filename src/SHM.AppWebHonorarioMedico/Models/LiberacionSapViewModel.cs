using SHM.AppDomain.Constants;
using SHM.AppDomain.Entities;

namespace SHM.AppWebHonorarioMedico.Models;

/// <summary>
/// Modelo del bloque "Liberacion de pagos en SAP" de las pantallas de detalle
/// de la orden de pago (partial _LiberacionSapCard).
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public class LiberacionSapViewModel
{
    public string GuidOrdenPago { get; set; } = string.Empty;
    public string? EstadoOrden { get; set; }
    public int CantComprobantes { get; set; }
    public List<OrdenPagoLiberacion> Items { get; set; } = new();

    /// <summary>Muestra el boton para reintentar la liberacion (solo Tesoreria).</summary>
    public bool PermiteReintento { get; set; }

    public int Liberados => Items.Count(i => i.EstadoLiberacion == EstadoLiberacion.Liberado);
    public int ConError => Items.Count(i => i.EstadoLiberacion == EstadoLiberacion.Error);

    /// <summary>
    /// LIBERADO si todos los comprobantes estan liberados, ERROR si alguno fallo,
    /// PENDIENTE si todavia no se envio.
    /// </summary>
    public string EstadoGeneral =>
        CantComprobantes > 0 && Liberados >= CantComprobantes ? EstadoLiberacion.Liberado
        : ConError > 0 ? EstadoLiberacion.Error
        : EstadoLiberacion.Pendiente;
}

/// <summary>
/// Request para reintentar la liberacion de una orden de pago en SAP.
/// </summary>
public class ReintentarLiberacionRequest
{
    public string GuidOrdenPago { get; set; } = string.Empty;
}
