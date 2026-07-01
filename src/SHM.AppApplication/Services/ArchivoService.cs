using Microsoft.Extensions.Configuration;
using SHM.AppDomain.DTOs.Archivo;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio de negocio para la gestion de archivos.
/// Implementa la logica de negocio y el mapeo entre entidades y DTOs.
/// Soporta almacenamiento dual: FILE (sistema de archivos) y BLOB (base de datos).
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Vladimir - 2026-01-29 - Agregado soporte para almacenamiento BLOB</modified>
/// </summary>
public class ArchivoService : IArchivoService
{
    private readonly IArchivoRepository _archivoRepository;
    private readonly IParametroService _parametroService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor del servicio.
    /// </summary>
    public ArchivoService(
        IArchivoRepository archivoRepository,
        IParametroService parametroService,
        IConfiguration configuration)
    {
        _archivoRepository = archivoRepository;
        _parametroService = parametroService;
        _configuration = configuration;
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
    /// Determina automaticamente el tipo de almacenamiento segun el parametro SHM_TIPO_ALMACENAMIENTO_ARCHIVO.
    /// </summary>
    public async Task<ArchivoResponseDto> CreateArchivoAsync(CreateArchivoDto createDto, int idCreador)
    {
        // Determinar tipo de almacenamiento segun parametro del sistema
        var tipoAlmacenamientoParam = await _parametroService.GetValorByCodigoAsync("SHM_TIPO_ALMACENAMIENTO_ARCHIVO");
        var usarBlob = tipoAlmacenamientoParam?.ToUpper() == "BLOB";

        int idArchivo;

        if (usarBlob && createDto.ContenidoArchivo != null && createDto.ContenidoArchivo.Length > 0)
        {
            // Almacenar en BLOB (base de datos)
            var archivo = new Archivo
            {
                TipoArchivo = createDto.TipoArchivo,
                NombreOriginal = createDto.NombreOriginal,
                NombreArchivo = createDto.NombreArchivo,
                Extension = createDto.Extension,
                Tamano = createDto.Tamano,
                Ruta = null, // No se usa ruta para BLOB
                TipoAlmacenamiento = "BLOB",
                IdCreador = idCreador,
                Activo = 1
            };

            idArchivo = await _archivoRepository.CreateWithBlobAsync(archivo, createDto.ContenidoArchivo);
        }
        else
        {
            // Almacenar en sistema de archivos (comportamiento por defecto)
            var archivo = new Archivo
            {
                TipoArchivo = createDto.TipoArchivo,
                NombreOriginal = createDto.NombreOriginal,
                NombreArchivo = createDto.NombreArchivo,
                Extension = createDto.Extension,
                Tamano = createDto.Tamano,
                Ruta = createDto.Ruta,
                TipoAlmacenamiento = "FILE",
                IdCreador = idCreador,
                Activo = 1
            };

            idArchivo = await _archivoRepository.CreateAsync(archivo);
        }

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
    /// Obtiene el contenido de un archivo (desde BLOB o sistema de archivos).
    /// </summary>
    public async Task<ArchivoContenidoDto?> GetArchivoContenidoAsync(int idArchivo)
    {
        var archivo = await _archivoRepository.GetByIdAsync(idArchivo);
        if (archivo == null || archivo.Activo != 1)
            return null;

        return await ObtenerContenidoArchivo(archivo);
    }

    /// <summary>
    /// Obtiene el contenido de un archivo por su GUID.
    /// </summary>
    public async Task<ArchivoContenidoDto?> GetArchivoContenidoByGuidAsync(string guid)
    {
        var archivo = await _archivoRepository.GetByGuidAsync(guid);
        if (archivo == null || archivo.Activo != 1)
            return null;

        return await ObtenerContenidoArchivo(archivo);
    }

    /// <summary>
    /// Obtiene el contenido binario de un archivo segun su tipo de almacenamiento.
    /// </summary>
    private async Task<ArchivoContenidoDto?> ObtenerContenidoArchivo(Archivo archivo)
    {
        byte[]? contenido;

        if (archivo.TipoAlmacenamiento == "BLOB")
        {
            // Obtener contenido desde la base de datos
            contenido = await _archivoRepository.GetBlobContentAsync(archivo.IdArchivo);
        }
        else
        {
            // Obtener contenido desde el sistema de archivos
            var uploadBasePath = _configuration["FileStorage:UploadPath"]
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadBasePath, archivo.Ruta ?? "");

            if (!File.Exists(filePath))
                return null;

            contenido = await File.ReadAllBytesAsync(filePath);
        }

        if (contenido == null || contenido.Length == 0)
            return null;

        return new ArchivoContenidoDto
        {
            Contenido = contenido,
            NombreArchivo = archivo.NombreArchivo ?? "archivo",
            Extension = archivo.Extension,
            ContentType = GetContentType(archivo.Extension)
        };
    }

    /// <summary>
    /// Obtiene el tipo MIME segun la extension del archivo.
    /// </summary>
    private static string GetContentType(string? extension)
    {
        return extension?.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
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
            TipoAlmacenamiento = archivo.TipoAlmacenamiento,
            GuidRegistro = archivo.GuidRegistro,
            Activo = archivo.Activo,
            FechaCreacion = archivo.FechaCreacion,
            FechaModificacion = archivo.FechaModificacion
        };
    }
}
