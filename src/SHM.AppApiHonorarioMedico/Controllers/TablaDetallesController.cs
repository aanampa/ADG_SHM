using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.TablaDetalle;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablaDetallesController : ControllerBase
{
    private readonly ITablaDetalleService _tablaDetalleService;
    private readonly ILogger<TablaDetallesController> _logger;

    public TablaDetallesController(ITablaDetalleService tablaDetalleService, ILogger<TablaDetallesController> logger)
    {
        _tablaDetalleService = tablaDetalleService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los detalles de tablas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TablaDetalleResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TablaDetalleResponseDto>>> GetAll()
    {
        try
        {
            var tablaDetalles = await _tablaDetalleService.GetAllTablaDetallesAsync();
            return Ok(tablaDetalles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de tablas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene los detalles de una tabla especifica
    /// </summary>
    [HttpGet("tabla/{idTabla}")]
    [ProducesResponseType(typeof(IEnumerable<TablaDetalleResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TablaDetalleResponseDto>>> GetByTablaId(int idTabla)
    {
        try
        {
            var tablaDetalles = await _tablaDetalleService.GetTablaDetallesByTablaIdAsync(idTabla);
            return Ok(tablaDetalles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de tabla {IdTabla}", idTabla);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un detalle de tabla por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TablaDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TablaDetalleResponseDto>> GetById(int id)
    {
        try
        {
            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByIdAsync(id);
            if (tablaDetalle == null)
                return NotFound(new { message = $"Detalle de tabla con ID {id} no encontrado" });

            return Ok(tablaDetalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de tabla {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un detalle de tabla por codigo dentro de una tabla
    /// </summary>
    [HttpGet("tabla/{idTabla}/codigo/{codigo}")]
    [ProducesResponseType(typeof(TablaDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TablaDetalleResponseDto>> GetByCodigo(int idTabla, string codigo)
    {
        try
        {
            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByCodigoAsync(idTabla, codigo);
            if (tablaDetalle == null)
                return NotFound(new { message = $"Detalle con codigo '{codigo}' no encontrado en tabla {idTabla}" });

            return Ok(tablaDetalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle con codigo {Codigo} de tabla {IdTabla}", codigo, idTabla);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo detalle de tabla
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TablaDetalleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TablaDetalleResponseDto>> Create([FromBody] CreateTablaDetalleDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingDetalle = await _tablaDetalleService.GetTablaDetalleByCodigoAsync(createDto.IdTabla, createDto.Codigo);
            if (existingDetalle != null)
                return BadRequest(new { message = "El codigo ya existe en esta tabla" });

            const int idCreador = 1;
            var tablaDetalle = await _tablaDetalleService.CreateTablaDetalleAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = tablaDetalle.IdTablaDetalle }, tablaDetalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear detalle de tabla");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un detalle de tabla existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTablaDetalleDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _tablaDetalleService.UpdateTablaDetalleAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Detalle de tabla con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar detalle de tabla {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) un detalle de tabla
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _tablaDetalleService.DeleteTablaDetalleAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Detalle de tabla con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar detalle de tabla {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
