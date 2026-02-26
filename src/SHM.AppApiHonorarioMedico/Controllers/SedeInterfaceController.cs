using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Common;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la sincronizacion de sedes desde el API externo de San Pablo.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-25</created>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SedeInterfaceController : ControllerBase
{
    private readonly IProduccionInterfaceService _produccionInterfaceService;
    private readonly ILogger<SedeInterfaceController> _logger;

    public SedeInterfaceController(IProduccionInterfaceService produccionInterfaceService, ILogger<SedeInterfaceController> logger)
    {
        _produccionInterfaceService = produccionInterfaceService;
        _logger = logger;
    }

    /// <summary>
    /// Sincroniza todas las sedes desde el API de San Pablo.
    /// Consulta con Codigo=X para obtener todas las sedes y registra las que no existan localmente.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-25</created>
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<object>>> SyncSedes()
    {
        try
        {
            _logger.LogInformation("Inicio de sincronizacion de sedes desde API San Pablo");

            const int idCreador = 1;
            var sedesCreadas = await _produccionInterfaceService.SyncSedesFromApiAsync(idCreador);

            var resultado = new
            {
                SedesNuevas = sedesCreadas
            };

            _logger.LogInformation("Sincronizacion de sedes completada. Sedes nuevas: {SedesNuevas}", sedesCreadas);

            return Ok(ApiResponseDto<object>.Success(resultado, "Sincronizacion de sedes completada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar sedes desde API San Pablo");
            return StatusCode(500, ApiResponseDto<object>.Error("Error interno del servidor.", ex.Message));
        }
    }
}
