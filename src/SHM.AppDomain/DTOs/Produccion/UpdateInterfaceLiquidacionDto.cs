using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para la actualizacion de datos de liquidacion a traves de la interface.
/// Permite recibir codigos en lugar de IDs para facilitar la integracion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-31</created>
/// <modified>ADG Antonio - 2026-11-05 - Agregados CodSapCpm, CodSapDetrac, MtoDetraccion y PorcDetraccion</modified>
/// </summary>
public class UpdateInterfaceLiquidacionDto
{
    [Required]
    public string CodigoSede { get; set; } = string.Empty;

    [Required]
    public string CodigoEntidad { get; set; } = string.Empty;

    [Required]
    public string CodigoProduccion { get; set; } = string.Empty;

    [Required]
    public string NumeroProduccion { get; set; } = string.Empty;

    [Required]
    public string TipoEntidadMedica { get; set; } = string.Empty;

    // Campos de Liquidacion
    [Required]
    public string NumeroLiquidacion { get; set; } = string.Empty;

    [Required]
    public string CodigoLiquidacion { get; set; } = string.Empty;

    [Required]
    public string PeriodoLiquidacion { get; set; } = string.Empty;

    [Required]
    public string EstadoLiquidacion { get; set; } = string.Empty;

    [Required]
    public string FechaLiquidacion { get; set; } = string.Empty;

    [Required]
    public string DescripcionLiquidacion { get; set; } = string.Empty;

    public string? TipoLiquidacion { get; set; }

    // Datos SAP (todos opcionales). SAP es la fuente oficial: al liquidar,
    // MtoDetraccion y PorcDetraccion sobrescriben los valores de la produccion.

    /// <summary>Codigo SAP (numero de documento) de la factura. Maximo 18 caracteres.</summary>
    public string? CodSapCpm { get; set; }

    /// <summary>Codigo SAP (numero de documento) de la detraccion. Maximo 20 caracteres. Vacio si la factura no esta afecta.</summary>
    public string? CodSapDetrac { get; set; }

    /// <summary>Monto de la detraccion. Mayor o igual a 0, hasta 2 decimales. Nulo equivale a 0.</summary>
    public decimal? MtoDetraccion { get; set; }

    /// <summary>Porcentaje de la detraccion. Entre 0 y 100, hasta 2 decimales. Nulo equivale a 0.</summary>
    public decimal? PorcDetraccion { get; set; }
}
