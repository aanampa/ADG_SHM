namespace SHM.AppDomain.DTOs.Bitacora;

/// <summary>
/// DTO de respuesta con la informacion de un registro de bitacora incluyendo datos del usuario que realizo la accion.
/// Utilizado para mostrar la bitacora en vistas con informacion del responsable.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-29</created>
/// </summary>
public class BitacoraConUsuarioDto
{
    public int IdBitacora { get; set; }
    public int? IdEntidad { get; set; }
    public string? Entidad { get; set; }
    public string? Accion { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaAccion { get; set; }
    public int? IdCreador { get; set; }

    // Datos del usuario que realizo la accion
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }

    /// <summary>
    /// Obtiene el nombre completo del usuario que realizo la accion.
    /// </summary>
    public string NombreCompletoUsuario =>
        $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();
}
