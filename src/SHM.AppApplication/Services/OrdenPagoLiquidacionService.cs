using SHM.AppDomain.DTOs.OrdenPagoLiquidacion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de relaciones orden de pago - liquidacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoLiquidacionService : IOrdenPagoLiquidacionService
{
    private readonly IOrdenPagoLiquidacionRepository _repository;

    public OrdenPagoLiquidacionService(IOrdenPagoLiquidacionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todas las relaciones orden de pago - liquidacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las relaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetAllActiveAsync()
    {
        var items = await _repository.GetAllActiveAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una relacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene una relacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto?> GetByGuidAsync(string guid)
    {
        var item = await _repository.GetByGuidAsync(guid);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene todas las liquidaciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        var items = await _repository.GetByOrdenPagoIdAsync(idOrdenPago);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago de una produccion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetByProduccionIdAsync(int idProduccion)
    {
        var items = await _repository.GetByProduccionIdAsync(idProduccion);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva relacion orden de pago - liquidacion.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto> CreateAsync(CreateOrdenPagoLiquidacionDto dto, int idCreador)
    {
        var entity = new OrdenPagoLiquidacion
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
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> CreateBulkAsync(
        IEnumerable<CreateOrdenPagoLiquidacionDto> dtos, int idCreador)
    {
        var entities = dtos.Select(dto => new OrdenPagoLiquidacion
        {
            IdOrdenPago = dto.IdOrdenPago,
            IdProduccion = dto.IdProduccion,
            IdCreador = idCreador,
            Activo = 1
        });

        await _repository.CreateBulkAsync(entities);

        // Obtener las liquidaciones creadas para la primera orden de pago
        var firstDto = dtos.FirstOrDefault();
        if (firstDto != null)
        {
            var items = await _repository.GetByOrdenPagoIdAsync(firstDto.IdOrdenPago);
            return items.Select(MapToResponseDto);
        }

        return Enumerable.Empty<OrdenPagoLiquidacionResponseDto>();
    }

    /// <summary>
    /// Actualiza una relacion existente.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto?> UpdateAsync(UpdateOrdenPagoLiquidacionDto dto, int idModificador)
    {
        var existing = await _repository.GetByIdAsync(dto.IdOrdenPagoLiquidacion);
        if (existing == null)
            return null;

        existing.IdOrdenPago = dto.IdOrdenPago;
        existing.IdProduccion = dto.IdProduccion;
        existing.IdModificador = idModificador;

        var updated = await _repository.UpdateAsync(existing);
        if (!updated)
            return null;

        var result = await _repository.GetByIdAsync(dto.IdOrdenPagoLiquidacion);
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

        return await _repository.DeleteAsync(item.IdOrdenPagoLiquidacion, idModificador);
    }

    /// <summary>
    /// Elimina logicamente todas las liquidaciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        return await _repository.DeleteByOrdenPagoIdAsync(idOrdenPago, idModificador);
    }

    private static OrdenPagoLiquidacionResponseDto MapToResponseDto(OrdenPagoLiquidacion entity)
    {
        return new OrdenPagoLiquidacionResponseDto
        {
            IdOrdenPagoLiquidacion = entity.IdOrdenPagoLiquidacion,
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
