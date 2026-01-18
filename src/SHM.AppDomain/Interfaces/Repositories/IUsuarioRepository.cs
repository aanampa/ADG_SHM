using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de usuarios en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Obtiene todos los usuarios registrados en el sistema.
    /// </summary>
    Task<IEnumerable<Usuario>> GetAllAsync();

    /// <summary>
    /// Obtiene un usuario por su identificador unico.
    /// </summary>
    Task<Usuario?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un usuario por su nombre de login.
    /// </summary>
    Task<Usuario?> GetByLoginAsync(string login);

    /// <summary>
    /// Crea un nuevo usuario en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Usuario usuario);

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Usuario usuario);

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Verifica si existe un usuario con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Obtiene un usuario por su direccion de correo electronico.
    /// </summary>
    Task<Usuario?> GetByEmailAsync(string email);

    /// <summary>
    /// Obtiene un usuario por su token de recuperacion de contrasena.
    /// </summary>
    Task<Usuario?> GetByTokenRecuperacionAsync(string token);

    /// <summary>
    /// Actualiza el token de recuperacion de contrasena para un usuario.
    /// </summary>
    Task<bool> UpdateTokenRecuperacionAsync(int idUsuario, string token, DateTime fechaExpiracion);

    /// <summary>
    /// Actualiza la contrasena de un usuario.
    /// </summary>
    Task<bool> UpdatePasswordAsync(int idUsuario, string newPasswordHash);

    /// <summary>
    /// Limpia el token de recuperacion de un usuario despues de su uso.
    /// </summary>
    Task<bool> ClearTokenRecuperacionAsync(int idUsuario);

    /// <summary>
    /// Obtiene las opciones de menu disponibles para un usuario segun su login.
    /// </summary>
    Task<IEnumerable<OpcionMenuDTO>> GetMenuByLoginAsync(string login);

    /// <summary>
    /// Actualiza los datos personales de un usuario.
    /// </summary>
    Task<bool> UpdateMisDatosAsync(int idUsuario, string? nombres, string? apellidoPaterno, string? apellidoMaterno, string? email, string? numeroDocumento, string? celular);

    /// <summary>
    /// Obtiene un usuario por su GUID de registro.
    /// </summary>
    Task<Usuario?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene usuarios externos de forma paginada con opcion de busqueda.
    /// </summary>
    Task<(IEnumerable<Usuario> Items, int TotalCount)> GetPaginatedExternosAsync(string? searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene usuarios internos de forma paginada con opcion de busqueda.
    /// </summary>
    Task<(IEnumerable<Usuario> Items, int TotalCount)> GetPaginatedInternosAsync(string? searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// Elimina un usuario registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);
}
