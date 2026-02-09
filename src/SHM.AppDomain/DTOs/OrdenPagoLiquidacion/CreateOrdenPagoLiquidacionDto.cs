namespace SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

/// <summary>
/// DTO para la creacion de una liquidacion de orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public class CreateOrdenPagoLiquidacionDto
{
    public int? IdOrdenPago { get; set; }
    public string? NumeroLiquidacion { get; set; }
    public string? CodigoLiquidacion { get; set; }
    public decimal? MtoConsumoAcum { get; set; }
    public decimal? MtoDescuentoAcum { get; set; }
    public decimal? MtoSubtotalAcum { get; set; }
    public decimal? MtoRentaAcum { get; set; }
    public decimal? MtoIgvAcum { get; set; }
    public decimal? MtoTotalAcum { get; set; }
    public int? CantComprobantes { get; set; }
    public string? DescripcionLiquidacion { get; set; }
    public string? PeriodoLiquidacion { get; set; }
    public int? IdBanco { get; set; }
    public string? TipoLiquidacion { get; set; }
    public string? Comentarios { get; set; }
}
