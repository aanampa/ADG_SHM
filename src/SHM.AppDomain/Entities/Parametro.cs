namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un parametro de configuracion del sistema.
/// Mapea a la tabla SHM_PARAMETRO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Parametro
{
    public int IdParametro { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Valor { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
