using SHM.AppDomain.DTOs.PerfilAprobacionUsuario;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de relaciones perfil de aprobacion - usuario.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionUsuarioService : IPerfilAprobacionUsuarioService
{
    private readonly IPerfilAprobacionUsuarioRepository _repository;

    public PerfilAprobacionUsuarioService(IPerfilAprobacionUsuarioRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todas las relaciones perfil-usuario.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionUsuarioResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una relacion por sus identificadores compuestos.
    /// </summary>
    public async Task<PerfilAprobacionUsuarioResponseDto?> GetByIdAsync(int idPerfilAprobacion, int idUsuario)
    {
        var item = await _repository.GetByIdAsync(idPerfilAprobacion, idUsuario);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene todos los usuarios de un perfil de aprobacion.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionUsuarioResponseDto>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion)
    {
        var items = await _repository.GetByPerfilAprobacionIdAsync(idPerfilAprobacion);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todos los perfiles de aprobacion de un usuario.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionUsuarioResponseDto>> GetByUsuarioIdAsync(int idUsuario)
    {
        var items = await _repository.GetByUsuarioIdAsync(idUsuario);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva relacion perfil-usuario.
    /// </summary>
    public async Task<PerfilAprobacionUsuarioResponseDto> CreateAsync(CreatePerfilAprobacionUsuarioDto dto)
    {
        var entity = new PerfilAprobacionUsuario
        {
            IdPerfilAprobacion = dto.IdPerfilAprobacion,
            IdUsuario = dto.IdUsuario,
            IdSede = dto.IdSede
        };

        await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(dto.IdPerfilAprobacion, dto.IdUsuario);

        return MapToResponseDto(created!);
    }

    /// <summary>
    /// Elimina una relacion perfil-usuario.
    /// </summary>
    public async Task<bool> DeleteAsync(int idPerfilAprobacion, int idUsuario)
    {
        return await _repository.DeleteAsync(idPerfilAprobacion, idUsuario);
    }

    private static PerfilAprobacionUsuarioResponseDto MapToResponseDto(PerfilAprobacionUsuario entity)
    {
        return new PerfilAprobacionUsuarioResponseDto
        {
            IdPerfilAprobacion = entity.IdPerfilAprobacion,
            IdUsuario = entity.IdUsuario,
            IdSede = entity.IdSede,
            NombreUsuario = entity.NombreUsuario,
            NombrePerfil = entity.NombrePerfil,
            NombreSede = entity.NombreSede
        };
    }
}
