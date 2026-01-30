namespace SHM.AppDomain.DTOs.Parametro;

/// <summary>
/// DTO de respuesta con la informacion de un parametro.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ParametroResponseDto
{
    public int IdParametro { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Valor { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
