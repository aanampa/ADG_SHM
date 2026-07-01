using Microsoft.Extensions.Logging;
using SHM.AppDomain.DTOs.Liquidacion;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de liquidaciones.
/// Proporciona acceso a registros de produccion con estado FACTURA_LIQUIDADA.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
public class LiquidacionService : ILiquidacionService
{
    private readonly ILiquidacionRepository _liquidacionRepository;
    private readonly ILogger<LiquidacionService> _logger;

    public LiquidacionService(
        ILiquidacionRepository liquidacionRepository,
        ILogger<LiquidacionService> logger)
    {
        _liquidacionRepository = liquidacionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el listado paginado de liquidaciones con datos relacionados y filtro por banco.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    public async Task<(IEnumerable<LiquidacionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        int? idBanco, int? idSede, int pageNumber, int pageSize)
    {
        return await _liquidacionRepository.GetPaginatedListAsync(idBanco, idSede, pageNumber, pageSize);
    }

    /// <summary>
    /// Obtiene una liquidacion por su GUID con datos relacionados.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    public async Task<LiquidacionListaResponseDto?> GetLiquidacionByGuidAsync(string guidRegistro)
    {
        return await _liquidacionRepository.GetByGuidWithDetailsAsync(guidRegistro);
    }

    /// <summary>
    /// Obtiene el listado agrupado de liquidaciones por CODIGO_LIQUIDACION e ID_BANCO.
    /// Para la generacion de ordenes de pago.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// </summary>
    public async Task<IEnumerable<LiquidacionGrupoResponseDto>> GetGruposAsync(int? idBanco, int idSede)
    {
        return await _liquidacionRepository.GetGruposAsync(idBanco, idSede);
    }

    /// <summary>
    /// Obtiene las producciones de un codigo de liquidacion especifico.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// <modified>ADG Vladimir D - 2026-02-06 - Agregado filtro por ID_BANCO</modified>
    /// </summary>
    public async Task<IEnumerable<LiquidacionListaResponseDto>> GetProduccionesByCodigoLiquidacionAsync(
        string codigoLiquidacion, int idSede, int? idBanco = null)
    {
        return await _liquidacionRepository.GetProduccionesByCodigoLiquidacionAsync(codigoLiquidacion, idSede, idBanco);
    }

    /// <summary>
    /// Actualiza el estado de las producciones por lista de IDs.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// </summary>
    public async Task<int> UpdateEstadoProduccionesAsync(IEnumerable<int> idsProduccion, string nuevoEstado, int idModificador)
    {
        return await _liquidacionRepository.UpdateEstadoProduccionesAsync(idsProduccion, nuevoEstado, idModificador);
    }
}
