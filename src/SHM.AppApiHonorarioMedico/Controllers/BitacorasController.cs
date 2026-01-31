using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class BitacorasController : ControllerBase
{
    private readonly IBitacoraService _bitacoraService;
    private readonly ILogger<BitacorasController> _logger;

    public BitacorasController(IBitacoraService bitacoraService, ILogger<BitacorasController> logger)
    {
        _bitacoraService = bitacoraService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las bitacoras
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BitacoraResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BitacoraResponseDto>>> GetAll()
    {
        try
        {
            var bitacoras = await _bitacoraService.GetAllBitacorasAsync();
            return Ok(bitacoras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bitacoras");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una bitacora por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BitacoraResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BitacoraResponseDto>> GetById(int id)
    {
        try
        {
            var bitacora = await _bitacoraService.GetBitacoraByIdAsync(id);
            if (bitacora == null)
                return NotFound(new { message = $"Bitacora con ID {id} no encontrada" });

            return Ok(bitacora);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bitacora {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene bitacoras por entidad
    /// </summary>
    [HttpGet("entidad/{entidad}")]
    [ProducesResponseType(typeof(IEnumerable<BitacoraResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BitacoraResponseDto>>> GetByEntidad(string entidad)
    {
        try
        {
            var bitacoras = await _bitacoraService.GetBitacorasByEntidadAsync(entidad);
            return Ok(bitacoras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bitacoras de entidad {Entidad}", entidad);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva bitacora
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BitacoraResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BitacoraResponseDto>> Create([FromBody] CreateBitacoraDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idCreador = 1;
            var bitacora = await _bitacoraService.CreateBitacoraAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = bitacora.IdBitacora }, bitacora);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear bitacora");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una bitacora existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBitacoraDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _bitacoraService.UpdateBitacoraAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Bitacora con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar bitacora {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) una bitacora
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _bitacoraService.DeleteBitacoraAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Bitacora con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar bitacora {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
