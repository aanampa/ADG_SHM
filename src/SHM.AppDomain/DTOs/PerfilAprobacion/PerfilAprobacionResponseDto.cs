namespace SHM.AppDomain.DTOs.PerfilAprobacion;

/// <summary>
/// DTO de respuesta para el perfil de aprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionResponseDto
{
    public int IdPerfilAprobacion { get; set; }
    public string? GrupoFlujoTrabajo { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Nivel { get; set; }
    public int Orden { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdUsuarioCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
}
