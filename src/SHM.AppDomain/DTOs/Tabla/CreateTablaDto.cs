using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Tabla;

/// <summary>
/// DTO para la creacion de una nueva tabla maestra.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateTablaDto
{
    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descripcion { get; set; }
}
