using SHM.AppDomain.DTOs.Produccion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la creacion masiva de producciones a traves de interface.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-01-31 - Agregado metodo UpdateLiquidacionesAsync</modified>
/// <modified>ADG Antonio - 2026-02-25 - Agregado metodo SyncSedesFromApiAsync</modified>
/// <modified>ADG Antonio - 2026-03-17 - Agregado metodo CheckExternalServicesAsync</modified>
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

    /// <summary>
    /// Sincroniza todas las sedes desde el API de San Pablo.
    /// Consulta con Codigo=X y registra las que no existan localmente.
    /// Retorna la cantidad de sedes nuevas registradas.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-25</created>
    /// </summary>
    Task<int> SyncSedesFromApiAsync(int idCreador);

    /// <summary>
    /// Verifica la disponibilidad de los servicios externos (SAP y San Pablo)
    /// intentando obtener sus tokens de acceso en paralelo.
    /// Retorna true si ambos servicios estan disponibles, false si alguno falla.
    /// </summary>
    Task<(bool IsAvailable, List<string> Errors)> CheckExternalServicesAsync();
}
