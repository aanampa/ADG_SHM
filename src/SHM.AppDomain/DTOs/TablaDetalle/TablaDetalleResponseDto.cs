namespace SHM.AppDomain.DTOs.TablaDetalle;

/// <summary>
/// DTO de respuesta con la informacion de un detalle de tabla.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaDetalleResponseDto
{
    public int IdTablaDetalle { get; set; }
    public int IdTabla { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
