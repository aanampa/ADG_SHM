using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de parametros del sistema en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IParametroRepository
{
    /// <summary>
    /// Obtiene todos los parametros registrados en el sistema.
    /// </summary>
    Task<IEnumerable<Parametro>> GetAllAsync();

    /// <summary>
    /// Obtiene un parametro por su identificador unico.
    /// </summary>
    Task<Parametro?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un parametro por su GUID de registro.
    /// </summary>
    Task<Parametro?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene un parametro por su codigo.
    /// </summary>
    Task<Parametro?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Crea un nuevo parametro en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Parametro parametro);

    /// <summary>
    /// Actualiza los datos de un parametro existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Parametro parametro);

    /// <summary>
    /// Elimina un parametro registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe un parametro con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
