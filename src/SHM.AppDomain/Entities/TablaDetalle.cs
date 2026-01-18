namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un detalle de tabla maestra.
/// Mapea a la tabla SHM_TABLA_DETALLE en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaDetalle
{
    public int IdTablaDetalle { get; set; }
    public int IdTabla { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
