using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Sede;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class SedesController : ControllerBase
{
    private readonly ISedeService _sedeService;
    private readonly ILogger<SedesController> _logger;

    public SedesController(ISedeService sedeService, ILogger<SedesController> logger)
    {
        _sedeService = sedeService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las sedes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SedeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SedeResponseDto>>> GetAll()
    {
        try
        {
            var sedes = await _sedeService.GetAllSedesAsync();
            return Ok(sedes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sedes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una sede por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SedeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SedeResponseDto>> GetById(int id)
    {
        try
        {
            var sede = await _sedeService.GetSedeByIdAsync(id);
            if (sede == null)
                return NotFound(new { message = $"Sede con ID {id} no encontrada" });

            return Ok(sede);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sede {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una sede por codigo
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(SedeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SedeResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var sede = await _sedeService.GetSedeByCodigoAsync(codigo);
            if (sede == null)
                return NotFound(new { message = $"Sede con codigo '{codigo}' no encontrada" });

            return Ok(sede);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sede con codigo {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva sede
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SedeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SedeResponseDto>> Create([FromBody] CreateSedeDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingSede = await _sedeService.GetSedeByCodigoAsync(createDto.Codigo);
            if (existingSede != null)
                return BadRequest(new { message = "El codigo ya existe" });

            const int idCreador = 1;
            var sede = await _sedeService.CreateSedeAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = sede.IdSede }, sede);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sede");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una sede existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSedeDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _sedeService.UpdateSedeAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Sede con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sede {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) una sede
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _sedeService.DeleteSedeAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Sede con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar sede {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
