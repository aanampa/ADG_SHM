using SHM.AppDomain.DTOs.Archivo;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio de negocio para la gestion de archivos.
/// Implementa la logica de negocio y el mapeo entre entidades y DTOs.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ArchivoService : IArchivoService
{
    private readonly IArchivoRepository _archivoRepository;

    /// <summary>
    /// Constructor del servicio.
    /// </summary>
    public ArchivoService(IArchivoRepository archivoRepository)
    {
        _archivoRepository = archivoRepository;
    }

    /// <summary>
    /// Obtiene todos los archivos.
    /// </summary>
    public async Task<IEnumerable<ArchivoResponseDto>> GetAllArchivosAsync()
    {
        var archivos = await _archivoRepository.GetAllAsync();
        return archivos.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un archivo por su ID.
    /// </summary>
    public async Task<ArchivoResponseDto?> GetArchivoByIdAsync(int id)
    {
        var archivo = await _archivoRepository.GetByIdAsync(id);
        return archivo != null ? MapToResponseDto(archivo) : null;
    }

    /// <summary>
    /// Obtiene un archivo por su GUID.
    /// </summary>
    public async Task<ArchivoResponseDto?> GetArchivoByGuidAsync(string guid)
    {
        var archivo = await _archivoRepository.GetByGuidAsync(guid);
        return archivo != null ? MapToResponseDto(archivo) : null;
    }

    /// <summary>
    /// Crea un nuevo archivo.
    /// </summary>
    public async Task<ArchivoResponseDto> CreateArchivoAsync(CreateArchivoDto createDto, int idCreador)
    {
        var archivo = new Archivo
        {
            TipoArchivo = createDto.TipoArchivo,
            NombreOriginal = createDto.NombreOriginal,
            NombreArchivo = createDto.NombreArchivo,
            Extension = createDto.Extension,
            Tamano = createDto.Tamano,
            Ruta = createDto.Ruta,
            IdCreador = idCreador,
            Activo = 1
        };

        var idArchivo = await _archivoRepository.CreateAsync(archivo);
        var createdArchivo = await _archivoRepository.GetByIdAsync(idArchivo);

        return MapToResponseDto(createdArchivo!);
    }

    /// <summary>
    /// Actualiza un archivo existente.
    /// </summary>
    public async Task<bool> UpdateArchivoAsync(int id, UpdateArchivoDto updateDto, int idModificador)
    {
        var archivoExistente = await _archivoRepository.GetByIdAsync(id);
        if (archivoExistente == null)
            return false;

        if (updateDto.TipoArchivo != null)
            archivoExistente.TipoArchivo = updateDto.TipoArchivo;

        if (updateDto.NombreOriginal != null)
            archivoExistente.NombreOriginal = updateDto.NombreOriginal;

        if (updateDto.NombreArchivo != null)
            archivoExistente.NombreArchivo = updateDto.NombreArchivo;

        if (updateDto.Extension != null)
            archivoExistente.Extension = updateDto.Extension;

        if (updateDto.Tamano.HasValue)
            archivoExistente.Tamano = updateDto.Tamano;

        if (updateDto.Ruta != null)
            archivoExistente.Ruta = updateDto.Ruta;

        if (updateDto.Activo.HasValue)
            archivoExistente.Activo = updateDto.Activo.Value;

        archivoExistente.IdModificador = idModificador;

        return await _archivoRepository.UpdateAsync(id, archivoExistente);
    }

    /// <summary>
    /// Elimina (soft delete) un archivo.
    /// </summary>
    public async Task<bool> DeleteArchivoAsync(int id, int idModificador)
    {
        var exists = await _archivoRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _archivoRepository.DeleteAsync(id, idModificador);
    }

    /// <summary>
    /// Mapea una entidad Archivo a ArchivoResponseDto.
    /// </summary>
    private static ArchivoResponseDto MapToResponseDto(Archivo archivo)
    {
        return new ArchivoResponseDto
        {
            IdArchivo = archivo.IdArchivo,
            TipoArchivo = archivo.TipoArchivo,
            NombreOriginal = archivo.NombreOriginal,
            NombreArchivo = archivo.NombreArchivo,
            Extension = archivo.Extension,
            Tamano = archivo.Tamano,
            Ruta = archivo.Ruta,
            GuidRegistro = archivo.GuidRegistro,
            Activo = archivo.Activo,
            FechaCreacion = archivo.FechaCreacion,
            FechaModificacion = archivo.FechaModificacion
        };
    }
}
