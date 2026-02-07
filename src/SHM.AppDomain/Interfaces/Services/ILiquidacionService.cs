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

    /// <summary>
    /// Obtiene el listado agrupado de liquidaciones por CODIGO_LIQUIDACION e ID_BANCO.
    /// Para la generacion de ordenes de pago.
    /// </summary>
    /// <param name="idBanco">Filtro por ID de Banco (opcional)</param>
    /// <param name="idSede">Filtro por ID de Sede del usuario logueado</param>
    /// <returns>Lista de liquidaciones agrupadas</returns>
    Task<IEnumerable<LiquidacionGrupoResponseDto>> GetGruposAsync(int? idBanco, int idSede);

    /// <summary>
    /// Obtiene las producciones de un codigo de liquidacion especifico.
    /// </summary>
    /// <param name="codigoLiquidacion">Codigo de liquidacion</param>
    /// <param name="idSede">ID de Sede</param>
    /// <param name="idBanco">ID de Banco (opcional para filtrar por banco)</param>
    /// <returns>Lista de producciones asociadas</returns>
    Task<IEnumerable<LiquidacionListaResponseDto>> GetProduccionesByCodigoLiquidacionAsync(
        string codigoLiquidacion, int idSede, int? idBanco = null);

    /// <summary>
    /// Actualiza el estado de las producciones por lista de IDs.
    /// </summary>
    /// <param name="idsProduccion">Lista de IDs de produccion</param>
    /// <param name="nuevoEstado">Nuevo estado a asignar</param>
    /// <param name="idModificador">ID del usuario que modifica</param>
    /// <returns>Cantidad de registros actualizados</returns>
    Task<int> UpdateEstadoProduccionesAsync(IEnumerable<int> idsProduccion, string nuevoEstado, int idModificador);
}
