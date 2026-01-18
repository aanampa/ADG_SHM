namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa la relacion entre rol y opcion (permisos).
/// Mapea a la tabla SHM_SEG_ROL_OPCION en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolOpcion
{
    public int IdRol { get; set; }
    public int IdOpcion { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
