using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de roles en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IRolRepository
{
    /// <summary>
    /// Obtiene todos los roles registrados en el sistema.
    /// </summary>
    Task<IEnumerable<Rol>> GetAllAsync();

    /// <summary>
    /// Obtiene un rol por su identificador unico.
    /// </summary>
    Task<Rol?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un rol por su GUID de registro.
    /// </summary>
    Task<Rol?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene un rol por su codigo.
    /// </summary>
    Task<Rol?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Crea un nuevo rol en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Rol rol);

    /// <summary>
    /// Actualiza los datos de un rol existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Rol rol);

    /// <summary>
    /// Elimina un rol registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe un rol con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
