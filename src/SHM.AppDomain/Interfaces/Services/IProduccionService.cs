using SHM.AppDomain.DTOs.Produccion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de producciones en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IProduccionService
{
    /// <summary>
    /// Obtiene todas las producciones registradas en el sistema.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetAllProduccionesAsync();

    /// <summary>
    /// Obtiene una produccion por su identificador unico.
    /// </summary>
    Task<ProduccionResponseDto?> GetProduccionByIdAsync(int id);

    /// <summary>
    /// Obtiene una produccion por su codigo.
    /// </summary>
    Task<ProduccionResponseDto?> GetProduccionByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una produccion por su GUID de registro.
    /// </summary>
    Task<ProduccionResponseDto?> GetProduccionByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una sede especifica.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesBySedeAsync(int idSede);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una entidad medica especifica.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByEntidadMedicaAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene todas las producciones de un periodo especifico.
    /// </summary>
    Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByPeriodoAsync(string periodo);

    /// <summary>
    /// Crea una nueva produccion en el sistema.
    /// </summary>
    Task<ProduccionResponseDto> CreateProduccionAsync(CreateProduccionDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una produccion existente.
    /// </summary>
    Task<bool> UpdateProduccionAsync(int id, UpdateProduccionDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una produccion registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteProduccionAsync(int id, int idModificador);
}
