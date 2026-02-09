using SHM.AppDomain.DTOs.PerfilAprobacion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de perfiles de aprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionService : IPerfilAprobacionService
{
    private readonly IPerfilAprobacionRepository _repository;

    public PerfilAprobacionService(IPerfilAprobacionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todos los perfiles de aprobacion.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todos los perfiles de aprobacion activos.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionResponseDto>> GetAllActiveAsync()
    {
        var items = await _repository.GetAllActiveAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un perfil de aprobacion por su identificador.
    /// </summary>
    public async Task<PerfilAprobacionResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene un perfil de aprobacion por su GUID.
    /// </summary>
    public async Task<PerfilAprobacionResponseDto?> GetByGuidAsync(string guid)
    {
        var item = await _repository.GetByGuidAsync(guid);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene un perfil de aprobacion por su codigo.
    /// </summary>
    public async Task<PerfilAprobacionResponseDto?> GetByCodigoAsync(string codigo)
    {
        var item = await _repository.GetByCodigoAsync(codigo);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Crea un nuevo perfil de aprobacion.
    /// </summary>
    public async Task<PerfilAprobacionResponseDto> CreateAsync(CreatePerfilAprobacionDto dto, int idCreador)
    {
        var entity = new PerfilAprobacion
        {
            GrupoFlujoTrabajo = dto.GrupoFlujoTrabajo,
            Codigo = dto.Codigo,
            Descripcion = dto.Descripcion,
            Nivel = dto.Nivel,
            Orden = dto.Orden,
            IdUsuarioCreador = idCreador,
            Activo = 1
        };

        var id = await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(id);

        return MapToResponseDto(created!);
    }

    /// <summary>
    /// Actualiza un perfil de aprobacion existente.
    /// </summary>
    public async Task<PerfilAprobacionResponseDto?> UpdateAsync(UpdatePerfilAprobacionDto dto, int idModificador)
    {
        var existing = await _repository.GetByIdAsync(dto.IdPerfilAprobacion);
        if (existing == null)
            return null;

        existing.GrupoFlujoTrabajo = dto.GrupoFlujoTrabajo;
        existing.Codigo = dto.Codigo;
        existing.Descripcion = dto.Descripcion;
        existing.Nivel = dto.Nivel;
        existing.Orden = dto.Orden;

        var updated = await _repository.UpdateAsync(existing);
        if (!updated)
            return null;

        var result = await _repository.GetByIdAsync(dto.IdPerfilAprobacion);
        return MapToResponseDto(result!);
    }

    /// <summary>
    /// Elimina logicamente un perfil de aprobacion por su GUID.
    /// </summary>
    public async Task<bool> DeleteAsync(string guid, int idModificador)
    {
        var item = await _repository.GetByGuidAsync(guid);
        if (item == null)
            return false;

        return await _repository.DeleteAsync(item.IdPerfilAprobacion, idModificador);
    }

    private static PerfilAprobacionResponseDto MapToResponseDto(PerfilAprobacion entity)
    {
        return new PerfilAprobacionResponseDto
        {
            IdPerfilAprobacion = entity.IdPerfilAprobacion,
            GrupoFlujoTrabajo = entity.GrupoFlujoTrabajo,
            Codigo = entity.Codigo,
            Descripcion = entity.Descripcion,
            Nivel = entity.Nivel,
            Orden = entity.Orden,
            GuidRegistro = entity.GuidRegistro,
            Activo = entity.Activo,
            IdUsuarioCreador = entity.IdUsuarioCreador,
            FechaCreacion = entity.FechaCreacion
        };
    }
}
