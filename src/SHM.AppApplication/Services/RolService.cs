using SHM.AppDomain.DTOs.Rol;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de roles del sistema, incluyendo operaciones CRUD y consultas por codigo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;

    public RolService(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
    }

    /// <summary>
    /// Obtiene todos los roles del sistema
    /// </summary>
    public async Task<IEnumerable<RolResponseDto>> GetAllRolesAsync()
    {
        var roles = await _rolRepository.GetAllAsync();
        return roles.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un rol por su identificador unico
    /// </summary>
    public async Task<RolResponseDto?> GetRolByIdAsync(int id)
    {
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol != null ? MapToResponseDto(rol) : null;
    }

    /// <summary>
    /// Obtiene un rol por su GUID de registro
    /// </summary>
    public async Task<RolResponseDto?> GetRolByGuidAsync(string guidRegistro)
    {
        var rol = await _rolRepository.GetByGuidAsync(guidRegistro);
        return rol != null ? MapToResponseDto(rol) : null;
    }

    /// <summary>
    /// Obtiene un rol por su codigo
    /// </summary>
    public async Task<RolResponseDto?> GetRolByCodigoAsync(string codigo)
    {
        var rol = await _rolRepository.GetByCodigoAsync(codigo);
        return rol != null ? MapToResponseDto(rol) : null;
    }

    /// <summary>
    /// Crea un nuevo rol en el sistema
    /// </summary>
    public async Task<RolResponseDto> CreateRolAsync(CreateRolDto createDto, int idCreador)
    {
        var rol = new Rol
        {
            Codigo = createDto.Codigo,
            Descripcion = createDto.Descripcion,
            IdCreador = idCreador,
            Activo = 1
        };

        var idRol = await _rolRepository.CreateAsync(rol);
        var createdRol = await _rolRepository.GetByIdAsync(idRol);

        return MapToResponseDto(createdRol!);
    }

    /// <summary>
    /// Actualiza los datos de un rol existente
    /// </summary>
    public async Task<bool> UpdateRolAsync(int id, UpdateRolDto updateDto, int idModificador)
    {
        var rolExistente = await _rolRepository.GetByIdAsync(id);
        if (rolExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.Codigo))
            rolExistente.Codigo = updateDto.Codigo;

        if (!string.IsNullOrEmpty(updateDto.Descripcion))
            rolExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Activo.HasValue)
            rolExistente.Activo = updateDto.Activo.Value;

        rolExistente.IdModificador = idModificador;

        return await _rolRepository.UpdateAsync(id, rolExistente);
    }

    /// <summary>
    /// Elimina un rol por su identificador
    /// </summary>
    public async Task<bool> DeleteRolAsync(int id, int idModificador)
    {
        var exists = await _rolRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _rolRepository.DeleteAsync(id, idModificador);
    }

    private static RolResponseDto MapToResponseDto(Rol rol)
    {
        return new RolResponseDto
        {
            IdRol = rol.IdRol,
            Codigo = rol.Codigo,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo,
            GuidRegistro = rol.GuidRegistro,
            FechaCreacion = rol.FechaCreacion,
            FechaModificacion = rol.FechaModificacion
        };
    }
}
