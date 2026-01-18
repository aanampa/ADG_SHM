namespace SHM.AppDomain.DTOs.ArchivoComprobante;

/// <summary>
/// DTO de respuesta con la informacion de una relacion archivo-comprobante.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ArchivoComprobanteResponseDto
{
    public int IdArchivoComprobante { get; set; }
    public int? IdProduccion { get; set; }
    public int? IdArchivo { get; set; }
    public string? TipoArchivo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
