namespace SHM.AppDomain.DTOs.Opcion;

/// <summary>
/// DTO de respuesta con la informacion de una opcion de menu.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class OpcionResponseDto
{
    public int IdOpcion { get; set; }
    public string? Nombre { get; set; }
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int? Orden { get; set; }
    public int? IdOpcionPadre { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
