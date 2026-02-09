using SHM.AppDomain.DTOs.OrdenPago;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de ordenes de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoService : IOrdenPagoService
{
    private readonly IOrdenPagoRepository _ordenPagoRepository;

    public OrdenPagoService(IOrdenPagoRepository ordenPagoRepository)
    {
        _ordenPagoRepository = ordenPagoRepository;
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoResponseDto>> GetAllAsync()
    {
        var ordenes = await _ordenPagoRepository.GetAllAsync();
        return ordenes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoResponseDto>> GetAllActiveAsync()
    {
        var ordenes = await _ordenPagoRepository.GetAllActiveAsync();
        return ordenes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una orden de pago por su identificador.
    /// </summary>
    public async Task<OrdenPagoResponseDto?> GetByIdAsync(int id)
    {
        var orden = await _ordenPagoRepository.GetByIdAsync(id);
        return orden != null ? MapToResponseDto(orden) : null;
    }

    /// <summary>
    /// Obtiene una orden de pago por su GUID.
    /// </summary>
    public async Task<OrdenPagoResponseDto?> GetByGuidAsync(string guid)
    {
        var orden = await _ordenPagoRepository.GetByGuidAsync(guid);
        return orden != null ? MapToResponseDto(orden) : null;
    }

    /// <summary>
    /// Obtiene una orden de pago por su numero.
    /// </summary>
    public async Task<OrdenPagoResponseDto?> GetByNumeroOrdenPagoAsync(string numeroOrdenPago)
    {
        var orden = await _ordenPagoRepository.GetByNumeroOrdenPagoAsync(numeroOrdenPago);
        return orden != null ? MapToResponseDto(orden) : null;
    }

    /// <summary>
    /// Obtiene ordenes de pago por banco.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoResponseDto>> GetByBancoAsync(int idBanco)
    {
        var ordenes = await _ordenPagoRepository.GetByBancoAsync(idBanco);
        return ordenes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene ordenes de pago por estado.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoResponseDto>> GetByEstadoAsync(string estado)
    {
        var ordenes = await _ordenPagoRepository.GetByEstadoAsync(estado);
        return ordenes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene ordenes de pago por rango de fecha de generacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoResponseDto>> GetByFechaGeneracionAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var ordenes = await _ordenPagoRepository.GetByFechaGeneracionAsync(fechaInicio, fechaFin);
        return ordenes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene ordenes de pago pendientes de aprobacion para un usuario.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoResponseDto>> GetPendingForApprovalByUserAsync(int idUsuario)
    {
        var ordenes = await _ordenPagoRepository.GetPendingForApprovalByUserAsync(idUsuario);
        return ordenes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva orden de pago.
    /// </summary>
    public async Task<OrdenPagoResponseDto> CreateAsync(CreateOrdenPagoDto dto, int idCreador)
    {
        var ordenPago = new OrdenPago
        {
            IdBanco = dto.IdBanco,
            NumeroOrdenPago = dto.NumeroOrdenPago,
            FechaGeneracion = dto.FechaGeneracion,
            Estado = dto.Estado,
            MtoConsumoAcum = dto.MtoConsumoAcum,
            MtoDescuentoAcum = dto.MtoDescuentoAcum,
            MtoSubtotalAcum = dto.MtoSubtotalAcum,
            MtoRentaAcum = dto.MtoRentaAcum,
            MtoIgvAcum = dto.MtoIgvAcum,
            MtoTotalAcum = dto.MtoTotalAcum,
            CantComprobantes = dto.CantComprobantes,
            CantLiquidaciones = dto.CantLiquidaciones,
            Comentarios = dto.Comentarios,
            IdCreador = idCreador,
            Activo = 1
        };

        var idOrdenPago = await _ordenPagoRepository.CreateAsync(ordenPago);
        var createdOrden = await _ordenPagoRepository.GetByIdAsync(idOrdenPago);

        return MapToResponseDto(createdOrden!);
    }

    /// <summary>
    /// Actualiza una orden de pago existente.
    /// </summary>
    public async Task<OrdenPagoResponseDto?> UpdateAsync(UpdateOrdenPagoDto dto, int idModificador)
    {
        var ordenExistente = await _ordenPagoRepository.GetByIdAsync(dto.IdOrdenPago);
        if (ordenExistente == null)
            return null;

        ordenExistente.IdBanco = dto.IdBanco;
        ordenExistente.NumeroOrdenPago = dto.NumeroOrdenPago;
        ordenExistente.FechaGeneracion = dto.FechaGeneracion;
        ordenExistente.Estado = dto.Estado;
        ordenExistente.MtoConsumoAcum = dto.MtoConsumoAcum;
        ordenExistente.MtoDescuentoAcum = dto.MtoDescuentoAcum;
        ordenExistente.MtoSubtotalAcum = dto.MtoSubtotalAcum;
        ordenExistente.MtoRentaAcum = dto.MtoRentaAcum;
        ordenExistente.MtoIgvAcum = dto.MtoIgvAcum;
        ordenExistente.MtoTotalAcum = dto.MtoTotalAcum;
        ordenExistente.CantComprobantes = dto.CantComprobantes;
        ordenExistente.CantLiquidaciones = dto.CantLiquidaciones;
        ordenExistente.Comentarios = dto.Comentarios;
        ordenExistente.IdModificador = idModificador;

        var updated = await _ordenPagoRepository.UpdateAsync(ordenExistente);
        if (!updated)
            return null;

        var updatedOrden = await _ordenPagoRepository.GetByIdAsync(dto.IdOrdenPago);
        return MapToResponseDto(updatedOrden!);
    }

    /// <summary>
    /// Elimina logicamente una orden de pago por su GUID.
    /// </summary>
    public async Task<bool> DeleteAsync(string guid, int idModificador)
    {
        var orden = await _ordenPagoRepository.GetByGuidAsync(guid);
        if (orden == null)
            return false;

        return await _ordenPagoRepository.DeleteAsync(orden.IdOrdenPago, idModificador);
    }

    private static OrdenPagoResponseDto MapToResponseDto(OrdenPago orden)
    {
        return new OrdenPagoResponseDto
        {
            IdOrdenPago = orden.IdOrdenPago,
            IdBanco = orden.IdBanco,
            NombreBanco = orden.NombreBanco,
            NumeroOrdenPago = orden.NumeroOrdenPago,
            FechaGeneracion = orden.FechaGeneracion,
            Estado = orden.Estado,
            MtoConsumoAcum = orden.MtoConsumoAcum,
            MtoDescuentoAcum = orden.MtoDescuentoAcum,
            MtoSubtotalAcum = orden.MtoSubtotalAcum,
            MtoRentaAcum = orden.MtoRentaAcum,
            MtoIgvAcum = orden.MtoIgvAcum,
            MtoTotalAcum = orden.MtoTotalAcum,
            CantComprobantes = orden.CantComprobantes,
            CantLiquidaciones = orden.CantLiquidaciones,
            Comentarios = orden.Comentarios,
            GuidRegistro = orden.GuidRegistro,
            Activo = orden.Activo,
            FechaCreacion = orden.FechaCreacion,
            FechaModificacion = orden.FechaModificacion
        };
    }
}
