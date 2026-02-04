using SHM.AppDomain.DTOs.OrdenPagoAprobacion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de aprobaciones de ordenes de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoAprobacionService : IOrdenPagoAprobacionService
{
    private readonly IOrdenPagoAprobacionRepository _repository;

    public OrdenPagoAprobacionService(IOrdenPagoAprobacionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las aprobaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetAllActiveAsync()
    {
        var items = await _repository.GetAllActiveAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una aprobacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene una aprobacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto?> GetByGuidAsync(string guid)
    {
        var item = await _repository.GetByGuidAsync(guid);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        var items = await _repository.GetByOrdenPagoIdAsync(idOrdenPago);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de un rol.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetByRolIdAsync(int idRol)
    {
        var items = await _repository.GetByRolIdAsync(idRol);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva aprobacion de orden de pago.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto> CreateAsync(CreateOrdenPagoAprobacionDto dto, int idCreador)
    {
        var entity = new OrdenPagoAprobacion
        {
            IdOrdenPago = dto.IdOrdenPago,
            IdRol = dto.IdRol,
            IdCreador = idCreador,
            Activo = 1
        };

        var id = await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(id);

        return MapToResponseDto(created!);
    }

    /// <summary>
    /// Actualiza una aprobacion existente.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto?> UpdateAsync(UpdateOrdenPagoAprobacionDto dto, int idModificador)
    {
        var existing = await _repository.GetByIdAsync(dto.IdOrdenPagoAprobacion);
        if (existing == null)
            return null;

        existing.IdOrdenPago = dto.IdOrdenPago;
        existing.IdRol = dto.IdRol;
        existing.IdModificador = idModificador;

        var updated = await _repository.UpdateAsync(existing);
        if (!updated)
            return null;

        var result = await _repository.GetByIdAsync(dto.IdOrdenPagoAprobacion);
        return MapToResponseDto(result!);
    }

    /// <summary>
    /// Elimina logicamente una aprobacion por su GUID.
    /// </summary>
    public async Task<bool> DeleteAsync(string guid, int idModificador)
    {
        var item = await _repository.GetByGuidAsync(guid);
        if (item == null)
            return false;

        return await _repository.DeleteAsync(item.IdOrdenPagoAprobacion, idModificador);
    }

    /// <summary>
    /// Elimina logicamente todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        return await _repository.DeleteByOrdenPagoIdAsync(idOrdenPago, idModificador);
    }

    private static OrdenPagoAprobacionResponseDto MapToResponseDto(OrdenPagoAprobacion entity)
    {
        return new OrdenPagoAprobacionResponseDto
        {
            IdOrdenPagoAprobacion = entity.IdOrdenPagoAprobacion,
            IdOrdenPago = entity.IdOrdenPago,
            IdRol = entity.IdRol,
            NumeroOrdenPago = entity.NumeroOrdenPago,
            NombreRol = entity.NombreRol,
            NombreAprobador = entity.NombreAprobador,
            GuidRegistro = entity.GuidRegistro,
            Activo = entity.Activo,
            FechaCreacion = entity.FechaCreacion,
            FechaModificacion = entity.FechaModificacion
        };
    }
}
