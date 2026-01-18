using SHM.AppDomain.DTOs.RolOpcion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de relaciones rol-opcion en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IRolOpcionService
{
    /// <summary>
    /// Obtiene todas las relaciones rol-opcion registradas en el sistema.
    /// </summary>
    Task<IEnumerable<RolOpcionResponseDto>> GetAllRolOpcionesAsync();

    /// <summary>
    /// Obtiene una relacion rol-opcion por sus identificadores compuestos.
    /// </summary>
    Task<RolOpcionResponseDto?> GetRolOpcionByIdAsync(int idRol, int idOpcion);

    /// <summary>
    /// Obtiene todas las opciones asociadas a un rol especifico.
    /// </summary>
    Task<IEnumerable<RolOpcionResponseDto>> GetOpcionesByRolAsync(int idRol);

    /// <summary>
    /// Obtiene todos los roles asociados a una opcion especifica.
    /// </summary>
    Task<IEnumerable<RolOpcionResponseDto>> GetRolesByOpcionAsync(int idOpcion);

    /// <summary>
    /// Crea una nueva relacion rol-opcion en el sistema.
    /// </summary>
    Task<RolOpcionResponseDto> CreateRolOpcionAsync(CreateRolOpcionDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una relacion rol-opcion existente.
    /// </summary>
    Task<bool> UpdateRolOpcionAsync(int idRol, int idOpcion, UpdateRolOpcionDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una relacion rol-opcion registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteRolOpcionAsync(int idRol, int idOpcion, int idModificador);
}
