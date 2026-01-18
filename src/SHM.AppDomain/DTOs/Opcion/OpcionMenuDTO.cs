namespace SHM.AppDomain.DTOs.Opcion;

/// <summary>
/// DTO para la estructura del menu de navegacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class OpcionMenuDTO
{
    public int IdOpcion { get; set; }
    public string? Nombre { get; set; }
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int IdOpcionPadre { get; set; }
    public int Orden { get; set; }
}