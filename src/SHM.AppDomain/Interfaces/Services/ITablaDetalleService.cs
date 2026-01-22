using SHM.AppDomain.DTOs.TablaDetalle;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de detalles de tablas maestras en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-20 - Agregado metodo ListarPorCodigoTablaAsync</modified>
/// </summary>
public interface ITablaDetalleService
{
    /// <summary>
    /// Obtiene todos los detalles de tablas registrados en el sistema.
    /// </summary>
    Task<IEnumerable<TablaDetalleResponseDto>> GetAllTablaDetallesAsync();

    /// <summary>
    /// Obtiene todos los detalles asociados a una tabla especifica.
    /// </summary>
    Task<IEnumerable<TablaDetalleResponseDto>> GetTablaDetallesByTablaIdAsync(int idTabla);

    /// <summary>
    /// Obtiene solo los detalles activos de una tabla especifica.
    /// </summary>
    Task<IEnumerable<TablaDetalleResponseDto>> GetActivosByTablaIdAsync(int idTabla);

    /// <summary>
    /// Lista los tipos de entidad medica disponibles.
    /// </summary>
    Task<IEnumerable<TablaDetalleResponseDto>> ListarTipoEntidadMedicaAsync();

    /// <summary>
    /// Obtiene un detalle de tabla por su identificador unico.
    /// </summary>
    Task<TablaDetalleResponseDto?> GetTablaDetalleByIdAsync(int id);

    /// <summary>
    /// Obtiene un detalle de tabla por su GUID de registro.
    /// </summary>
    Task<TablaDetalleResponseDto?> GetTablaDetalleByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene un detalle de tabla por su codigo dentro de una tabla especifica.
    /// </summary>
    Task<TablaDetalleResponseDto?> GetTablaDetalleByCodigoAsync(int idTabla, string codigo);

    /// <summary>
    /// Crea un nuevo detalle de tabla en el sistema.
    /// </summary>
    Task<TablaDetalleResponseDto> CreateTablaDetalleAsync(CreateTablaDetalleDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un detalle de tabla existente.
    /// </summary>
    Task<bool> UpdateTablaDetalleAsync(int id, UpdateTablaDetalleDto updateDto, int idModificador);

    /// <summary>
    /// Elimina un detalle de tabla por su identificador.
    /// </summary>
    Task<bool> DeleteTablaDetalleAsync(int id);

    /// <summary>
    /// Obtiene los detalles activos de una tabla por su codigo.
    /// </summary>
    /// <param name="codigoTabla">Codigo de la tabla maestra</param>
    /// <returns>Lista de detalles de la tabla</returns>
    Task<IEnumerable<TablaDetalleResponseDto>> ListarPorCodigoTablaAsync(string codigoTabla);
}
