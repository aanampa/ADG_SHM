using SHM.AppDomain.DTOs.Liquidacion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de liquidaciones en la capa de aplicacion.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
public interface ILiquidacionService
{
    /// <summary>
    /// Obtiene el listado paginado de liquidaciones con datos relacionados y filtro por banco.
    /// </summary>
    /// <param name="idBanco">Filtro por ID de Banco (opcional)</param>
    /// <param name="idSede">Filtro por ID de Sede del usuario logueado (opcional)</param>
    /// <param name="pageNumber">Numero de pagina</param>
    /// <param name="pageSize">Tamaño de pagina</param>
    /// <returns>Tupla con lista de liquidaciones y total de registros</returns>
    Task<(IEnumerable<LiquidacionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        int? idBanco, int? idSede, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene una liquidacion por su GUID con datos relacionados.
    /// </summary>
    /// <param name="guidRegistro">GUID del registro de produccion</param>
    /// <returns>Liquidacion con datos relacionados o null si no existe</returns>
    Task<LiquidacionListaResponseDto?> GetLiquidacionByGuidAsync(string guidRegistro);
}
