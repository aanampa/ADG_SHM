using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Sede;

/// <summary>
/// DTO para la creacion de una nueva sede.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateSedeDto
{
    public int? IdCorporacion { get; set; }

    [Required]
    [MaxLength(5)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Ruc { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }
}
