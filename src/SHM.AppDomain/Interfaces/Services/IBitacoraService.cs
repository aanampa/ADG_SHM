using SHM.AppDomain.DTOs.Bitacora;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de bitacoras de auditoria en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IBitacoraService
{
    /// <summary>
    /// Obtiene todas las bitacoras registradas en el sistema.
    /// </summary>
    Task<IEnumerable<BitacoraResponseDto>> GetAllBitacorasAsync();

    /// <summary>
    /// Obtiene una bitacora por su identificador unico.
    /// </summary>
    Task<BitacoraResponseDto?> GetBitacoraByIdAsync(int id);

    /// <summary>
    /// Obtiene todas las bitacoras asociadas a una entidad especifica.
    /// </summary>
    Task<IEnumerable<BitacoraResponseDto>> GetBitacorasByEntidadAsync(string entidad);

    /// <summary>
    /// Crea un nuevo registro de bitacora en el sistema.
    /// </summary>
    Task<BitacoraResponseDto> CreateBitacoraAsync(CreateBitacoraDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una bitacora existente.
    /// </summary>
    Task<bool> UpdateBitacoraAsync(int id, UpdateBitacoraDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una bitacora registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteBitacoraAsync(int id, int idModificador);

    /// <summary>
    /// Obtiene las bitacoras de una entidad especifica por su ID, incluyendo datos del usuario que realizo la accion.
    /// </summary>
    /// <param name="entidad">Nombre de la entidad (ej: SHM_PRODUCCION)</param>
    /// <param name="idEntidad">ID de la entidad</param>
    /// <returns>Lista de bitacoras con datos del usuario</returns>
    Task<IEnumerable<BitacoraConUsuarioDto>> GetBitacorasByEntidadYIdAsync(string entidad, int idEntidad);
}
