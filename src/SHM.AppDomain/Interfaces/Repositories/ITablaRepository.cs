using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de tablas maestras en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface ITablaRepository
{
    /// <summary>
    /// Obtiene todas las tablas maestras registradas en el sistema.
    /// </summary>
    Task<IEnumerable<Tabla>> GetAllAsync();

    /// <summary>
    /// Obtiene una tabla por su identificador unico.
    /// </summary>
    Task<Tabla?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene una tabla por su GUID de registro.
    /// </summary>
    Task<Tabla?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene una tabla por su codigo.
    /// </summary>
    Task<Tabla?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Crea una nueva tabla maestra en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Tabla tabla);

    /// <summary>
    /// Actualiza los datos de una tabla existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Tabla tabla);

    /// <summary>
    /// Elimina una tabla por su identificador.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Verifica si existe una tabla con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
