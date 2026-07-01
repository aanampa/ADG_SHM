namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa la relacion entre un usuario y una sede.
/// Mapea a la tabla SHM_SEG_USUARIO_SEDE en la base de datos.
/// Un usuario interno puede pertenecer a una o varias sedes.
///
/// <author>Vladimir</author>
/// <created>2026-02-02</created>
/// </summary>
public class UsuarioSede
{
    public int IdUsuario { get; set; }
    public int IdSede { get; set; }
    public string? GuidRegistro { get; set; }
    public int EsUltimaSede { get; set; }
    public int Activo { get; set; }
    public int? IdCreador { get; set; }
    public DateTime? FechaCreacion { get; set; }
}
