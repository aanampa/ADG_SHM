using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Archivo;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador REST para la gestion de archivos.
/// Expone endpoints CRUD para la entidad Archivo.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ArchivosController : ControllerBase
{
    private readonly IArchivoService _archivoService;
    private readonly ILogger<ArchivosController> _logger;

    /// <summary>
    /// Constructor del controlador.
    /// </summary>
    public ArchivosController(IArchivoService archivoService, ILogger<ArchivosController> logger)
    {
        _archivoService = archivoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los archivos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArchivoResponseDto>>> GetAll()
    {
        try
        {
            var archivos = await _archivoService.GetAllArchivosAsync();
            return Ok(archivos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los archivos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un archivo por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ArchivoResponseDto>> GetById(int id)
    {
        try
        {
            var archivo = await _archivoService.GetArchivoByIdAsync(id);

            if (archivo == null)
                return NotFound(new { message = $"Archivo con ID {id} no encontrado" });

            return Ok(archivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el archivo con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un archivo por su GUID
    /// </summary>
    [HttpGet("guid/{guid}")]
    public async Task<ActionResult<ArchivoResponseDto>> GetByGuid(string guid)
    {
        try
        {
            var archivo = await _archivoService.GetArchivoByGuidAsync(guid);

            if (archivo == null)
                return NotFound(new { message = $"Archivo con GUID {guid} no encontrado" });

            return Ok(archivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el archivo con GUID {Guid}", guid);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo archivo
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArchivoResponseDto>> Create([FromBody] CreateArchivoDto createDto, [FromHeader(Name = "X-User-Id")] int idCreador = 1)
    {
        try
        {
            var archivo = await _archivoService.CreateArchivoAsync(createDto, idCreador);
            return CreatedAtAction(nameof(GetById), new { id = archivo.IdArchivo }, archivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el archivo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un archivo existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateArchivoDto updateDto, [FromHeader(Name = "X-User-Id")] int idModificador = 1)
    {
        try
        {
            var updated = await _archivoService.UpdateArchivoAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Archivo con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el archivo con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina (desactiva) un archivo
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-User-Id")] int idModificador = 1)
    {
        try
        {
            var deleted = await _archivoService.DeleteArchivoAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Archivo con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el archivo con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
