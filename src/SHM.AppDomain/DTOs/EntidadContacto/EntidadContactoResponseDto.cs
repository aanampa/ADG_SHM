namespace SHM.AppDomain.DTOs.EntidadContacto;

/// <summary>
/// DTO de respuesta con la informacion de un contacto de entidad medica.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public class EntidadContactoResponseDto
{
    public int IdEntidadContacto { get; set; }
    public int? IdEntidadMedica { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string? Celular { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
