using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de contactos de entidades medicas.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public interface IEntidadContactoRepository
{
    /// <summary>
    /// Obtiene contactos de una entidad medica. soloActivos=true para notificaciones, false para la lista UI.
    /// </summary>
    Task<IEnumerable<EntidadContacto>> GetByEntidadMedicaAsync(int idEntidadMedica, bool soloActivos = true);

    /// <summary>
    /// Obtiene un contacto por su identificador unico.
    /// </summary>
    Task<EntidadContacto?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un contacto por su GUID de registro.
    /// </summary>
    Task<EntidadContacto?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Crea un nuevo contacto para una entidad medica.
    /// </summary>
    Task<int> CreateAsync(EntidadContacto contacto);

    /// <summary>
    /// Actualiza los datos de un contacto existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, EntidadContacto contacto);

    /// <summary>
    /// Elimina logicamente un contacto.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe un contacto con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Verifica si ya existe un contacto con el mismo email en la entidad medica.
    /// </summary>
    Task<bool> ExisteEmailEnEntidadAsync(int idEntidadMedica, string email, int? excludeId = null);
}
