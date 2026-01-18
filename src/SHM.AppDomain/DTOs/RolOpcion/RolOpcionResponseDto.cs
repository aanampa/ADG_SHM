namespace SHM.AppDomain.DTOs.RolOpcion;

/// <summary>
/// DTO de respuesta con la informacion de una relacion rol-opcion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolOpcionResponseDto
{
    public int IdRol { get; set; }
    public int IdOpcion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
