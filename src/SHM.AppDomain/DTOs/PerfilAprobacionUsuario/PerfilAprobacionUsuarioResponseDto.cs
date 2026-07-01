namespace SHM.AppDomain.DTOs.PerfilAprobacionUsuario;

/// <summary>
/// DTO de respuesta para la relacion perfil de aprobacion - usuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionUsuarioResponseDto
{
    public int IdPerfilAprobacion { get; set; }
    public int IdUsuario { get; set; }
    public int? IdSede { get; set; }
    public string? NombreUsuario { get; set; }
    public string? NombrePerfil { get; set; }
    public string? NombreSede { get; set; }
}
