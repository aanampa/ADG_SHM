using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Rol;

/// <summary>
/// DTO para la actualizacion de un rol existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateRolDto
{
    [MaxLength(100)]
    public string? Codigo { get; set; }

    [MaxLength(100)]
    public string? Descripcion { get; set; }

    public int? Activo { get; set; }
}
