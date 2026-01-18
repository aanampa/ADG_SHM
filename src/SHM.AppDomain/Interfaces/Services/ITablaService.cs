using SHM.AppDomain.DTOs.Tabla;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de tablas maestras en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface ITablaService
{
    /// <summary>
    /// Obtiene todas las tablas maestras registradas en el sistema.
    /// </summary>
    Task<IEnumerable<TablaResponseDto>> GetAllTablasAsync();

    /// <summary>
    /// Obtiene una tabla por su identificador unico.
    /// </summary>
    Task<TablaResponseDto?> GetTablaByIdAsync(int id);

    /// <summary>
    /// Obtiene una tabla por su GUID de registro.
    /// </summary>
    Task<TablaResponseDto?> GetTablaByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene una tabla por su codigo.
    /// </summary>
    Task<TablaResponseDto?> GetTablaByCodigoAsync(string codigo);

    /// <summary>
    /// Crea una nueva tabla maestra en el sistema.
    /// </summary>
    Task<TablaResponseDto> CreateTablaAsync(CreateTablaDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una tabla existente.
    /// </summary>
    Task<bool> UpdateTablaAsync(int id, UpdateTablaDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una tabla por su identificador.
    /// </summary>
    Task<bool> DeleteTablaAsync(int id);
}
