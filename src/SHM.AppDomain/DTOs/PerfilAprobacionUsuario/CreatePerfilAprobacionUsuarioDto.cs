namespace SHM.AppDomain.DTOs.PerfilAprobacionUsuario;

/// <summary>
/// DTO para la creacion de una relacion perfil de aprobacion - usuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class CreatePerfilAprobacionUsuarioDto
{
    public int IdPerfilAprobacion { get; set; }
    public int IdUsuario { get; set; }
    public int? IdSede { get; set; }
}
