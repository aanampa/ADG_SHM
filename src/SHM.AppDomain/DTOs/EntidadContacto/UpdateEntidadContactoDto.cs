namespace SHM.AppDomain.DTOs.EntidadContacto;

/// <summary>
/// DTO para la actualizacion de un contacto de entidad medica existente.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public class UpdateEntidadContactoDto
{
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Cargo { get; set; }
    public int? Activo { get; set; }
}
