using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Rol;

/// <summary>
/// DTO para la creacion de un nuevo rol.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateRolDto
{
    [Required]
    [MaxLength(100)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;
}
