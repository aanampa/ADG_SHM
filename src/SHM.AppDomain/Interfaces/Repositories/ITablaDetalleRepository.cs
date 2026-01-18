using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de detalles de tablas maestras en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface ITablaDetalleRepository
{
    /// <summary>
    /// Obtiene todos los detalles de tablas registrados en el sistema.
    /// </summary>
    Task<IEnumerable<TablaDetalle>> GetAllAsync();

    /// <summary>
    /// Obtiene todos los detalles asociados a una tabla especifica.
    /// </summary>
    Task<IEnumerable<TablaDetalle>> GetByTablaIdAsync(int idTabla);

    /// <summary>
    /// Obtiene solo los detalles activos de una tabla especifica.
    /// </summary>
    Task<IEnumerable<TablaDetalle>> GetActivosByTablaIdAsync(int idTabla);

    /// <summary>
    /// Obtiene un detalle de tabla por su identificador unico.
    /// </summary>
    Task<TablaDetalle?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un detalle de tabla por su GUID de registro.
    /// </summary>
    Task<TablaDetalle?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene un detalle de tabla por su codigo dentro de una tabla especifica.
    /// </summary>
    Task<TablaDetalle?> GetByCodigoAsync(int idTabla, string codigo);

    /// <summary>
    /// Crea un nuevo detalle de tabla en la base de datos.
    /// </summary>
    Task<int> CreateAsync(TablaDetalle tablaDetalle);

    /// <summary>
    /// Actualiza los datos de un detalle de tabla existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, TablaDetalle tablaDetalle);

    /// <summary>
    /// Elimina un detalle de tabla por su identificador.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Verifica si existe un detalle de tabla con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
