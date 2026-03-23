using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador TEMPORAL para validar el despliegue y configuracion del servidor.
/// IMPORTANTE: Ocultar o eliminar este controlador antes de pasar a produccion definitiva.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-21</created>
/// </summary>
public class DeployCheckController : Controller
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
    public IActionResult Config()
    {
        _logger.LogWarning("Acceso al endpoint temporal DeployCheck/Config");

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
            AppSettings = new
            {
                ApplicationCode          = _configuration["AppSettings:ApplicationCode"],
                InstanceName             = _configuration["AppSettings:InstanceName"],
                LoginBackgroundColor     = _configuration["AppSettings:LoginBackgroundColor"],
                CompanyName              = _configuration["AppSettings:CompanyName"],
                Version                  = _configuration["AppSettings:Version"],
                Year                     = _configuration["AppSettings:Year"],
                UrlBaseApp               = _configuration["AppSettings:UrlBaseApp"]
            },
            SmtpSettings = new
            {
                Host      = _configuration["SmtpSettings:Host"],
                Port      = _configuration["SmtpSettings:Port"],
                EnableSsl = _configuration["SmtpSettings:EnableSsl"],
                UserName  = _configuration["SmtpSettings:UserName"],
                Password  = _configuration["SmtpSettings:Password"],
                FromEmail = _configuration["SmtpSettings:FromEmail"],
                FromName  = _configuration["SmtpSettings:FromName"]
            },
            FileStorage = new
            {
                UploadPath = _configuration["FileStorage:UploadPath"]
            },
            SapApi = new
            {
                BaseUrl        = _configuration["SapApi:BaseUrl"],
                Username       = _configuration["SapApi:Username"],
                Password       = _configuration["SapApi:Password"],
                Scope          = _configuration["SapApi:Scope"],
                TimeoutSeconds = _configuration["SapApi:TimeoutSeconds"],
                EndpointToken  = _configuration["SapApi:EndpointToken"],
                EndpointBancos = _configuration["SapApi:EndpointBancos"]
            },
            _meta = new
            {
                Nota = "ENDPOINT TEMPORAL - Usar solo para validacion de despliegue",
                ServerDateTime = DateTime.Now,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "No definido"
            }
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        return Content(json, "application/json");
    }
}
