using SHM.AppDomain.DTOs.Sede;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de sedes en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface ISedeService
{
    /// <summary>
    /// Obtiene todas las sedes registradas en el sistema.
    /// </summary>
    Task<IEnumerable<SedeResponseDto>> GetAllSedesAsync();

    /// <summary>
    /// Obtiene una sede por su identificador unico.
    /// </summary>
    Task<SedeResponseDto?> GetSedeByIdAsync(int id);

    /// <summary>
    /// Obtiene una sede por su codigo.
    /// </summary>
    Task<SedeResponseDto?> GetSedeByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una sede por su GUID de registro.
    /// </summary>
    Task<SedeResponseDto?> GetSedeByGuidAsync(string guidRegistro);

    /// <summary>
    /// Crea una nueva sede en el sistema.
    /// </summary>
    Task<SedeResponseDto> CreateSedeAsync(CreateSedeDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una sede existente.
    /// </summary>
    Task<bool> UpdateSedeAsync(int id, UpdateSedeDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una sede registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteSedeAsync(int id, int idModificador);
}
