namespace SHM.AppDomain.DTOs.Rol;

/// <summary>
/// DTO de respuesta con la informacion de un rol.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolResponseDto
{
    public int IdRol { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
