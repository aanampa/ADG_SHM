using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.EntidadCuentaBancaria;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // ← Oculta todo el controlador
public class EntidadCuentasBancariasController : ControllerBase
{
    private readonly IEntidadCuentaBancariaService _entidadCuentaBancariaService;
    private readonly ILogger<EntidadCuentasBancariasController> _logger;

    public EntidadCuentasBancariasController(IEntidadCuentaBancariaService entidadCuentaBancariaService, ILogger<EntidadCuentasBancariasController> logger)
    {
        _entidadCuentaBancariaService = entidadCuentaBancariaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las cuentas bancarias de entidades
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EntidadCuentaBancariaResponseDto>>> GetAll()
    {
        try
        {
            var cuentas = await _entidadCuentaBancariaService.GetAllEntidadCuentasBancariasAsync();
            return Ok(cuentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las cuentas bancarias de entidades");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una cuenta bancaria por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EntidadCuentaBancariaResponseDto>> GetById(int id)
    {
        try
        {
            var cuenta = await _entidadCuentaBancariaService.GetEntidadCuentaBancariaByIdAsync(id);

            if (cuenta == null)
                return NotFound(new { message = $"Cuenta bancaria con ID {id} no encontrada" });

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la cuenta bancaria con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene las cuentas bancarias de una entidad médica
    /// </summary>
    [HttpGet("entidad/{idEntidad}")]
    public async Task<ActionResult<IEnumerable<EntidadCuentaBancariaResponseDto>>> GetByEntidadId(int idEntidad)
    {
        try
        {
            var cuentas = await _entidadCuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(idEntidad);
            return Ok(cuentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las cuentas bancarias de la entidad con ID {IdEntidad}", idEntidad);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea una nueva cuenta bancaria de entidad
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EntidadCuentaBancariaResponseDto>> Create([FromBody] CreateEntidadCuentaBancariaDto createDto, [FromHeader(Name = "X-User-Id")] int idCreador = 1)
    {
        try
        {
            var cuenta = await _entidadCuentaBancariaService.CreateEntidadCuentaBancariaAsync(createDto, idCreador);
            return CreatedAtAction(nameof(GetById), new { id = cuenta.IdCuentaBancaria }, cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la cuenta bancaria de entidad");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza una cuenta bancaria existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEntidadCuentaBancariaDto updateDto, [FromHeader(Name = "X-User-Id")] int idModificador = 1)
    {
        try
        {
            var updated = await _entidadCuentaBancariaService.UpdateEntidadCuentaBancariaAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Cuenta bancaria con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la cuenta bancaria con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina (desactiva) una cuenta bancaria
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-User-Id")] int idModificador = 1)
    {
        try
        {
            var deleted = await _entidadCuentaBancariaService.DeleteEntidadCuentaBancariaAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Cuenta bancaria con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la cuenta bancaria con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
