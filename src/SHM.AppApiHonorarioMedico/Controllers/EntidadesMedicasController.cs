using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.EntidadMedica;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class EntidadesMedicasController : ControllerBase
{
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly ILogger<EntidadesMedicasController> _logger;

    public EntidadesMedicasController(IEntidadMedicaService entidadMedicaService, ILogger<EntidadesMedicasController> logger)
    {
        _entidadMedicaService = entidadMedicaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las entidades médicas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EntidadMedicaResponseDto>>> GetAll()
    {
        try
        {
            var entidades = await _entidadMedicaService.GetAllEntidadesMedicasAsync();
            return Ok(entidades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las entidades médicas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una entidad médica por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EntidadMedicaResponseDto>> GetById(int id)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByIdAsync(id);

            if (entidad == null)
                return NotFound(new { message = $"Entidad médica con ID {id} no encontrada" });

            return Ok(entidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la entidad médica con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una entidad médica por su código
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    public async Task<ActionResult<EntidadMedicaResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByCodigoAsync(codigo);

            if (entidad == null)
                return NotFound(new { message = $"Entidad médica con código {codigo} no encontrada" });

            return Ok(entidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la entidad médica con código {Codigo}", codigo);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una entidad médica por su RUC
    /// </summary>
    [HttpGet("ruc/{ruc}")]
    public async Task<ActionResult<EntidadMedicaResponseDto>> GetByRuc(string ruc)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByRucAsync(ruc);

            if (entidad == null)
                return NotFound(new { message = $"Entidad médica con RUC {ruc} no encontrada" });

            return Ok(entidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la entidad médica con RUC {Ruc}", ruc);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea una nueva entidad médica
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EntidadMedicaResponseDto>> Create([FromBody] CreateEntidadMedicaDto createDto, [FromHeader(Name = "X-User-Id")] int idCreador = 1)
    {
        try
        {
            if (string.IsNullOrEmpty(createDto.CodigoEntidad))
                return BadRequest(new { message = "El código de la entidad es requerido" });

            var entidad = await _entidadMedicaService.CreateEntidadMedicaAsync(createDto, idCreador);
            return CreatedAtAction(nameof(GetById), new { id = entidad.IdEntidadMedica }, entidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la entidad médica");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza una entidad médica existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEntidadMedicaDto updateDto, [FromHeader(Name = "X-User-Id")] int idModificador = 1)
    {
        try
        {
            var updated = await _entidadMedicaService.UpdateEntidadMedicaAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Entidad médica con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la entidad médica con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina (desactiva) una entidad médica
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-User-Id")] int idModificador = 1)
    {
        try
        {
            var deleted = await _entidadMedicaService.DeleteEntidadMedicaAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Entidad médica con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la entidad médica con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
