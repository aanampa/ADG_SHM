using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Tabla;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablasController : ControllerBase
{
    private readonly ITablaService _tablaService;
    private readonly ILogger<TablasController> _logger;

    public TablasController(ITablaService tablaService, ILogger<TablasController> logger)
    {
        _tablaService = tablaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tablas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TablaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TablaResponseDto>>> GetAll()
    {
        try
        {
            var tablas = await _tablaService.GetAllTablasAsync();
            return Ok(tablas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tablas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una tabla por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TablaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TablaResponseDto>> GetById(int id)
    {
        try
        {
            var tabla = await _tablaService.GetTablaByIdAsync(id);
            if (tabla == null)
                return NotFound(new { message = $"Tabla con ID {id} no encontrada" });

            return Ok(tabla);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una tabla por codigo
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(TablaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TablaResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var tabla = await _tablaService.GetTablaByCodigoAsync(codigo);
            if (tabla == null)
                return NotFound(new { message = $"Tabla con codigo '{codigo}' no encontrada" });

            return Ok(tabla);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla con codigo {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva tabla
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TablaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TablaResponseDto>> Create([FromBody] CreateTablaDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTabla = await _tablaService.GetTablaByCodigoAsync(createDto.Codigo);
            if (existingTabla != null)
                return BadRequest(new { message = "El codigo ya existe" });

            const int idCreador = 1;
            var tabla = await _tablaService.CreateTablaAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = tabla.IdTabla }, tabla);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tabla");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una tabla existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTablaDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _tablaService.UpdateTablaAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Tabla con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tabla {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) una tabla
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _tablaService.DeleteTablaAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Tabla con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tabla {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
