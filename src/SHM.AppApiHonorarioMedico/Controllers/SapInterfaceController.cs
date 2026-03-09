using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Banco;
using SHM.AppDomain.DTOs.Common;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la sincronizacion de datos desde SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-07</created>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SapInterfaceController : ControllerBase
{
    private readonly ISapApiService _sapApiService;
    private readonly IBancoService _bancoService;
    private readonly ILogger<SapInterfaceController> _logger;

    public SapInterfaceController(
        ISapApiService sapApiService,
        IBancoService bancoService,
        ILogger<SapInterfaceController> logger)
    {
        _sapApiService = sapApiService;
        _bancoService = bancoService;
        _logger = logger;
    }

    /// <summary>
    /// Sincroniza los bancos desde SAP (COD_BANCOSet).
    /// Obtiene la lista de bancos de SAP y los inserta en SHM_BANCO si no existen.
    /// </summary>
    [HttpPost("sincronizar-bancos")]
    [ProducesResponseType(typeof(ApiResponseDto<SincronizarBancosResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<SincronizarBancosResultDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<SincronizarBancosResultDto>>> SincronizarBancos()
    {
        try
        {
            _logger.LogInformation("Inicio de sincronizacion de bancos desde SAP");

            var bancosSap = await _sapApiService.GetBancosAsync();

            if (bancosSap == null || bancosSap.Count == 0)
            {
                _logger.LogWarning("No se obtuvieron bancos desde SAP");
                return Ok(ApiResponseDto<SincronizarBancosResultDto>.Error(
                    "No se obtuvieron bancos desde SAP."));
            }

            _logger.LogInformation("Se obtuvieron {Count} bancos desde SAP", bancosSap.Count);

            var resultado = new SincronizarBancosResultDto();
            const int idCreador = 1;

            foreach (var bancoSap in bancosSap)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(bancoSap.CodigoBanco))
                    {
                        resultado.CantidadErrores++;
                        resultado.Detalle.Add(new SincronizarBancoDetalleDto
                        {
                            CodigoBanco = "(vacio)",
                            Estado = "ER",
                            Mensaje = "Codigo de banco vacio"
                        });
                        continue;
                    }

                    // Verificar si ya existe
                    var bancoExistente = await _bancoService.GetBancoByCodigoAsync(bancoSap.CodigoBanco);

                    if (bancoExistente != null)
                    {
                        resultado.CantidadObviados++;
                        resultado.Detalle.Add(new SincronizarBancoDetalleDto
                        {
                            CodigoBanco = bancoSap.CodigoBanco,
                            NombreBanco = bancoSap.DescripcionBanco,
                            Estado = "OK",
                            Mensaje = "Ya existe"
                        });
                        continue;
                    }

                    // Insertar nuevo banco
                    var createDto = new CreateBancoDto
                    {
                        CodigoBanco = bancoSap.CodigoBanco,
                        NombreBanco = bancoSap.DescripcionBanco ?? string.Empty
                    };

                    await _bancoService.CreateBancoAsync(createDto, idCreador);

                    resultado.CantidadCreados++;
                    resultado.Detalle.Add(new SincronizarBancoDetalleDto
                    {
                        CodigoBanco = bancoSap.CodigoBanco,
                        NombreBanco = bancoSap.DescripcionBanco,
                        Estado = "OK",
                        Mensaje = "Creado"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al sincronizar banco {CodigoBanco}", bancoSap.CodigoBanco);
                    resultado.CantidadErrores++;
                    resultado.Detalle.Add(new SincronizarBancoDetalleDto
                    {
                        CodigoBanco = bancoSap.CodigoBanco,
                        NombreBanco = bancoSap.DescripcionBanco,
                        Estado = "ER",
                        Mensaje = ex.Message
                    });
                }
            }

            resultado.TotalProcesados = bancosSap.Count;

            _logger.LogInformation(
                "Sincronizacion de bancos finalizada. Total: {Total}, Creados: {Creados}, Existentes: {Existentes}, Errores: {Errores}",
                resultado.TotalProcesados, resultado.CantidadCreados, resultado.CantidadObviados, resultado.CantidadErrores);

            return Ok(ApiResponseDto<SincronizarBancosResultDto>.Success(resultado, "Sincronizacion completada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar bancos desde SAP");
            return StatusCode(500, ApiResponseDto<SincronizarBancosResultDto>.Error(
                "Error interno del servidor.", ex.Message));
        }
    }
}

/// <summary>
/// DTO con el resultado de la sincronizacion de bancos.
/// </summary>
public class SincronizarBancosResultDto
{
    public int TotalProcesados { get; set; }
    public int CantidadCreados { get; set; }
    public int CantidadObviados { get; set; }
    public int CantidadErrores { get; set; }
    public List<SincronizarBancoDetalleDto> Detalle { get; set; } = new();
}

/// <summary>
/// DTO con el detalle de cada banco procesado.
/// </summary>
public class SincronizarBancoDetalleDto
{
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
