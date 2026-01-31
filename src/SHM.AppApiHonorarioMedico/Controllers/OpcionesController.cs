using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class OpcionesController : ControllerBase
{
    private readonly IOpcionService _opcionService;
    private readonly ILogger<OpcionesController> _logger;

    public OpcionesController(IOpcionService opcionService, ILogger<OpcionesController> logger)
    {
        _opcionService = opcionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las opciones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OpcionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OpcionResponseDto>>> GetAll()
    {
        try
        {
            var opciones = await _opcionService.GetAllOpcionesAsync();
            return Ok(opciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una opcion por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OpcionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OpcionResponseDto>> GetById(int id)
    {
        try
        {
            var opcion = await _opcionService.GetOpcionByIdAsync(id);
            if (opcion == null)
                return NotFound(new { message = $"Opcion con ID {id} no encontrada" });

            return Ok(opcion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opcion {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene opciones por opcion padre (null para opciones raiz)
    /// </summary>
    [HttpGet("padre/{idOpcionPadre?}")]
    [ProducesResponseType(typeof(IEnumerable<OpcionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OpcionResponseDto>>> GetByPadre(int? idOpcionPadre = null)
    {
        try
        {
            var opciones = await _opcionService.GetOpcionesByPadreAsync(idOpcionPadre);
            return Ok(opciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opciones por padre {IdOpcionPadre}", idOpcionPadre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva opcion
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OpcionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OpcionResponseDto>> Create([FromBody] CreateOpcionDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idCreador = 1;
            var opcion = await _opcionService.CreateOpcionAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = opcion.IdOpcion }, opcion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear opcion");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una opcion existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOpcionDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _opcionService.UpdateOpcionAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Opcion con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar opcion {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) una opcion
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _opcionService.DeleteOpcionAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Opcion con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar opcion {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
