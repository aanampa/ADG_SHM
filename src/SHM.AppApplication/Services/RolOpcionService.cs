using SHM.AppDomain.DTOs.RolOpcion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de la relacion entre roles y opciones del menu, incluyendo operaciones CRUD y consultas por rol u opcion
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolOpcionService : IRolOpcionService
{
    private readonly IRolOpcionRepository _rolOpcionRepository;

    public RolOpcionService(IRolOpcionRepository rolOpcionRepository)
    {
        _rolOpcionRepository = rolOpcionRepository;
    }

    /// <summary>
    /// Obtiene todas las relaciones rol-opcion del sistema
    /// </summary>
    public async Task<IEnumerable<RolOpcionResponseDto>> GetAllRolOpcionesAsync()
    {
        var rolOpciones = await _rolOpcionRepository.GetAllAsync();
        return rolOpciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una relacion rol-opcion por sus identificadores
    /// </summary>
    public async Task<RolOpcionResponseDto?> GetRolOpcionByIdAsync(int idRol, int idOpcion)
    {
        var rolOpcion = await _rolOpcionRepository.GetByIdAsync(idRol, idOpcion);
        return rolOpcion != null ? MapToResponseDto(rolOpcion) : null;
    }

    /// <summary>
    /// Obtiene las opciones asignadas a un rol especifico
    /// </summary>
    public async Task<IEnumerable<RolOpcionResponseDto>> GetOpcionesByRolAsync(int idRol)
    {
        var rolOpciones = await _rolOpcionRepository.GetByRolAsync(idRol);
        return rolOpciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene los roles que tienen asignada una opcion especifica
    /// </summary>
    public async Task<IEnumerable<RolOpcionResponseDto>> GetRolesByOpcionAsync(int idOpcion)
    {
        var rolOpciones = await _rolOpcionRepository.GetByOpcionAsync(idOpcion);
        return rolOpciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva relacion entre un rol y una opcion
    /// </summary>
    public async Task<RolOpcionResponseDto> CreateRolOpcionAsync(CreateRolOpcionDto createDto, int idCreador)
    {
        var rolOpcion = new RolOpcion
        {
            IdRol = createDto.IdRol,
            IdOpcion = createDto.IdOpcion,
            IdCreador = idCreador,
            Activo = 1
        };

        await _rolOpcionRepository.CreateAsync(rolOpcion);
        var createdRolOpcion = await _rolOpcionRepository.GetByIdAsync(createDto.IdRol, createDto.IdOpcion);

        return MapToResponseDto(createdRolOpcion!);
    }

    /// <summary>
    /// Actualiza una relacion rol-opcion existente
    /// </summary>
    public async Task<bool> UpdateRolOpcionAsync(int idRol, int idOpcion, UpdateRolOpcionDto updateDto, int idModificador)
    {
        var rolOpcionExistente = await _rolOpcionRepository.GetByIdAsync(idRol, idOpcion);
        if (rolOpcionExistente == null)
            return false;

        if (updateDto.Activo.HasValue)
            rolOpcionExistente.Activo = updateDto.Activo.Value;

        rolOpcionExistente.IdModificador = idModificador;

        return await _rolOpcionRepository.UpdateAsync(idRol, idOpcion, rolOpcionExistente);
    }

    /// <summary>
    /// Elimina una relacion rol-opcion por sus identificadores
    /// </summary>
    public async Task<bool> DeleteRolOpcionAsync(int idRol, int idOpcion, int idModificador)
    {
        var exists = await _rolOpcionRepository.ExistsAsync(idRol, idOpcion);
        if (!exists)
            return false;

        return await _rolOpcionRepository.DeleteAsync(idRol, idOpcion, idModificador);
    }

    private static RolOpcionResponseDto MapToResponseDto(RolOpcion rolOpcion)
    {
        return new RolOpcionResponseDto
        {
            IdRol = rolOpcion.IdRol,
            IdOpcion = rolOpcion.IdOpcion,
            Activo = rolOpcion.Activo,
            GuidRegistro = rolOpcion.GuidRegistro,
            FechaCreacion = rolOpcion.FechaCreacion,
            FechaModificacion = rolOpcion.FechaModificacion
        };
    }
}
