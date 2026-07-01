namespace SHM.AppDomain.DTOs.Archivo;

/// <summary>
/// DTO para la creacion de un nuevo archivo.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Vladimir - 2026-01-29 - Agregado soporte para almacenamiento BLOB</modified>
/// </summary>
public class CreateArchivoDto
{
    public string? TipoArchivo { get; set; }
    public string? NombreOriginal { get; set; }
    public string? NombreArchivo { get; set; }
    public string? Extension { get; set; }
    public int? Tamano { get; set; }
    public string? Ruta { get; set; }

    /// <summary>
    /// Contenido binario del archivo para almacenamiento BLOB.
    /// Si es null, se utiliza almacenamiento FILE con la propiedad Ruta.
    /// </summary>
    public byte[]? ContenidoArchivo { get; set; }
}
