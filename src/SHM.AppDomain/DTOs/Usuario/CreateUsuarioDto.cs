using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Usuario;

/// <summary>
/// DTO para la creacion de un nuevo usuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateUsuarioDto
{
    [Required]
    [StringLength(1)]
    public string TipoUsuario { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? NumeroDocumento { get; set; }

    [StringLength(100)]
    public string? Nombres { get; set; }

    [StringLength(100)]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100)]
    public string? ApellidoMaterno { get; set; }

    [StringLength(10)]
    public string? Celular { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [StringLength(120)]
    public string? Cargo { get; set; }

    public int? IdEntidadMedica { get; set; }

    public int? IdRol { get; set; }
}
