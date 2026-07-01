namespace SHM.AppDomain.DTOs.Archivo;

/// <summary>
/// DTO de respuesta con la informacion de un archivo.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Vladimir - 2026-01-29 - Agregado soporte para almacenamiento BLOB</modified>
/// </summary>
public class ArchivoResponseDto
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
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Tipo de almacenamiento: 'FILE' (sistema de archivos) o 'BLOB' (base de datos).
    /// </summary>
    public string? TipoAlmacenamiento { get; set; }
}
