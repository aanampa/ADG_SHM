namespace SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

/// <summary>
/// DTO para el detalle de produccion por liquidacion en una orden de pago.
/// Resultado del JOIN entre SHM_PRODUCCION, SHM_ORDEN_PAGO_LIQUIDACION,
/// SHM_ENTIDAD_MEDICA y SHM_ENTIDAD_CUENTA_BANCO.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class DetalleLiquidacionItemDto
{
    // Campos de Liquidacion
    public string? CodigoLiquidacion { get; set; }
    public string? DescripcionLiquidacion { get; set; }
    public string? TipoLiquidacion { get; set; }
    public string? PeriodoLiquidacion { get; set; }

    // Entidad Medica
    public string? Ruc { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? RazonSocial { get; set; }

    // Banco
    public string? NombreBanco { get; set; }

    // Comprobante (Produccion)
    public string? TipoComprobante { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }

    // Montos (Produccion)
    public decimal? MtoSubtotal { get; set; }
    public decimal? MtoIgv { get; set; }
    public decimal? MtoRenta { get; set; }
    public decimal? MtoTotal { get; set; }
}
