using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Parametro;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class ParametrosController : ControllerBase
{
    private readonly IParametroService _parametroService;
    private readonly ILogger<ParametrosController> _logger;

    public ParametrosController(IParametroService parametroService, ILogger<ParametrosController> logger)
    {
        _parametroService = parametroService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los parametros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParametroResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ParametroResponseDto>>> GetAll()
    {
        try
        {
            var parametros = await _parametroService.GetAllParametrosAsync();
            return Ok(parametros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener parametros");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un parametro por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ParametroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParametroResponseDto>> GetById(int id)
    {
        try
        {
            var parametro = await _parametroService.GetParametroByIdAsync(id);
            if (parametro == null)
                return NotFound(new { message = $"Parametro con ID {id} no encontrado" });

            return Ok(parametro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener parametro {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un parametro por codigo
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(ParametroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParametroResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var parametro = await _parametroService.GetParametroByCodigoAsync(codigo);
            if (parametro == null)
                return NotFound(new { message = $"Parametro con codigo '{codigo}' no encontrado" });

            return Ok(parametro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener parametro con codigo {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo parametro
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ParametroResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParametroResponseDto>> Create([FromBody] CreateParametroDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingParametro = await _parametroService.GetParametroByCodigoAsync(createDto.Codigo);
            if (existingParametro != null)
                return BadRequest(new { message = "El codigo ya existe" });

            const int idCreador = 1;
            var parametro = await _parametroService.CreateParametroAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = parametro.IdParametro }, parametro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear parametro");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un parametro existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateParametroDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _parametroService.UpdateParametroAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Parametro con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar parametro {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) un parametro
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _parametroService.DeleteParametroAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Parametro con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar parametro {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
