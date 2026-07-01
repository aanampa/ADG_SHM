using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Rol;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRolService rolService, ILogger<RolesController> logger)
    {
        _rolService = rolService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los roles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RolResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RolResponseDto>>> GetAll()
    {
        try
        {
            var roles = await _rolService.GetAllRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un rol por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RolResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolResponseDto>> GetById(int id)
    {
        try
        {
            var rol = await _rolService.GetRolByIdAsync(id);
            if (rol == null)
                return NotFound(new { message = $"Rol con ID {id} no encontrado" });

            return Ok(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un rol por codigo
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(RolResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var rol = await _rolService.GetRolByCodigoAsync(codigo);
            if (rol == null)
                return NotFound(new { message = $"Rol con codigo '{codigo}' no encontrado" });

            return Ok(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol con codigo {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RolResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RolResponseDto>> Create([FromBody] CreateRolDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingRol = await _rolService.GetRolByCodigoAsync(createDto.Codigo);
            if (existingRol != null)
                return BadRequest(new { message = "El codigo ya existe" });

            const int idCreador = 1;
            var rol = await _rolService.CreateRolAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = rol.IdRol }, rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear rol");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRolDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _rolService.UpdateRolAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Rol con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) un rol
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _rolService.DeleteRolAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Rol con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
