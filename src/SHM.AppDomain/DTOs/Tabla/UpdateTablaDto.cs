using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Tabla;

/// <summary>
/// DTO para la actualizacion de una tabla maestra existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateTablaDto
{
    [MaxLength(50)]
    public string? Codigo { get; set; }

    [MaxLength(250)]
    public string? Descripcion { get; set; }

    public int? Activo { get; set; }
}
