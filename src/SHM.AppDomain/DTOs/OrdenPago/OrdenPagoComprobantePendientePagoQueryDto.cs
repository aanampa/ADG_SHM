namespace SHM.AppDomain.DTOs.OrdenPago;

/// <summary>
/// DTO interno para la consulta JOIN que obtiene los comprobantes pendientes de
/// actualizar estado de pago: producciones de ordenes de pago APROBADAS cuyo
/// PAGO_ESTADO aun es nulo.
/// Usado exclusivamente para el proceso masivo de actualizacion de estado de pago en SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-07-18</created>
/// </summary>
public class OrdenPagoComprobantePendientePagoQueryDto
{
    public int IdOrdenPago { get; set; }
    public string? GuidOrdenPago { get; set; }
    public string? NumeroOrdenPago { get; set; }

    public int IdProduccion { get; set; }
    public string? GuidProduccion { get; set; }
    public string? TipoComprobante { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? CodigoAcreedor { get; set; }
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
}
