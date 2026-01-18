using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.RolOpcion;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolOpcionesController : ControllerBase
{
    private readonly IRolOpcionService _rolOpcionService;
    private readonly ILogger<RolOpcionesController> _logger;

    public RolOpcionesController(IRolOpcionService rolOpcionService, ILogger<RolOpcionesController> logger)
    {
        _rolOpcionService = rolOpcionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las relaciones rol-opcion
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RolOpcionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RolOpcionResponseDto>>> GetAll()
    {
        try
        {
            var rolOpciones = await _rolOpcionService.GetAllRolOpcionesAsync();
            return Ok(rolOpciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol-opciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una relacion rol-opcion por ID compuesto
    /// </summary>
    [HttpGet("{idRol}/{idOpcion}")]
    [ProducesResponseType(typeof(RolOpcionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolOpcionResponseDto>> GetById(int idRol, int idOpcion)
    {
        try
        {
            var rolOpcion = await _rolOpcionService.GetRolOpcionByIdAsync(idRol, idOpcion);
            if (rolOpcion == null)
                return NotFound(new { message = $"RolOpcion con IdRol {idRol} e IdOpcion {idOpcion} no encontrada" });

            return Ok(rolOpcion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol-opcion {IdRol}/{IdOpcion}", idRol, idOpcion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las opciones de un rol
    /// </summary>
    [HttpGet("rol/{idRol}")]
    [ProducesResponseType(typeof(IEnumerable<RolOpcionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RolOpcionResponseDto>>> GetByRol(int idRol)
    {
        try
        {
            var rolOpciones = await _rolOpcionService.GetOpcionesByRolAsync(idRol);
            return Ok(rolOpciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opciones del rol {IdRol}", idRol);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todos los roles de una opcion
    /// </summary>
    [HttpGet("opcion/{idOpcion}")]
    [ProducesResponseType(typeof(IEnumerable<RolOpcionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RolOpcionResponseDto>>> GetByOpcion(int idOpcion)
    {
        try
        {
            var rolOpciones = await _rolOpcionService.GetRolesByOpcionAsync(idOpcion);
            return Ok(rolOpciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles de la opcion {IdOpcion}", idOpcion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva relacion rol-opcion
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RolOpcionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RolOpcionResponseDto>> Create([FromBody] CreateRolOpcionDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _rolOpcionService.GetRolOpcionByIdAsync(createDto.IdRol, createDto.IdOpcion);
            if (existing != null)
                return BadRequest(new { message = "La relacion rol-opcion ya existe" });

            const int idCreador = 1;
            var rolOpcion = await _rolOpcionService.CreateRolOpcionAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { idRol = rolOpcion.IdRol, idOpcion = rolOpcion.IdOpcion }, rolOpcion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear rol-opcion");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una relacion rol-opcion existente
    /// </summary>
    [HttpPut("{idRol}/{idOpcion}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int idRol, int idOpcion, [FromBody] UpdateRolOpcionDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _rolOpcionService.UpdateRolOpcionAsync(idRol, idOpcion, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"RolOpcion con IdRol {idRol} e IdOpcion {idOpcion} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol-opcion {IdRol}/{IdOpcion}", idRol, idOpcion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) una relacion rol-opcion
    /// </summary>
    [HttpDelete("{idRol}/{idOpcion}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idRol, int idOpcion)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _rolOpcionService.DeleteRolOpcionAsync(idRol, idOpcion, idModificador);

            if (!deleted)
                return NotFound(new { message = $"RolOpcion con IdRol {idRol} e IdOpcion {idOpcion} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol-opcion {IdRol}/{IdOpcion}", idRol, idOpcion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
