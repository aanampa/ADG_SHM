using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Opcion;

/// <summary>
/// DTO para la creacion de una nueva opcion de menu.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateOpcionDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Url { get; set; }

    [MaxLength(255)]
    public string? Icono { get; set; }

    public int? Orden { get; set; }

    public int? IdOpcionPadre { get; set; }
}
