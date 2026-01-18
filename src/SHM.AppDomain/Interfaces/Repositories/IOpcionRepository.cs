using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de opciones de menu en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IOpcionRepository
{
    /// <summary>
    /// Obtiene todas las opciones de menu registradas en el sistema.
    /// </summary>
    Task<IEnumerable<Opcion>> GetAllAsync();

    /// <summary>
    /// Obtiene una opcion de menu por su identificador unico.
    /// </summary>
    Task<Opcion?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene una opcion de menu por su GUID de registro.
    /// </summary>
    Task<Opcion?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene las opciones de menu hijas de una opcion padre especifica.
    /// </summary>
    Task<IEnumerable<Opcion>> GetByPadreAsync(int? idOpcionPadre);

    /// <summary>
    /// Crea una nueva opcion de menu en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Opcion opcion);

    /// <summary>
    /// Actualiza los datos de una opcion de menu existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Opcion opcion);

    /// <summary>
    /// Elimina una opcion de menu registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una opcion de menu con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
