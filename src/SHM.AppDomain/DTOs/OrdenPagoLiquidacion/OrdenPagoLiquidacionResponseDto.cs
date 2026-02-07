namespace SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

/// <summary>
/// DTO de respuesta para una liquidacion de orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public class OrdenPagoLiquidacionResponseDto
{
    public int IdOrdenPagoLiquidacion { get; set; }
    public int? IdOrdenPago { get; set; }
    public string? NumeroOrdenPago { get; set; }
    public string? NumeroLiquidacion { get; set; }
    public string? CodigoLiquidacion { get; set; }
    public decimal? MtoConsumoAcum { get; set; }
    public decimal? MtoDescuentoAcum { get; set; }
    public decimal? MtoSubtotalAcum { get; set; }
    public decimal? MtoRentaAcum { get; set; }
    public decimal? MtoIgvAcum { get; set; }
    public decimal? MtoTotalAcum { get; set; }
    public int? CantComprobantes { get; set; }
    public string? Comentarios { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
