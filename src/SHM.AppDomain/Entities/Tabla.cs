namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa una tabla maestra del sistema.
/// Mapea a la tabla SHM_TABLA en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Tabla
{
    public int IdTabla { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
