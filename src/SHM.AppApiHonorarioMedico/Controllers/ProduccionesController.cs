using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class ProduccionesController : ControllerBase
{
    private readonly IProduccionService _produccionService;
    private readonly ILogger<ProduccionesController> _logger;

    public ProduccionesController(IProduccionService produccionService, ILogger<ProduccionesController> logger)
    {
        _produccionService = produccionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las producciones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProduccionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduccionResponseDto>>> GetAll()
    {
        try
        {
            var producciones = await _produccionService.GetAllProduccionesAsync();
            return Ok(producciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una produccion por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProduccionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProduccionResponseDto>> GetById(int id)
    {
        try
        {
            var produccion = await _produccionService.GetProduccionByIdAsync(id);
            if (produccion == null)
                return NotFound(new { message = $"Produccion con ID {id} no encontrada" });

            return Ok(produccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener produccion {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una produccion por codigo
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(ProduccionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProduccionResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var produccion = await _produccionService.GetProduccionByCodigoAsync(codigo);
            if (produccion == null)
                return NotFound(new { message = $"Produccion con codigo '{codigo}' no encontrada" });

            return Ok(produccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener produccion con codigo {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene producciones por sede
    /// </summary>
    [HttpGet("sede/{idSede}")]
    [ProducesResponseType(typeof(IEnumerable<ProduccionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduccionResponseDto>>> GetBySede(int idSede)
    {
        try
        {
            var producciones = await _produccionService.GetProduccionesBySedeAsync(idSede);
            return Ok(producciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producciones de sede {IdSede}", idSede);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene producciones por entidad medica
    /// </summary>
    [HttpGet("entidad-medica/{idEntidadMedica}")]
    [ProducesResponseType(typeof(IEnumerable<ProduccionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduccionResponseDto>>> GetByEntidadMedica(int idEntidadMedica)
    {
        try
        {
            var producciones = await _produccionService.GetProduccionesByEntidadMedicaAsync(idEntidadMedica);
            return Ok(producciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producciones de entidad medica {IdEntidadMedica}", idEntidadMedica);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene producciones por periodo
    /// </summary>
    [HttpGet("periodo/{periodo}")]
    [ProducesResponseType(typeof(IEnumerable<ProduccionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduccionResponseDto>>> GetByPeriodo(string periodo)
    {
        try
        {
            var producciones = await _produccionService.GetProduccionesByPeriodoAsync(periodo);
            return Ok(producciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producciones del periodo {Periodo}", periodo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva produccion
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProduccionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProduccionResponseDto>> Create([FromBody] CreateProduccionDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idCreador = 1;
            var produccion = await _produccionService.CreateProduccionAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = produccion.IdProduccion }, produccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear produccion");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una produccion existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProduccionDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _produccionService.UpdateProduccionAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Produccion con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar produccion {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) una produccion
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _produccionService.DeleteProduccionAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Produccion con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar produccion {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
