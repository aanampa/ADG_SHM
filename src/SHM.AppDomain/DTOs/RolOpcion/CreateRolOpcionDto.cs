using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.RolOpcion;

/// <summary>
/// DTO para la creacion de una nueva relacion rol-opcion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateRolOpcionDto
{
    [Required]
    public int IdRol { get; set; }

    [Required]
    public int IdOpcion { get; set; }
}
