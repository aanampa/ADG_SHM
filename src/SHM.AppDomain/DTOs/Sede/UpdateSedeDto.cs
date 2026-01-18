using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Sede;

/// <summary>
/// DTO para la actualizacion de una sede existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateSedeDto
{
    public int? IdCorporacion { get; set; }

    [MaxLength(5)]
    public string? Codigo { get; set; }

    [MaxLength(100)]
    public string? Nombre { get; set; }

    [MaxLength(20)]
    public string? Ruc { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    public int? Activo { get; set; }
}
