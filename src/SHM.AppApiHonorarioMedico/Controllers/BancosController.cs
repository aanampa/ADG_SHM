using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Banco;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BancosController : ControllerBase
{
    private readonly IBancoService _bancoService;
    private readonly ILogger<BancosController> _logger;

    public BancosController(IBancoService bancoService, ILogger<BancosController> logger)
    {
        _bancoService = bancoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los bancos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BancoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BancoResponseDto>>> GetAll()
    {
        try
        {
            var bancos = await _bancoService.GetAllBancosAsync();
            return Ok(bancos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bancos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un banco por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BancoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BancoResponseDto>> GetById(int id)
    {
        try
        {
            var banco = await _bancoService.GetBancoByIdAsync(id);
            if (banco == null)
                return NotFound(new { message = $"Banco con ID {id} no encontrado" });

            return Ok(banco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener banco {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un banco por codigo
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(BancoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BancoResponseDto>> GetByCodigo(string codigo)
    {
        try
        {
            var banco = await _bancoService.GetBancoByCodigoAsync(codigo);
            if (banco == null)
                return NotFound(new { message = $"Banco con codigo '{codigo}' no encontrado" });

            return Ok(banco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener banco con codigo {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo banco
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BancoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BancoResponseDto>> Create([FromBody] CreateBancoDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingBanco = await _bancoService.GetBancoByCodigoAsync(createDto.CodigoBanco);
            if (existingBanco != null)
                return BadRequest(new { message = "El codigo ya existe" });

            const int idCreador = 1;
            var banco = await _bancoService.CreateBancoAsync(createDto, idCreador);

            return CreatedAtAction(nameof(GetById), new { id = banco.IdBanco }, banco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear banco");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un banco existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBancoDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int idModificador = 1;
            var updated = await _bancoService.UpdateBancoAsync(id, updateDto, idModificador);

            if (!updated)
                return NotFound(new { message = $"Banco con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar banco {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) un banco
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            const int idModificador = 1;
            var deleted = await _bancoService.DeleteBancoAsync(id, idModificador);

            if (!deleted)
                return NotFound(new { message = $"Banco con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar banco {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
