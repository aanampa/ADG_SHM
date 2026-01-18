using SHM.AppDomain.DTOs.ArchivoComprobante;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de archivos de comprobantes asociados a producciones, incluyendo operaciones CRUD y consultas por produccion o archivo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ArchivoComprobanteService : IArchivoComprobanteService
{
    private readonly IArchivoComprobanteRepository _archivoComprobanteRepository;

    public ArchivoComprobanteService(IArchivoComprobanteRepository archivoComprobanteRepository)
    {
        _archivoComprobanteRepository = archivoComprobanteRepository;
    }

    /// <summary>
    /// Obtiene todos los archivos de comprobantes del sistema
    /// </summary>
    public async Task<IEnumerable<ArchivoComprobanteResponseDto>> GetAllArchivoComprobantesAsync()
    {
        var archivoComprobantes = await _archivoComprobanteRepository.GetAllAsync();
        return archivoComprobantes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un archivo de comprobante por su identificador unico
    /// </summary>
    public async Task<ArchivoComprobanteResponseDto?> GetArchivoComprobanteByIdAsync(int id)
    {
        var archivoComprobante = await _archivoComprobanteRepository.GetByIdAsync(id);
        return archivoComprobante != null ? MapToResponseDto(archivoComprobante) : null;
    }

    /// <summary>
    /// Obtiene los archivos de comprobante de una produccion especifica
    /// </summary>
    public async Task<IEnumerable<ArchivoComprobanteResponseDto>> GetArchivoComprobantesByProduccionAsync(int idProduccion)
    {
        var archivoComprobantes = await _archivoComprobanteRepository.GetByProduccionAsync(idProduccion);
        return archivoComprobantes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene los comprobantes asociados a un archivo especifico
    /// </summary>
    public async Task<IEnumerable<ArchivoComprobanteResponseDto>> GetArchivoComprobantesByArchivoAsync(int idArchivo)
    {
        var archivoComprobantes = await _archivoComprobanteRepository.GetByArchivoAsync(idArchivo);
        return archivoComprobantes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea un nuevo archivo de comprobante en el sistema
    /// </summary>
    public async Task<ArchivoComprobanteResponseDto> CreateArchivoComprobanteAsync(CreateArchivoComprobanteDto createDto, int idCreador)
    {
        var archivoComprobante = new ArchivoComprobante
        {
            IdProduccion = createDto.IdProduccion,
            IdArchivo = createDto.IdArchivo,
            TipoArchivo = createDto.TipoArchivo,
            Descripcion = createDto.Descripcion,
            IdCreador = idCreador,
            Activo = 1
        };

        var idArchivoComprobante = await _archivoComprobanteRepository.CreateAsync(archivoComprobante);
        var createdArchivoComprobante = await _archivoComprobanteRepository.GetByIdAsync(idArchivoComprobante);

        return MapToResponseDto(createdArchivoComprobante!);
    }

    /// <summary>
    /// Actualiza un archivo de comprobante existente
    /// </summary>
    public async Task<bool> UpdateArchivoComprobanteAsync(int id, UpdateArchivoComprobanteDto updateDto, int idModificador)
    {
        var archivoComprobanteExistente = await _archivoComprobanteRepository.GetByIdAsync(id);
        if (archivoComprobanteExistente == null)
            return false;

        if (updateDto.IdProduccion.HasValue)
            archivoComprobanteExistente.IdProduccion = updateDto.IdProduccion;

        if (updateDto.IdArchivo.HasValue)
            archivoComprobanteExistente.IdArchivo = updateDto.IdArchivo;

        if (updateDto.TipoArchivo != null)
            archivoComprobanteExistente.TipoArchivo = updateDto.TipoArchivo;

        if (updateDto.Descripcion != null)
            archivoComprobanteExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Activo.HasValue)
            archivoComprobanteExistente.Activo = updateDto.Activo.Value;

        archivoComprobanteExistente.IdModificador = idModificador;

        return await _archivoComprobanteRepository.UpdateAsync(id, archivoComprobanteExistente);
    }

    /// <summary>
    /// Elimina un archivo de comprobante por su identificador
    /// </summary>
    public async Task<bool> DeleteArchivoComprobanteAsync(int id, int idModificador)
    {
        var exists = await _archivoComprobanteRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _archivoComprobanteRepository.DeleteAsync(id, idModificador);
    }

    private static ArchivoComprobanteResponseDto MapToResponseDto(ArchivoComprobante archivoComprobante)
    {
        return new ArchivoComprobanteResponseDto
        {
            IdArchivoComprobante = archivoComprobante.IdArchivoComprobante,
            IdProduccion = archivoComprobante.IdProduccion,
            IdArchivo = archivoComprobante.IdArchivo,
            TipoArchivo = archivoComprobante.TipoArchivo,
            Descripcion = archivoComprobante.Descripcion,
            Activo = archivoComprobante.Activo,
            GuidRegistro = archivoComprobante.GuidRegistro,
            FechaCreacion = archivoComprobante.FechaCreacion,
            FechaModificacion = archivoComprobante.FechaModificacion
        };
    }
}
