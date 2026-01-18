namespace SHM.AppDomain.DTOs.Usuario;

/// <summary>
/// DTO de respuesta con la informacion de un usuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UsuarioResponseDto
{
    public int IdUsuario { get; set; }
    public string TipoUsuario { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Nombres { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Celular { get; set; }
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public int? IdEntidadMedica { get; set; }
    public int? IdRol { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
