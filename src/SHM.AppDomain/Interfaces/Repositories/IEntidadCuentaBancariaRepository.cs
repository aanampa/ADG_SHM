using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de cuentas bancarias de entidades en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IEntidadCuentaBancariaRepository
{
    /// <summary>
    /// Obtiene todas las cuentas bancarias de entidades registradas en el sistema.
    /// </summary>
    Task<IEnumerable<EntidadCuentaBancaria>> GetAllAsync();

    /// <summary>
    /// Obtiene una cuenta bancaria por su identificador unico.
    /// </summary>
    Task<EntidadCuentaBancaria?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todas las cuentas bancarias asociadas a una entidad especifica.
    /// </summary>
    Task<IEnumerable<EntidadCuentaBancaria>> GetByEntidadIdAsync(int idEntidad);

    /// <summary>
    /// Crea una nueva cuenta bancaria de entidad en la base de datos.
    /// </summary>
    Task<int> CreateAsync(EntidadCuentaBancaria entidadCuentaBancaria);

    /// <summary>
    /// Actualiza los datos de una cuenta bancaria existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, EntidadCuentaBancaria entidadCuentaBancaria);

    /// <summary>
    /// Elimina una cuenta bancaria registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una cuenta bancaria con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Obtiene una cuenta bancaria por su GUID de registro.
    /// </summary>
    Task<EntidadCuentaBancaria?> GetByGuidAsync(string guidRegistro);
}
