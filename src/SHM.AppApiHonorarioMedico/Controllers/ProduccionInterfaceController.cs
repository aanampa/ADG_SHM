using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Common;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la creacion masiva de producciones a traves de interface.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-01-31 - Agregado metodo UpdateLiquidaciones</modified>
/// <modified>ADG Antonio - 2026-02-08 - Detalle de estado por registro en respuesta, log de errores</modified>
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
    /// Si ya existe, lo omite. Retorna el detalle de estado de cada registro procesado.
    ///
    /// <modified>ADG Antonio - 2026-02-08 - Detalle de estado por registro en respuesta</modified>
    /// </summary>
    [HttpPost("producciones")]
    [ProducesResponseType(typeof(ApiResponseDto<InterfaceProduccionResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<InterfaceProduccionResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<InterfaceProduccionResultDto>>> Create([FromBody] IEnumerable<CreateInterfaceProduccionDto> createDtos)
    {
        try
        {
            _logger.LogInformation("Inicio de creacion masiva de producciones mediante interface");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponseDto<InterfaceProduccionResultDto>.Error("Error de validacion.", errors));
            }

            if (createDtos == null || !createDtos.Any())
                return BadRequest(ApiResponseDto<InterfaceProduccionResultDto>.Error("Error de validacion.", "La coleccion de producciones no puede estar vacia"));

            const int idCreador = 1;
            var resultado = await _produccionInterfaceService.CreateProduccionesAsync(createDtos, idCreador);

            _logger.LogInformation(
                "Producciones procesadas: {TotalProcesados}, Creados: {CantidadCreados}, Obviados: {CantidadObviados}, Errores: {CantidadErrores}",
                resultado.TotalProcesados,
                resultado.CantidadCreados,
                resultado.CantidadObviados,
                resultado.CantidadErrores);

            return StatusCode(StatusCodes.Status201Created, ApiResponseDto<InterfaceProduccionResultDto>.Success(resultado, "Correcto."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producciones mediante interface");
            return StatusCode(500, ApiResponseDto<InterfaceProduccionResultDto>.Error("Error interno del servidor.", ex.Message));
        }
    }

    /// <summary>
    /// Actualiza los datos de liquidacion de multiples producciones.
    /// Busca por llave compuesta (CodigoSede, CodigoEntidad, CodigoProduccion, NumeroProduccion, TipoEntidadMedica).
    /// Si no existe la produccion, la omite. Retorna el detalle de estado de cada registro procesado.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-31</created>
    /// <modified>ADG Antonio - 2026-02-08 - Detalle de estado por registro en respuesta</modified>
    /// </summary>
    [HttpPost("liquidaciones")]
    [ProducesResponseType(typeof(ApiResponseDto<InterfaceProduccionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<InterfaceProduccionResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<InterfaceProduccionResultDto>>> UpdateLiquidaciones([FromBody] IEnumerable<UpdateInterfaceLiquidacionDto> updateDtos)
    {
        try
        {
            _logger.LogInformation("Inicio de actualizacion masiva de liquidaciones mediante interface");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponseDto<InterfaceProduccionResultDto>.Error("Error de validacion.", errors));
            }

            if (updateDtos == null || !updateDtos.Any())
                return BadRequest(ApiResponseDto<InterfaceProduccionResultDto>.Error("Error de validacion.", "La coleccion de liquidaciones no puede estar vacia"));

            const int idModificador = 1;
            var resultado = await _produccionInterfaceService.UpdateLiquidacionesAsync(updateDtos, idModificador);

            _logger.LogInformation(
                "Liquidaciones procesadas: {TotalProcesados}, Actualizados: {CantidadCreados}, Obviados: {CantidadObviados}, Errores: {CantidadErrores}",
                resultado.TotalProcesados,
                resultado.CantidadCreados,
                resultado.CantidadObviados,
                resultado.CantidadErrores);

            return Ok(ApiResponseDto<InterfaceProduccionResultDto>.Success(resultado, "Correcto."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar liquidaciones mediante interface");
            return StatusCode(500, ApiResponseDto<InterfaceProduccionResultDto>.Error("Error interno del servidor.", ex.Message));
        }
    }
}
