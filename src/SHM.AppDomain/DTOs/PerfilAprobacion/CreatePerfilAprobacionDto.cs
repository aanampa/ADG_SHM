namespace SHM.AppDomain.DTOs.PerfilAprobacion;

/// <summary>
/// DTO para la creacion de un perfil de aprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class CreatePerfilAprobacionDto
{
    public string? GrupoFlujoTrabajo { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Nivel { get; set; }
    public int Orden { get; set; }
}
