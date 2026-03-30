using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SHM.AppDomain.Configurations;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador de pruebas para diagnostico y verificacion de configuracion del sistema.
///
/// <author>ADG Vladimir</author>
/// <created>2026-03-23</created>
/// </summary>
public class TestController : Controller
{
    private readonly SmtpSettings _smtpSettings;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IOptions<SmtpSettings> smtpSettings,
        IConfiguration configuration,
        ILogger<TestController> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Muestra la pagina de pruebas con formulario de correo y datos de configuracion.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        CargarDatosConfiguracion();
        return View();
    }

    /// <summary>
    /// Envia un correo de prueba al destinatario indicado usando la configuracion SMTP del sistema.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCorreoPrueba(string correoDestino)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(correoDestino))
                return Json(new { success = false, message = "Debe ingresar un correo electronico." });

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password)
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = "Correo de prueba - Portal Honorarios Medicos",
                Body = $@"
                    <html><body>
                    <h2>Correo de Prueba</h2>
                    <p>Este es un correo de prueba generado desde el Portal de Honorarios Medicos.</p>
                    <p><strong>Servidor SMTP:</strong> {_smtpSettings.Host}:{_smtpSettings.Port}</p>
                    <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    </body></html>",
                IsBodyHtml = true
            };
            mensaje.To.Add(correoDestino);

            await client.SendMailAsync(mensaje);

            _logger.LogInformation("Correo de prueba enviado a {Correo}", correoDestino);
            return Json(new { success = true, message = $"Correo enviado exitosamente a {correoDestino}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo de prueba a {Correo}", correoDestino);
            return Json(new { success = false, message = $"Error al enviar correo: {ex.Message}" });
        }
    }

    private void CargarDatosConfiguracion()
    {
        // SMTP
        ViewBag.SmtpHost = _smtpSettings.Host;
        ViewBag.SmtpPort = _smtpSettings.Port;
        ViewBag.SmtpEnableSsl = _smtpSettings.EnableSsl;
        ViewBag.SmtpUserName = _smtpSettings.UserName;
        ViewBag.SmtpPassword = _smtpSettings.Password;
        ViewBag.SmtpFromEmail = _smtpSettings.FromEmail;
        ViewBag.SmtpFromName = _smtpSettings.FromName;

        // AppSettings
        ViewBag.AppInstanceName = _configuration["AppSettings:InstanceName"];
        ViewBag.AppApplicationCode = _configuration["AppSettings:ApplicationCode"];
        ViewBag.AppCompanyName = _configuration["AppSettings:CompanyName"];
        ViewBag.AppVersion = _configuration["AppSettings:Version"];
        ViewBag.AppUrlBase = _configuration["AppSettings:UrlBaseApp"];

        // ConnectionStrings (ocultando password)
        var connLocal = _configuration.GetConnectionString("OracleConnectionLocal") ?? "";
        var connNube = _configuration.GetConnectionString("OracleConnectionNube") ?? "";
        var connTest = _configuration.GetConnectionString("OracleConnectionTest") ?? "";
        var connActiva = _configuration.GetConnectionString("OracleConnection") ?? "";
        ViewBag.ConnLocal = OcultarPassword(connLocal);
        ViewBag.ConnNube = OcultarPassword(connNube);
        ViewBag.ConnTest = OcultarPassword(connTest);
        //ViewBag.ConnActiva = OcultarPassword(connActiva);
        ViewBag.ConnActiva = connActiva;

        // FileStorage
        ViewBag.FileStorageUploadPath = _configuration["FileStorage:UploadPath"];

        // SapApi
        ViewBag.SapBaseUrl = _configuration["SapApi:BaseUrl"];
        ViewBag.SapUsername = _configuration["SapApi:Username"];
        ViewBag.SapPassword = _configuration["SapApi:Password"];
        ViewBag.SapScope = _configuration["SapApi:Scope"];
        ViewBag.SapTimeoutSeconds = _configuration["SapApi:TimeoutSeconds"];
        ViewBag.SapEndpointToken = _configuration["SapApi:EndpointToken"];
        ViewBag.SapEndpointBancos = _configuration["SapApi:EndpointBancos"];

        // SanPabloApi
        ViewBag.SanPabloBaseUrl = _configuration["SanPabloApi:BaseUrl"];
        ViewBag.SanPabloUsuario = _configuration["SanPabloApi:Usuario"];
        ViewBag.SanPabloPassword = _configuration["SanPabloApi:Password"];
        ViewBag.SanPabloTimeoutSeconds = _configuration["SanPabloApi:TimeoutSeconds"];
        ViewBag.SanPabloEndpointLogin = _configuration["SanPabloApi:EndpointLogin"];
        ViewBag.SanPabloEndpointObtenerEntidad = _configuration["SanPabloApi:EndpointObtenerEntidad"];
        ViewBag.SanPabloEndpointObtenerSede = _configuration["SanPabloApi:EndpointObtenerSede"];
        ViewBag.SanPabloEndpointRegistrarComprobante = _configuration["SanPabloApi:EndpointRegistrarComprobante"];
    }

    private static string OcultarPassword(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) return "(no configurada)";
        // Oculta el valor de Password= en la cadena de conexion
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(Password=)[^;]+",
            "$1***",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
