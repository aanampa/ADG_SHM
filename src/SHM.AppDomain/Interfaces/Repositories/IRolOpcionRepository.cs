using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de relaciones rol-opcion en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IRolOpcionRepository
{
    /// <summary>
    /// Obtiene todas las relaciones rol-opcion registradas en el sistema.
    /// </summary>
    Task<IEnumerable<RolOpcion>> GetAllAsync();

    /// <summary>
    /// Obtiene una relacion rol-opcion por sus identificadores compuestos.
    /// </summary>
    Task<RolOpcion?> GetByIdAsync(int idRol, int idOpcion);

    /// <summary>
    /// Obtiene todas las opciones asociadas a un rol especifico.
    /// </summary>
    Task<IEnumerable<RolOpcion>> GetByRolAsync(int idRol);

    /// <summary>
    /// Obtiene todos los roles asociados a una opcion especifica.
    /// </summary>
    Task<IEnumerable<RolOpcion>> GetByOpcionAsync(int idOpcion);

    /// <summary>
    /// Crea una nueva relacion rol-opcion en la base de datos.
    /// </summary>
    Task<bool> CreateAsync(RolOpcion rolOpcion);

    /// <summary>
    /// Actualiza los datos de una relacion rol-opcion existente.
    /// </summary>
    Task<bool> UpdateAsync(int idRol, int idOpcion, RolOpcion rolOpcion);

    /// <summary>
    /// Elimina una relacion rol-opcion registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int idRol, int idOpcion, int idModificador);

    /// <summary>
    /// Verifica si existe una relacion rol-opcion con los identificadores especificados.
    /// </summary>
    Task<bool> ExistsAsync(int idRol, int idOpcion);
}
