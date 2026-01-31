using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la creacion masiva de producciones a traves de interface.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProduccionInterfaceController : ControllerBase
{
    private readonly IProduccionInterfaceService _produccionInterfaceService;
    private readonly ILogger<ProduccionInterfaceController> _logger;

    public ProduccionInterfaceController(IProduccionInterfaceService produccionInterfaceService, ILogger<ProduccionInterfaceController> logger)
    {
        _produccionInterfaceService = produccionInterfaceService;
        _logger = logger;
    }

    /// <summary>
    /// Metodo de prueba para validar si el API esta activo.
    /// Devuelve la fecha y hora actual del servidor.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-30</created>
    [HttpGet("test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Test()
    {
        return Ok(new
        {
            status = "OK",
            message = "API activa",
            serverDateTime = DateTime.Now,
            serverDateTimeUtc = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Crea multiples producciones a partir de una coleccion de datos.
    /// Resuelve automaticamente los codigos de sede y entidad medica a sus respectivos IDs.
    /// Valida duplicados por llave compuesta (CodigoSede, CodigoEntidad, CodigoProduccion).
    /// Si ya existe, lo omite. Si hay error, aborta toda la operacion.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InterfaceProduccionResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InterfaceProduccionResultDto>> Create([FromBody] IEnumerable<CreateInterfaceProduccionDto> createDtos)
    {
        try
        {

            _logger.LogInformation("Inicio de creacion masiva de producciones mediante interface");


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createDtos == null || !createDtos.Any())
                return BadRequest(new { message = "La coleccion de producciones no puede estar vacia" });

            const int idCreador = 1;
            var resultado = await _produccionInterfaceService.CreateProduccionesAsync(createDtos, idCreador);

            _logger.LogInformation(
                "Producciones procesadas: {TotalProcesados}, Creados: {CantidadCreados}, Obviados: {CantidadObviados}",
                resultado.TotalProcesados,
                resultado.CantidadCreados,
                resultado.CantidadObviados);

            return StatusCode(StatusCodes.Status201Created, resultado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validacion al crear producciones");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producciones mediante interface");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
