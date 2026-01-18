using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de entidades medicas en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IEntidadMedicaRepository
{
    /// <summary>
    /// Obtiene todas las entidades medicas registradas en el sistema.
    /// </summary>
    Task<IEnumerable<EntidadMedica>> GetAllAsync();

    /// <summary>
    /// Obtiene una entidad medica por su identificador unico.
    /// </summary>
    Task<EntidadMedica?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene una entidad medica por su codigo.
    /// </summary>
    Task<EntidadMedica?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una entidad medica por su numero de RUC.
    /// </summary>
    Task<EntidadMedica?> GetByRucAsync(string ruc);

    /// <summary>
    /// Crea una nueva entidad medica en la base de datos.
    /// </summary>
    Task<int> CreateAsync(EntidadMedica entidadMedica);

    /// <summary>
    /// Actualiza los datos de una entidad medica existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, EntidadMedica entidadMedica);

    /// <summary>
    /// Elimina una entidad medica registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una entidad medica con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Obtiene una entidad medica por su GUID de registro.
    /// </summary>
    Task<EntidadMedica?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene entidades medicas de forma paginada con opcion de busqueda.
    /// </summary>
    Task<(IEnumerable<EntidadMedica> Items, int TotalCount)> GetPaginatedAsync(string? searchTerm, int pageNumber, int pageSize);
}
