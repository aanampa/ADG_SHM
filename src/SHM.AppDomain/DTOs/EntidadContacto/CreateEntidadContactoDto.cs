namespace SHM.AppDomain.DTOs.EntidadContacto;

/// <summary>
/// DTO para la creacion de un nuevo contacto de entidad medica.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public class CreateEntidadContactoDto
{
    public int IdEntidadMedica { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string? Celular { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Cargo { get; set; }
}
