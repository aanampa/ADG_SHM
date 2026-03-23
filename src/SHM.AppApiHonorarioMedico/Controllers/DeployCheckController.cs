using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador TEMPORAL para validar el despliegue y configuracion del servidor.
/// IMPORTANTE: Ocultar o eliminar este controlador antes de pasar a produccion definitiva.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-21</created>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DeployCheckController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeployCheckController> _logger;

    public DeployCheckController(IConfiguration configuration, ILogger<DeployCheckController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Retorna el contenido del appsettings.json para validar la configuracion del despliegue.
    /// TEMPORAL - Solo para uso en validacion de despliegue y pruebas.
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        _logger.LogWarning("Acceso al endpoint temporal DeployCheck/config");

        var config = new
        {
            Logging = new
            {
                LogLevel = new
                {
                    Default = _configuration["Logging:LogLevel:Default"],
                    MicrosoftAspNetCore = _configuration["Logging:LogLevel:Microsoft.AspNetCore"]
                }
            },
            AllowedHosts = _configuration["AllowedHosts"],
            ConnectionStrings = new
            {
                OracleConnection = _configuration.GetConnectionString("OracleConnection")
            },
            SanPabloApi = new
            {
                BaseUrl          = _configuration["SanPabloApi:BaseUrl"],
                Usuario          = _configuration["SanPabloApi:Usuario"],
                Password         = _configuration["SanPabloApi:Password"],
                TimeoutSeconds   = _configuration["SanPabloApi:TimeoutSeconds"],
                EndpointLogin               = _configuration["SanPabloApi:EndpointLogin"],
                EndpointObtenerEntidad      = _configuration["SanPabloApi:EndpointObtenerEntidad"],
                EndpointObtenerSede         = _configuration["SanPabloApi:EndpointObtenerSede"],
                EndpointRegistrarComprobante = _configuration["SanPabloApi:EndpointRegistrarComprobante"]
            },
            SapApi = new
            {
                BaseUrl                 = _configuration["SapApi:BaseUrl"],
                Username                = _configuration["SapApi:Username"],
                Password                = _configuration["SapApi:Password"],
                Scope                   = _configuration["SapApi:Scope"],
                TimeoutSeconds          = _configuration["SapApi:TimeoutSeconds"],
                EndpointToken           = _configuration["SapApi:EndpointToken"],
                EndpointBancos          = _configuration["SapApi:EndpointBancos"],
                EndpointCuentasAcreedor = _configuration["SapApi:EndpointCuentasAcreedor"]
            },
            _meta = new
            {
                Nota = "ENDPOINT TEMPORAL - Usar solo para validacion de despliegue",
                ServerDateTime = DateTime.Now,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "No definido"
            }
        };

        return Ok(config);
    }
}
