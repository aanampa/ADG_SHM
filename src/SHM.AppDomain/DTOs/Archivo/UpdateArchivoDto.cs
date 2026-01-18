namespace SHM.AppDomain.DTOs.Archivo;

/// <summary>
/// DTO para la actualizacion de un archivo existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateArchivoDto
{
    public string? TipoArchivo { get; set; }
    public string? NombreOriginal { get; set; }
    public string? NombreArchivo { get; set; }
    public string? Extension { get; set; }
    public int? Tamano { get; set; }
    public string? Ruta { get; set; }
    public int? Activo { get; set; }
}
