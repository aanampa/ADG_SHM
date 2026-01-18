using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de sedes en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface ISedeRepository
{
    /// <summary>
    /// Obtiene todas las sedes registradas en el sistema.
    /// </summary>
    Task<IEnumerable<Sede>> GetAllAsync();

    /// <summary>
    /// Obtiene una sede por su identificador unico.
    /// </summary>
    Task<Sede?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene una sede por su codigo.
    /// </summary>
    Task<Sede?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una sede por su GUID de registro.
    /// </summary>
    Task<Sede?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Crea una nueva sede en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Sede sede);

    /// <summary>
    /// Actualiza los datos de una sede existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Sede sede);

    /// <summary>
    /// Elimina una sede registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una sede con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
