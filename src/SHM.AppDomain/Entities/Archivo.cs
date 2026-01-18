namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un archivo almacenado en el sistema.
/// Mapea a la tabla SHM_ARCHIVO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Archivo
{
    public int IdArchivo { get; set; }
    public string? TipoArchivo { get; set; }
    public string? NombreOriginal { get; set; }
    public string? NombreArchivo { get; set; }
    public string? Extension { get; set; }
    public int? Tamano { get; set; }
    public string? Ruta { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
