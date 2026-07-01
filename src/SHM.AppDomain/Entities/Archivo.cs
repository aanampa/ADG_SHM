namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un archivo almacenado en el sistema.
/// Mapea a la tabla SHM_ARCHIVO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Vladimir - 2026-01-29 - Agregado soporte para almacenamiento BLOB</modified>
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

    /// <summary>
    /// Contenido binario del archivo (solo cuando TipoAlmacenamiento = 'BLOB').
    /// </summary>
    public byte[]? ContenidoArchivo { get; set; }

    /// <summary>
    /// Tipo de almacenamiento: 'FILE' (sistema de archivos) o 'BLOB' (base de datos).
    /// </summary>
    public string? TipoAlmacenamiento { get; set; }
}
