using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para la actualizacion de datos de liquidacion a traves de la interface.
/// Permite recibir codigos en lugar de IDs para facilitar la integracion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-31</created>
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
}
