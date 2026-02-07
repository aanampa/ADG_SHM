using SHM.AppDomain.DTOs.OrdenPagoProduccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de relaciones orden de pago - produccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// <modified>ADG Antonio - 2026-02-07 - Renombrado de OrdenPagoLiquidacion a OrdenPagoProduccion</modified>
/// </summary>
public class OrdenPagoProduccionService : IOrdenPagoProduccionService
{
    private readonly IOrdenPagoProduccionRepository _repository;

    public OrdenPagoProduccionService(IOrdenPagoProduccionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todas las relaciones orden de pago - produccion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las relaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetAllActiveAsync()
    {
        var items = await _repository.GetAllActiveAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una relacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoProduccionResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene una relacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoProduccionResponseDto?> GetByGuidAsync(string guid)
    {
        var item = await _repository.GetByGuidAsync(guid);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene todas las producciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        var items = await _repository.GetByOrdenPagoIdAsync(idOrdenPago);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago de una produccion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccionResponseDto>> GetByProduccionIdAsync(int idProduccion)
    {
        var items = await _repository.GetByProduccionIdAsync(idProduccion);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva relacion orden de pago - produccion.
    /// </summary>
    public async Task<OrdenPagoProduccionResponseDto> CreateAsync(CreateOrdenPagoProduccionDto dto, int idCreador)
    {
        var entity = new OrdenPagoProduccion
        {
            IdOrdenPago = dto.IdOrdenPago,
            IdProduccion = dto.IdProduccion,
            IdCreador = idCreador,
            Activo = 1
        };

        var id = await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(id);

        return MapToResponseDto(created!);
    }

    /// <summary>
    /// Crea multiples relaciones en una sola operacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccionResponseDto>> CreateBulkAsync(
        IEnumerable<CreateOrdenPagoProduccionDto> dtos, int idCreador)
    {
        var entities = dtos.Select(dto => new OrdenPagoProduccion
        {
            IdOrdenPago = dto.IdOrdenPago,
            IdProduccion = dto.IdProduccion,
            IdCreador = idCreador,
            Activo = 1
        });

        await _repository.CreateBulkAsync(entities);

        var firstDto = dtos.FirstOrDefault();
        if (firstDto != null)
        {
            var items = await _repository.GetByOrdenPagoIdAsync(firstDto.IdOrdenPago);
            return items.Select(MapToResponseDto);
        }

        return Enumerable.Empty<OrdenPagoProduccionResponseDto>();
    }

    /// <summary>
    /// Actualiza una relacion existente.
    /// </summary>
    public async Task<OrdenPagoProduccionResponseDto?> UpdateAsync(UpdateOrdenPagoProduccionDto dto, int idModificador)
    {
        var existing = await _repository.GetByIdAsync(dto.IdOrdenPagoProduccion);
        if (existing == null)
            return null;

        existing.IdOrdenPago = dto.IdOrdenPago;
        existing.IdProduccion = dto.IdProduccion;
        existing.IdModificador = idModificador;

        var updated = await _repository.UpdateAsync(existing);
        if (!updated)
            return null;

        var result = await _repository.GetByIdAsync(dto.IdOrdenPagoProduccion);
        return MapToResponseDto(result!);
    }

    /// <summary>
    /// Elimina logicamente una relacion por su GUID.
    /// </summary>
    public async Task<bool> DeleteAsync(string guid, int idModificador)
    {
        var item = await _repository.GetByGuidAsync(guid);
        if (item == null)
            return false;

        return await _repository.DeleteAsync(item.IdOrdenPagoProduccion, idModificador);
    }

    /// <summary>
    /// Elimina logicamente todas las producciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        return await _repository.DeleteByOrdenPagoIdAsync(idOrdenPago, idModificador);
    }

    private static OrdenPagoProduccionResponseDto MapToResponseDto(OrdenPagoProduccion entity)
    {
        return new OrdenPagoProduccionResponseDto
        {
            IdOrdenPagoProduccion = entity.IdOrdenPagoProduccion,
            IdOrdenPago = entity.IdOrdenPago,
            IdProduccion = entity.IdProduccion,
            NumeroOrdenPago = entity.NumeroOrdenPago,
            NumeroLiquidacion = entity.NumeroLiquidacion,
            GuidRegistro = entity.GuidRegistro,
            Activo = entity.Activo,
            FechaCreacion = entity.FechaCreacion,
            FechaModificacion = entity.FechaModificacion
        };
    }
}
