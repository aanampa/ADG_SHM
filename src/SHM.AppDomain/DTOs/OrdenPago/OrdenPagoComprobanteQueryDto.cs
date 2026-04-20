namespace SHM.AppDomain.DTOs.OrdenPago;

/// <summary>
/// DTO interno para la consulta JOIN que obtiene los datos de comprobante
/// de todas las producciones de una orden de pago.
/// Usado exclusivamente para la consulta de estado de pago en SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-15</created>
/// </summary>
public class OrdenPagoComprobanteQueryDto
{
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
