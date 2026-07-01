namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un contacto de notificacion de una entidad medica.
/// Mapea a la tabla SHM_ENTIDAD_CONTACTO en la base de datos.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public class EntidadContacto
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
    public int Activo { get; set; } = 1;
    public int? IdCreador { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
