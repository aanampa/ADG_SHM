using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Usuario;

/// <summary>
/// DTO para la actualizacion de un usuario existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateUsuarioDto
{
    [StringLength(1)]
    public string? TipoUsuario { get; set; }

    [StringLength(100)]
    public string? Login { get; set; }

    [StringLength(255)]
    public string? Password { get; set; }

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

    public int? Activo { get; set; }
}
