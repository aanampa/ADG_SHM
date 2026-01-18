namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un registro de bitacora (log de auditoria).
/// Mapea a la tabla SHM_BITACORA en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Bitacora
{
    public int IdBitacora { get; set; }
    public string? Entidad { get; set; }
    public string? Accion { get; set; }
    public string? Descripcion { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
