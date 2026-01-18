namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un usuario del sistema.
/// Mapea a la tabla SHM_SEG_USUARIO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string TipoUsuario { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? TokenRecuperacion { get; set; }
    public DateTime? FechaExpiracionToken { get; set; }
}
