using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para la creacion de producciones a traves de la interface.
/// Permite recibir codigos en lugar de IDs para facilitar la integracion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-01-31 - Agregado campo FechaProduccion</modified>
/// <modified>ADG Antonio - 2026-01-31 - Quitado FechaCreacion, Concepto y campos liquidacion, nueva llave compuesta</modified>
/// </summary>
public class CreateInterfaceProduccionDto
{
    [Required]
    public string CodigoSede { get; set; } = string.Empty;

    [Required]
    public string CodigoEntidad { get; set; } = string.Empty;

    [Required]
    public string CodigoProduccion { get; set; } = string.Empty;

    public string? NumeroProduccion { get; set; }

    [Required]
    public string TipoProduccion { get; set; } = string.Empty;

    [Required]
    public string TipoEntidadMedica { get; set; } = string.Empty;

    [Required]
    public string TipoMedico { get; set; } = string.Empty;

    [Required]
    public string TipoRubro { get; set; } = string.Empty;

    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public string Periodo { get; set; } = string.Empty;

    [Required]
    public string FechaProduccion { get; set; } = string.Empty;

    [Required]
    public string EstadoProduccion { get; set; } = string.Empty;

    [Required]
    public decimal MtoConsumo { get; set; }

    [Required]
    public decimal MtoDescuento { get; set; }

    [Required]
    public decimal MtoSubtotal { get; set; }

    [Required]
    public decimal MtoRenta { get; set; }

    [Required]
    public decimal MtoIgv { get; set; }

    [Required]
    public decimal MtoTotal { get; set; }
}
