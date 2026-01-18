using SHM.AppDomain.DTOs.Archivo;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio de negocio para la gestion de archivos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IArchivoService
{
    /// <summary>
    /// Obtiene todos los archivos.
    /// </summary>
    Task<IEnumerable<ArchivoResponseDto>> GetAllArchivosAsync();

    /// <summary>
    /// Obtiene un archivo por su ID.
    /// </summary>
    Task<ArchivoResponseDto?> GetArchivoByIdAsync(int id);

    /// <summary>
    /// Obtiene un archivo por su GUID.
    /// </summary>
    Task<ArchivoResponseDto?> GetArchivoByGuidAsync(string guid);

    /// <summary>
    /// Crea un nuevo archivo.
    /// </summary>
    Task<ArchivoResponseDto> CreateArchivoAsync(CreateArchivoDto createDto, int idCreador);

    /// <summary>
    /// Actualiza un archivo existente.
    /// </summary>
    Task<bool> UpdateArchivoAsync(int id, UpdateArchivoDto updateDto, int idModificador);

    /// <summary>
    /// Elimina (soft delete) un archivo.
    /// </summary>
    Task<bool> DeleteArchivoAsync(int id, int idModificador);
}
