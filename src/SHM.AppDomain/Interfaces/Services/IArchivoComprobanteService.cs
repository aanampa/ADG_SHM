using SHM.AppDomain.DTOs.ArchivoComprobante;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de archivos comprobantes en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IArchivoComprobanteService
{
    /// <summary>
    /// Obtiene todos los archivos comprobantes registrados en el sistema.
    /// </summary>
    Task<IEnumerable<ArchivoComprobanteResponseDto>> GetAllArchivoComprobantesAsync();

    /// <summary>
    /// Obtiene un archivo comprobante por su identificador unico.
    /// </summary>
    Task<ArchivoComprobanteResponseDto?> GetArchivoComprobanteByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los archivos comprobantes asociados a una produccion especifica.
    /// </summary>
    Task<IEnumerable<ArchivoComprobanteResponseDto>> GetArchivoComprobantesByProduccionAsync(int idProduccion);

    /// <summary>
    /// Obtiene todos los comprobantes asociados a un archivo especifico.
    /// </summary>
    Task<IEnumerable<ArchivoComprobanteResponseDto>> GetArchivoComprobantesByArchivoAsync(int idArchivo);

    /// <summary>
    /// Crea un nuevo archivo comprobante en el sistema.
    /// </summary>
    Task<ArchivoComprobanteResponseDto> CreateArchivoComprobanteAsync(CreateArchivoComprobanteDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un archivo comprobante existente.
    /// </summary>
    Task<bool> UpdateArchivoComprobanteAsync(int id, UpdateArchivoComprobanteDto updateDto, int idModificador);

    /// <summary>
    /// Elimina un archivo comprobante registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteArchivoComprobanteAsync(int id, int idModificador);
}
