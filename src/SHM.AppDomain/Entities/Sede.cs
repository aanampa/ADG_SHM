namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa una sede del grupo corporativo.
/// Mapea a la tabla SHM_SEDE en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Sede
{
    public int IdSede { get; set; }
    public int? IdCorporacion { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Ruc { get; set; }
    public string? Direccion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
