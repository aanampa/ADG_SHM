using SHM.AppDomain.DTOs.Produccion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la creacion masiva de producciones a traves de interface.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-01-31 - Agregado metodo UpdateLiquidacionesAsync</modified>
/// </summary>
public interface IProduccionInterfaceService
{
    /// <summary>
    /// Crea multiples producciones en el sistema a partir de una coleccion de DTOs.
    /// Resuelve los codigos de sede y entidad medica a sus respectivos IDs.
    /// Valida duplicados por llave compuesta (CodigoSede, CodigoEntidad, CodigoProduccion).
    /// Si hay error, aborta toda la operacion.
    /// </summary>
    Task<InterfaceProduccionResultDto> CreateProduccionesAsync(IEnumerable<CreateInterfaceProduccionDto> createDtos, int idCreador);

    /// <summary>
    /// Actualiza los datos de liquidacion de multiples producciones.
    /// Busca por llave compuesta (CodigoSede, CodigoEntidad, CodigoProduccion, NumeroProduccion, TipoEntidadMedica).
    /// </summary>
    Task<InterfaceProduccionResultDto> UpdateLiquidacionesAsync(IEnumerable<UpdateInterfaceLiquidacionDto> updateDtos, int idModificador);
}
