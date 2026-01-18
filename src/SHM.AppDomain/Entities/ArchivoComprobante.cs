namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa la relacion entre archivo y comprobante/produccion.
/// Mapea a la tabla SHM_ARCHIVO_COMPROBANTE en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ArchivoComprobante
{
    public int IdArchivoComprobante { get; set; }
    public int? IdProduccion { get; set; }
    public int? IdArchivo { get; set; }
    public string? TipoArchivo { get; set; }
    public string? Descripcion { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
