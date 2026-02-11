using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para la actualizacion de una produccion existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos de fechas de factura</modified>
/// <modified>ADG Antonio - 2026-01-30 - Agregados campos de liquidacion</modified>
/// <modified>ADG Antonio - 2026-01-31 - Agregado campo FechaProduccion</modified>
/// </summary>
public class UpdateProduccionDto
{
    public int? IdSede { get; set; }
    public int? IdEntidadMedica { get; set; }
    public int? IdCuentaBanco { get; set; }

    [MaxLength(8)]
    public string? CodigoProduccion { get; set; }

    [MaxLength(10)]
    public string? NumeroProduccion { get; set; }

    [MaxLength(5)]
    public string? TipoProduccion { get; set; }

    [MaxLength(2)]
    public string? TipoEntidadMedica { get; set; }

    [MaxLength(2)]
    public string? TipoMedico { get; set; }

    [MaxLength(5)]
    public string? TipoRubro { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    [MaxLength(10)]
    public string? Periodo { get; set; }

    public DateTime? FechaProduccion { get; set; }

    [MaxLength(30)]
    public string? EstadoProduccion { get; set; }

    public decimal? MtoConsumo { get; set; }
    public decimal? MtoDescuento { get; set; }
    public decimal? MtoSubtotal { get; set; }
    public decimal? MtoRenta { get; set; }
    public decimal? MtoIgv { get; set; }
    public decimal? MtoTotal { get; set; }

    [MaxLength(2)]
    public string? TipoComprobante { get; set; }

    [MaxLength(4)]
    public string? Serie { get; set; }

    [MaxLength(10)]
    public string? Numero { get; set; }

    public DateTime? FechaEmision { get; set; }

    [MaxLength(300)]
    public string? Glosa { get; set; }

    [MaxLength(20)]
    public string? EstadoComprobante { get; set; }

    [MaxLength(20)]
    public string? Concepto { get; set; }

    public DateTime? FechaLimite { get; set; }

    [MaxLength(20)]
    public string? Estado { get; set; }

    public DateTime? FacturaFechaSolicitud { get; set; }
    public DateTime? FacturaFechaEnvio { get; set; }
    public DateTime? FacturaFechaAceptacion { get; set; }
    public DateTime? FacturaFechaPago { get; set; }

    // Campos de Liquidacion
    [MaxLength(10)]
    public string? NumeroLiquidacion { get; set; }

    [MaxLength(2)]
    public string? CodigoLiquidacion { get; set; }

    [MaxLength(10)]
    public string? PeriodoLiquidacion { get; set; }

    [MaxLength(10)]
    public string? EstadoLiquidacion { get; set; }

    public DateTime? FechaLiquidacion { get; set; }

    [MaxLength(300)]
    public string? DescripcionLiquidacion { get; set; }

    [MaxLength(5)]
    public string? TipoLiquidacion { get; set; }

    public int? Activo { get; set; }
}
