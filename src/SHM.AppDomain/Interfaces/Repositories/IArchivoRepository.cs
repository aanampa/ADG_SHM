using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para operaciones de acceso a datos de Archivo.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IArchivoRepository
{
    /// <summary>
    /// Obtiene todos los archivos.
    /// </summary>
    Task<IEnumerable<Archivo>> GetAllAsync();

    /// <summary>
    /// Obtiene un archivo por su ID.
    /// </summary>
    Task<Archivo?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un archivo por su GUID.
    /// </summary>
    Task<Archivo?> GetByGuidAsync(string guid);

    /// <summary>
    /// Crea un nuevo archivo y retorna el ID generado.
    /// </summary>
    Task<int> CreateAsync(Archivo archivo);

    /// <summary>
    /// Actualiza un archivo existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Archivo archivo);

    /// <summary>
    /// Elimina (soft delete) un archivo.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe un archivo con el ID especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
