namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO de respuesta con la informacion de una produccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos de fechas de factura</modified>
/// <modified>ADG Antonio - 2026-01-30 - Agregados campos de liquidacion</modified>
/// <modified>ADG Antonio - 2026-01-31 - Agregado campo FechaProduccion</modified>
/// </summary>
public class ProduccionResponseDto
{
    public int IdProduccion { get; set; }
    public int IdSede { get; set; }
    public int IdEntidadMedica { get; set; }
    public string? CodigoProduccion { get; set; }
    public string? NumeroProduccion { get; set; }
    public string? TipoProduccion { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? TipoMedico { get; set; }
    public string? TipoRubro { get; set; }
    public string? Descripcion { get; set; }
    public string? Periodo { get; set; }
    public DateTime? FechaProduccion { get; set; }
    public string? EstadoProduccion { get; set; }
    public decimal? MtoConsumo { get; set; }
    public decimal? MtoDescuento { get; set; }
    public decimal? MtoSubtotal { get; set; }
    public decimal? MtoRenta { get; set; }
    public decimal? MtoIgv { get; set; }
    public decimal? MtoTotal { get; set; }
    public string? TipoComprobante { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? Glosa { get; set; }
    public string? EstadoComprobante { get; set; }
    public string? Concepto { get; set; }
    public DateTime? FechaLimite { get; set; }
    public string? Estado { get; set; }
    public DateTime? FacturaFechaSolicitud { get; set; }
    public DateTime? FacturaFechaEnvio { get; set; }
    public DateTime? FacturaFechaAceptacion { get; set; }
    public DateTime? FacturaFechaPago { get; set; }

    // Campos de Liquidacion
    public string? NumeroLiquidacion { get; set; }
    public string? CodigoLiquidacion { get; set; }
    public string? PeriodoLiquidacion { get; set; }
    public string? EstadoLiquidacion { get; set; }
    public DateTime? FechaLiquidacion { get; set; }
    public string? DescripcionLiquidacion { get; set; }
    public string? TipoLiquidacion { get; set; }

    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
