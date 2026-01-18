namespace SHM.AppDomain.DTOs.Archivo;

/// <summary>
/// DTO para la creacion de un nuevo archivo.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateArchivoDto
{
    public string? TipoArchivo { get; set; }
    public string? NombreOriginal { get; set; }
    public string? NombreArchivo { get; set; }
    public string? Extension { get; set; }
    public int? Tamano { get; set; }
    public string? Ruta { get; set; }
}
