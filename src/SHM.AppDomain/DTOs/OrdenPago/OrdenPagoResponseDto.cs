namespace SHM.AppDomain.DTOs.OrdenPago;

/// <summary>
/// DTO de respuesta para orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoResponseDto
{
    public int IdOrdenPago { get; set; }
    public int IdBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? NumeroOrdenPago { get; set; }
    public DateTime? FechaGeneracion { get; set; }
    public string? Estado { get; set; }
    public decimal? MtoConsumoAcum { get; set; }
    public decimal? MtoDescuentoAcum { get; set; }
    public decimal? MtoSubtotalAcum { get; set; }
    public decimal? MtoRentaAcum { get; set; }
    public decimal? MtoIgvAcum { get; set; }
    public decimal? MtoTotalAcum { get; set; }
    public int? CantComprobantes { get; set; }
    public int? CantLiquidaciones { get; set; }
    public string? Comentarios { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
