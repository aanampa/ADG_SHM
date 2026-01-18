using SHM.AppDomain.DTOs.Opcion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de opciones de menu en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IOpcionService
{
    /// <summary>
    /// Obtiene todas las opciones de menu registradas en el sistema.
    /// </summary>
    Task<IEnumerable<OpcionResponseDto>> GetAllOpcionesAsync();

    /// <summary>
    /// Obtiene una opcion de menu por su identificador unico.
    /// </summary>
    Task<OpcionResponseDto?> GetOpcionByIdAsync(int id);

    /// <summary>
    /// Obtiene una opcion de menu por su GUID de registro.
    /// </summary>
    Task<OpcionResponseDto?> GetOpcionByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene las opciones de menu hijas de una opcion padre especifica.
    /// </summary>
    Task<IEnumerable<OpcionResponseDto>> GetOpcionesByPadreAsync(int? idOpcionPadre);

    /// <summary>
    /// Crea una nueva opcion de menu en el sistema.
    /// </summary>
    Task<OpcionResponseDto> CreateOpcionAsync(CreateOpcionDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una opcion de menu existente.
    /// </summary>
    Task<bool> UpdateOpcionAsync(int id, UpdateOpcionDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una opcion de menu registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteOpcionAsync(int id, int idModificador);
}
