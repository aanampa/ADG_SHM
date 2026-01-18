namespace SHM.AppDomain.DTOs.Sede;

/// <summary>
/// DTO de respuesta con la informacion de una sede.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class SedeResponseDto
{
    public int IdSede { get; set; }
    public int? IdCorporacion { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Ruc { get; set; }
    public string? Direccion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
