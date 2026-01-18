namespace SHM.AppDomain.DTOs.Tabla;

/// <summary>
/// DTO de respuesta con la informacion de una tabla maestra.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaResponseDto
{
    public int IdTabla { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
