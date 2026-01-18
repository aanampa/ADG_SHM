using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de bancos en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IBancoRepository
{
    /// <summary>
    /// Obtiene todos los bancos registrados en el sistema.
    /// </summary>
    Task<IEnumerable<Banco>> GetAllAsync();

    /// <summary>
    /// Obtiene un banco por su identificador unico.
    /// </summary>
    Task<Banco?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un banco por su codigo.
    /// </summary>
    Task<Banco?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene un banco por su GUID de registro.
    /// </summary>
    Task<Banco?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Crea un nuevo banco en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Banco banco);

    /// <summary>
    /// Actualiza los datos de un banco existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Banco banco);

    /// <summary>
    /// Elimina un banco registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe un banco con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
