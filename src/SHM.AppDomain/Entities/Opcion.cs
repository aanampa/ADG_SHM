namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa una opcion de menu del sistema.
/// Mapea a la tabla SHM_SEG_OPCION en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Opcion
{
    public int IdOpcion { get; set; }
    public string? Nombre { get; set; }
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int? Orden { get; set; }
    public int? IdOpcionPadre { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
