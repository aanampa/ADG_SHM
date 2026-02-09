namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa la relacion entre perfil de aprobacion y usuario.
/// Mapea a la tabla SHM_PERFIL_APROBACION_USUARIO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionUsuario
{
    public int IdPerfilAprobacion { get; set; }
    public int IdUsuario { get; set; }
    public int? IdSede { get; set; }

    // Propiedades de navegacion
    public string? NombreUsuario { get; set; }
    public string? NombrePerfil { get; set; }
    public string? NombreSede { get; set; }
}
