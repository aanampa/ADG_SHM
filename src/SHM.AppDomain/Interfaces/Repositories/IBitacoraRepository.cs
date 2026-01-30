using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de bitacoras de auditoria en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IBitacoraRepository
{
    /// <summary>
    /// Obtiene todas las bitacoras registradas en el sistema.
    /// </summary>
    Task<IEnumerable<Bitacora>> GetAllAsync();

    /// <summary>
    /// Obtiene una bitacora por su identificador unico.
    /// </summary>
    Task<Bitacora?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todas las bitacoras asociadas a una entidad especifica.
    /// </summary>
    Task<IEnumerable<Bitacora>> GetByEntidadAsync(string entidad);

    /// <summary>
    /// Crea un nuevo registro de bitacora en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Bitacora bitacora);

    /// <summary>
    /// Actualiza los datos de una bitacora existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Bitacora bitacora);

    /// <summary>
    /// Elimina una bitacora registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una bitacora con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Obtiene las bitacoras de una entidad especifica por su ID, incluyendo datos del usuario que realizo la accion.
    /// </summary>
    /// <param name="entidad">Nombre de la entidad (ej: SHM_PRODUCCION)</param>
    /// <param name="idEntidad">ID de la entidad</param>
    /// <returns>Lista de bitacoras con datos del usuario</returns>
    Task<IEnumerable<BitacoraConUsuarioDto>> GetByEntidadYIdConUsuarioAsync(string entidad, int idEntidad);
}
