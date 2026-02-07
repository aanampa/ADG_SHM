namespace SHM.AppDomain.DTOs.Liquidacion;

/// <summary>
/// DTO de respuesta para el listado agrupado de liquidaciones.
/// Agrupa por CODIGO_LIQUIDACION e ID_BANCO para la generacion de ordenes de pago.
/// </summary>
/// <author>ADG Vladimir D</author>
/// <created>2026-02-06</created>
public class LiquidacionGrupoResponseDto
{
    // ===== Identificadores de agrupacion =====
    public int IdSede { get; set; }
    public string? CodigoLiquidacion { get; set; }
    public string? NumeroLiquidacion { get; set; }
    public int? IdBanco { get; set; }

    // ===== Banco =====
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }

    // ===== Tipos con descripciones =====
    public string? TipoProduccion { get; set; }
    public string? DesTipoProduccion { get; set; }
    public string? TipoMedico { get; set; }
    public string? DesTipoMedico { get; set; }
    public string? TipoRubro { get; set; }
    public string? DesTipoRubro { get; set; }

    // ===== Descripcion y periodo =====
    public string? Descripcion { get; set; }
    public string? Periodo { get; set; }

    // ===== Totales agregados =====
    public decimal? MtoTotal { get; set; }
    public int CantidadFacturas { get; set; }
}
