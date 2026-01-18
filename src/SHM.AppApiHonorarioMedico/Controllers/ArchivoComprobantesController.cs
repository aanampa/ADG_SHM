using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.ArchivoComprobante;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchivoComprobantesController : ControllerBase
{
    private readonly IArchivoComprobanteService _archivoComprobanteService;
    private readonly ILogger<ArchivoComprobantesController> _logger;

    public ArchivoComprobantesController(IArchivoComprobanteService archivoComprobanteService, ILogger<ArchivoComprobantesController> logger)
    {
        _archivoComprobanteService = archivoComprobanteService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los archivos comprobantes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ArchivoComprobanteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArchivoComprobanteResponseDto>>> GetAll()
    {
        try
        {
            var archivoComprobantes = await _archivoComprobanteService.GetAllArchivoComprobantesAsync();
            return Ok(archivoComprobantes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivos comprobantes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un archivo comprobante por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ArchivoComprobanteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArchivoComprobanteResponseDto>> GetById(int id)
    {
        try
        {
            var archivoComprobante = await _archivoComprobanteService.GetArchivoComprobanteByIdAsync(id);
            if (archivoComprobante == null)
                return NotFound(new { message = $"ArchivoComprobante con ID {id} no encontrado" });

            return Ok(archivoComprobante);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivo comprobante {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene archivos comprobantes por produccion
    /// </summary>
    [HttpGet("produccion/{idProduccion}")]
    [ProducesResponseType(typeof(IEnumerable<ArchivoComprobanteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArchivoComprobanteResponseDto>>> GetByProduccion(int idProduccion)
    {
        try
        {
            var archivoComprobantes = await _archivoComprobanteService.GetArchivoComprobantesByProduccionAsync(idProduccion);
            return Ok(archivoComprobantes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivos comprobantes de produccion {IdProduccion}", idProduccion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene archivos comprobantes por archivo
    /// </summary>
    [HttpGet("archivo/{idArchivo}")]
    [ProducesResponseType(typeof(IEnumerable<ArchivoComprobanteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArchivoComprobanteResponseDto>>> GetByArchivo(int idArchivo)
    {
        try
        {
            var archivoComprobantes = await _archivoComprobanteService.GetArchivoComprobantesByArchivoAsync(idArchivo);
            return Ok(archivoComprobantes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivos comprobantes de archivo {IdArchivo}", idArchivo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo archivo comprobante
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ArchivoComprobanteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArchivoComprobanteResponseDto>> Create([FromBody] CreateArchivoComprobanteDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idCreador = 1;
            var archivoComprobante = await _archivoComprobanteService.CreateArchivoComprobanteAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = archivoComprobante.IdArchivoComprobante }, archivoComprobante);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear archivo comprobante");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un archivo comprobante existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateArchivoComprobanteDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _archivoComprobanteService.UpdateArchivoComprobanteAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"ArchivoComprobante con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar archivo comprobante {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) un archivo comprobante
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _archivoComprobanteService.DeleteArchivoComprobanteAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"ArchivoComprobante con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo comprobante {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
