using SHM.AppDomain.DTOs.OrdenPagoLiquidacion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de liquidaciones de ordenes de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public class OrdenPagoLiquidacionService : IOrdenPagoLiquidacionService
{
    private readonly IOrdenPagoLiquidacionRepository _repository;

    public OrdenPagoLiquidacionService(IOrdenPagoLiquidacionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todas las liquidaciones de ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las liquidaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacionResponseDto>> GetAllActiveAsync()
    {
        var items = await _repository.GetAllActiveAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una liquidacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene una liquidacion por su GUID.
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
    /// Crea una nueva liquidacion de orden de pago.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto> CreateAsync(CreateOrdenPagoLiquidacionDto dto, int idCreador)
    {
        var entity = new OrdenPagoLiquidacion
        {
            IdOrdenPago = dto.IdOrdenPago,
            NumeroLiquidacion = dto.NumeroLiquidacion,
            CodigoLiquidacion = dto.CodigoLiquidacion,
            MtoConsumoAcum = dto.MtoConsumoAcum,
            MtoDescuentoAcum = dto.MtoDescuentoAcum,
            MtoSubtotalAcum = dto.MtoSubtotalAcum,
            MtoRentaAcum = dto.MtoRentaAcum,
            MtoIgvAcum = dto.MtoIgvAcum,
            MtoTotalAcum = dto.MtoTotalAcum,
            CantComprobantes = dto.CantComprobantes,
            Comentarios = dto.Comentarios,
            IdCreador = idCreador,
            Activo = 1
        };

        var id = await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(id);

        return MapToResponseDto(created!);
    }

    /// <summary>
    /// Actualiza una liquidacion existente.
    /// </summary>
    public async Task<OrdenPagoLiquidacionResponseDto?> UpdateAsync(UpdateOrdenPagoLiquidacionDto dto, int idModificador)
    {
        var existing = await _repository.GetByIdAsync(dto.IdOrdenPagoLiquidacion);
        if (existing == null)
            return null;

        existing.IdOrdenPago = dto.IdOrdenPago;
        existing.NumeroLiquidacion = dto.NumeroLiquidacion;
        existing.CodigoLiquidacion = dto.CodigoLiquidacion;
        existing.MtoConsumoAcum = dto.MtoConsumoAcum;
        existing.MtoDescuentoAcum = dto.MtoDescuentoAcum;
        existing.MtoSubtotalAcum = dto.MtoSubtotalAcum;
        existing.MtoRentaAcum = dto.MtoRentaAcum;
        existing.MtoIgvAcum = dto.MtoIgvAcum;
        existing.MtoTotalAcum = dto.MtoTotalAcum;
        existing.CantComprobantes = dto.CantComprobantes;
        existing.Comentarios = dto.Comentarios;
        existing.IdModificador = idModificador;

        var updated = await _repository.UpdateAsync(existing);
        if (!updated)
            return null;

        var result = await _repository.GetByIdAsync(dto.IdOrdenPagoLiquidacion);
        return MapToResponseDto(result!);
    }

    /// <summary>
    /// Elimina logicamente una liquidacion por su GUID.
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
            NumeroOrdenPago = entity.NumeroOrdenPago,
            NumeroLiquidacion = entity.NumeroLiquidacion,
            CodigoLiquidacion = entity.CodigoLiquidacion,
            MtoConsumoAcum = entity.MtoConsumoAcum,
            MtoDescuentoAcum = entity.MtoDescuentoAcum,
            MtoSubtotalAcum = entity.MtoSubtotalAcum,
            MtoRentaAcum = entity.MtoRentaAcum,
            MtoIgvAcum = entity.MtoIgvAcum,
            MtoTotalAcum = entity.MtoTotalAcum,
            CantComprobantes = entity.CantComprobantes,
            Comentarios = entity.Comentarios,
            GuidRegistro = entity.GuidRegistro,
            Activo = entity.Activo,
            FechaCreacion = entity.FechaCreacion,
            FechaModificacion = entity.FechaModificacion
        };
    }
}
