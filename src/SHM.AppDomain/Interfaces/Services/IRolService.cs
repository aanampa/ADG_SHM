using SHM.AppDomain.DTOs.Rol;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de roles en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IRolService
{
    /// <summary>
    /// Obtiene todos los roles registrados en el sistema.
    /// </summary>
    Task<IEnumerable<RolResponseDto>> GetAllRolesAsync();

    /// <summary>
    /// Obtiene un rol por su identificador unico.
    /// </summary>
    Task<RolResponseDto?> GetRolByIdAsync(int id);

    /// <summary>
    /// Obtiene un rol por su GUID de registro.
    /// </summary>
    Task<RolResponseDto?> GetRolByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene un rol por su codigo.
    /// </summary>
    Task<RolResponseDto?> GetRolByCodigoAsync(string codigo);

    /// <summary>
    /// Crea un nuevo rol en el sistema.
    /// </summary>
    Task<RolResponseDto> CreateRolAsync(CreateRolDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un rol existente.
    /// </summary>
    Task<bool> UpdateRolAsync(int id, UpdateRolDto updateDto, int idModificador);

    /// <summary>
    /// Elimina un rol registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteRolAsync(int id, int idModificador);
}
