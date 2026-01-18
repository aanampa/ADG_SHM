using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Opcion;

/// <summary>
/// DTO para la actualizacion de una opcion de menu existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateOpcionDto
{
    [MaxLength(100)]
    public string? Nombre { get; set; }

    [MaxLength(255)]
    public string? Url { get; set; }

    [MaxLength(255)]
    public string? Icono { get; set; }

    public int? Orden { get; set; }

    public int? IdOpcionPadre { get; set; }

    public int? Activo { get; set; }
}
