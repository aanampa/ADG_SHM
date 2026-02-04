namespace SHM.AppDomain.DTOs.Liquidacion;

/// <summary>
/// DTO de respuesta para el listado de liquidaciones con datos de produccion,
/// liquidacion y cuenta bancaria de la entidad medica.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
public class LiquidacionListaResponseDto
{
    // ===== Produccion - Identificadores =====
    public int IdProduccion { get; set; }
    public string? GuidRegistro { get; set; }
    public int? IdSede { get; set; }
    public int? IdEntidadMedica { get; set; }
    public string? CodigoProduccion { get; set; }

    // ===== Produccion - Tipos con descripciones =====
    public string? TipoProduccion { get; set; }
    public string? DesTipoProduccion { get; set; }
    public string? TipoMedico { get; set; }
    public string? DesTipoMedico { get; set; }
    public string? TipoRubro { get; set; }
    public string? DesTipoRubro { get; set; }
    public string? Estado { get; set; }
    public string? DesEstado { get; set; }

    // ===== Produccion - Descripcion y periodo =====
    public string? Descripcion { get; set; }
    public string? Periodo { get; set; }
    public string? EstadoProduccion { get; set; }

    // ===== Produccion - Montos =====
    public decimal? MtoConsumo { get; set; }
    public decimal? MtoDescuento { get; set; }
    public decimal? MtoSubtotal { get; set; }
    public decimal? MtoRenta { get; set; }
    public decimal? MtoIgv { get; set; }
    public decimal? MtoTotal { get; set; }

    // ===== Produccion - Comprobante =====
    public string? TipoComprobante { get; set; }
    public string? Concepto { get; set; }
    public DateTime? FechaLimite { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? Glosa { get; set; }
    public string? EstadoComprobante { get; set; }

    // ===== Liquidacion - Campos especificos =====
    public string? NumeroLiquidacion { get; set; }
    public string? CodigoLiquidacion { get; set; }
    public string? PeriodoLiquidacion { get; set; }
    public string? EstadoLiquidacion { get; set; }
    public DateTime? FechaLiquidacion { get; set; }
    public string? DescripcionLiquidacion { get; set; }

    // ===== Cuenta Bancaria - Datos del primer registro activo =====
    public int? IdBanco { get; set; }
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }

    // ===== Sede =====
    public string? CodigoSede { get; set; }
    public string? NombreSede { get; set; }

    // ===== Entidad Medica =====
    public string? Ruc { get; set; }
    public string? RazonSocial { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? DesTipoEntidadMedica { get; set; }

    // ===== Auditoria =====
    public int Activo { get; set; }
    public int? IdCreador { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // ===== Propiedades calculadas =====
    public string? ComprobanteFactura => !string.IsNullOrEmpty(Serie) && !string.IsNullOrEmpty(Numero)
        ? $"{Serie}-{Numero}"
        : null;
}
