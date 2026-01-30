namespace SHM.AppDomain.DTOs.Archivo;

/// <summary>
/// DTO para transferir el contenido binario de un archivo.
/// Utilizado para descargas y operaciones que requieren el contenido del archivo.
///
/// <author>ADG Vladimir</author>
/// <created>2026-01-29</created>
/// </summary>
public class ArchivoContenidoDto
{
    /// <summary>
    /// Contenido binario del archivo.
    /// </summary>
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Nombre del archivo para descarga.
    /// </summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>
    /// Extension del archivo (ej: .pdf, .xml).
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Tipo MIME del archivo (ej: application/pdf).
    /// </summary>
    public string? ContentType { get; set; }
}
