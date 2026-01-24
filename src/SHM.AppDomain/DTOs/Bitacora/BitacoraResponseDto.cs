namespace SHM.AppDomain.DTOs.Bitacora;

/// <summary>
/// DTO de respuesta con la informacion de un registro de bitacora.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos IdEntidad y FechaAccion</modified>
/// </summary>
public class BitacoraResponseDto
{
    public int IdBitacora { get; set; }
    public int? IdEntidad { get; set; }
    public string? Entidad { get; set; }
    public string? Accion { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaAccion { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
