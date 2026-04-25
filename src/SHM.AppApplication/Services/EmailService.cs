using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SHM.AppDomain.Configurations;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para el envio de correos electronicos del sistema, incluyendo emails de recuperacion de clave con plantillas HTML.
/// Registra cada envio en la tabla SHM_EMAIL_LOG.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-25 - Agregado logging de emails en base de datos</modified>
/// <modified>ADG Vladimir D - 2026-04-11 - URLs de portales desde appsettings.json</modified>
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IConfiguration _configuration;

    public EmailService(
        IOptions<SmtpSettings> smtpSettings,
        ILogger<EmailService> logger,
        IEmailLogRepository emailLogRepository,
        IConfiguration configuration)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
        _emailLogRepository = emailLogRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// Envia un correo electronico de recuperacion de clave al usuario
    /// </summary>
    public async Task<bool> EnviarEmailRecuperacionAsync(string email, string nombreUsuario, string token, string baseUrl)
    {
        var resetUrl = $"{baseUrl}/Auth/RestablecerClave?token={token}";
        var subject = "Recuperación de Contraseña - Portal de Honorarios";
        string body;

        try
        {
            // Intentar cargar plantilla desde archivo
            var templatePath = Path.Combine(baseUrl.Contains("localhost")
                ? Directory.GetCurrentDirectory()
                : System.AppDomain.CurrentDomain.BaseDirectory,
                "wwwroot", "archivos", "plantillas_html", "RecuperarClave.html");

            if (File.Exists(templatePath))
            {
                body = await File.ReadAllTextAsync(templatePath);
                body = body.Replace("{{NOMBRE_USUARIO}}", nombreUsuario)
                          .Replace("{{URL_RECUPERACION}}", resetUrl)
                          .Replace("{{ANIO}}", DateTime.Now.Year.ToString());
            }
            else
            {
                // Usar plantilla embebida si no existe el archivo
                body = GenerarPlantillaRecuperacion(nombreUsuario, resetUrl);
            }

            await EnviarEmailConLogAsync(
                toEmail: email,
                nombreDestino: nombreUsuario,
                subject: subject,
                body: body,
                tipoEmail: "RECUPERACION_CLAVE",
                isHtml: true,
                idUsuario: null,
                idEntidadMedica: null,
                entidadReferencia: null,
                idReferencia: null);

            _logger.LogInformation("Email de recuperación enviado a: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de recuperación a: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Envia un email y guarda el log en la base de datos.
    /// </summary>
    private async Task EnviarEmailConLogAsync(
        string toEmail,
        string? nombreDestino,
        string subject,
        string body,
        string tipoEmail,
        bool isHtml = false,
        int? idUsuario = null,
        int? idEntidadMedica = null,
        string? entidadReferencia = null,
        int? idReferencia = null)
    {
        var emailLog = new EmailLog
        {
            GuidRegistro = Guid.NewGuid().ToString(),
            EmailOrigen = _smtpSettings.FromEmail,
            NombreOrigen = _smtpSettings.FromName,
            EmailDestino = toEmail,
            NombreDestino = nombreDestino,
            Asunto = subject,
            TipoEmail = tipoEmail,
            Contenido = body,
            EsHtml = isHtml ? 1 : 0,
            IdUsuario = idUsuario,
            IdEntidadMedica = idEntidadMedica,
            EntidadReferencia = entidadReferencia,
            IdReferencia = idReferencia,
            ServidorSmtp = $"{_smtpSettings.Host}:{_smtpSettings.Port}",
            Activo = 1
        };

        try
        {
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            // Email enviado exitosamente
            emailLog.Estado = "ENVIADO";
        }
        catch (Exception ex)
        {
            // Error al enviar email
            emailLog.Estado = "ERROR";
            emailLog.MensajeError = ex.Message.Length > 4000
                ? ex.Message.Substring(0, 4000)
                : ex.Message;

            // Re-lanzar la excepcion despues de guardar el log
            try
            {
                await _emailLogRepository.CreateAsync(emailLog);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error al guardar log de email fallido");
            }

            throw;
        }

        // Guardar log de email exitoso
        try
        {
            await _emailLogRepository.CreateAsync(emailLog);
        }
        catch (Exception logEx)
        {
            _logger.LogError(logEx, "Error al guardar log de email enviado");
        }
    }

    /// <summary>
    /// Envia un correo electronico de solicitud de factura a la Cia Medica.
    /// Intenta cargar la plantilla desde archivo HTML externo, si no existe usa plantilla embebida.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-01-26</created>
    /// </summary>
    public async Task<bool> EnviarEmailSolicitudFacturaAsync(
        string email,
        string nombreDestinatario,
        string codigoProduccion,
        string razonSocial,
        decimal? mtoTotal,
        DateTime fechaLimite,
        int? idEntidadMedica,
        int idProduccion)
    {
        var subject = $"Solicitud de Factura - Producción {codigoProduccion}";
        var montoFormateado = mtoTotal?.ToString("N2") ?? "0.00";
        var fechaFormateada = fechaLimite.ToString("dd/MM/yyyy");
        var horaFormateada = fechaLimite.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        string body;

        try
        {
            // Intentar cargar plantilla desde archivo
            var templatePath = ObtenerRutaPlantilla("SolicitudFactura.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("No se encontro la plantilla de email: {TemplatePath}", templatePath);
                return false;
            }

            body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{NOMBRE_DESTINATARIO}}", nombreDestinatario)
                      .Replace("{{CODIGO_PRODUCCION}}", codigoProduccion)
                      .Replace("{{RAZON_SOCIAL}}", razonSocial)
                      .Replace("{{MONTO_TOTAL}}", montoFormateado)
                      .Replace("{{FECHA_LIMITE}}", fechaFormateada)
                      .Replace("{{HORA_LIMITE}}", horaFormateada)
                      .Replace("{{URL_SISTEMA}}", _configuration["AppSettings:UrlPortalCompaniaMedica"] ?? "")
                      .Replace("{{ANIO}}", DateTime.Now.Year.ToString());

            await EnviarEmailConLogAsync(
                toEmail: email,
                nombreDestino: nombreDestinatario,
                subject: subject,
                body: body,
                tipoEmail: "SOLICITUD_FACTURA",
                isHtml: true,
                idUsuario: null,
                idEntidadMedica: idEntidadMedica,
                entidadReferencia: "SHM_PRODUCCION",
                idReferencia: idProduccion);

            _logger.LogInformation("Email de solicitud de factura enviado a: {Email}, Produccion: {Codigo}",
                email, codigoProduccion);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de solicitud de factura a: {Email}, Produccion: {Codigo}",
                email, codigoProduccion);
            return false;
        }
    }

    /// <summary>
    /// Envia un correo electronico notificando a la Cia Medica que su factura fue devuelta.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-23</created>
    /// </summary>
    public async Task<bool> EnviarEmailFacturaDevueltaAsync(
        string email,
        string nombreDestinatario,
        string codigoProduccion,
        string razonSocial,
        decimal? mtoTotal,
        DateTime fechaLimite,
        int? idEntidadMedica,
        int idProduccion)
    {
        var subject = $"Factura Devuelta - Producción {codigoProduccion}";
        var montoFormateado = mtoTotal?.ToString("N2") ?? "0.00";
        var fechaFormateada = fechaLimite.ToString("dd/MM/yyyy");
        var horaFormateada  = fechaLimite.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        string body;

        try
        {
            var templatePath = ObtenerRutaPlantilla("FacturaDevuelta.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("No se encontro la plantilla de email: {TemplatePath}", templatePath);
                return false;
            }

            body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{NOMBRE_DESTINATARIO}}", nombreDestinatario)
                       .Replace("{{CODIGO_PRODUCCION}}", codigoProduccion)
                       .Replace("{{RAZON_SOCIAL}}", razonSocial)
                       .Replace("{{MONTO_TOTAL}}", montoFormateado)
                       .Replace("{{FECHA_LIMITE}}", fechaFormateada)
                       .Replace("{{HORA_LIMITE}}", horaFormateada)
                       .Replace("{{URL_SISTEMA}}", _configuration["AppSettings:UrlPortalCompaniaMedica"] ?? "")
                       .Replace("{{ANIO}}", DateTime.Now.Year.ToString());

            await EnviarEmailConLogAsync(
                toEmail: email,
                nombreDestino: nombreDestinatario,
                subject: subject,
                body: body,
                tipoEmail: "FACTURA_DEVUELTA",
                isHtml: true,
                idUsuario: null,
                idEntidadMedica: idEntidadMedica,
                entidadReferencia: "SHM_PRODUCCION",
                idReferencia: idProduccion);

            _logger.LogInformation("Email de factura devuelta enviado a: {Email}, Produccion: {Codigo}",
                email, codigoProduccion);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de factura devuelta a: {Email}, Produccion: {Codigo}",
                email, codigoProduccion);
            return false;
        }
    }

    /// <summary>
    /// Envia un correo electronico notificando al usuario que su clave fue restablecida por un administrador.
    /// <modified>ADG Vladimir D - 2026-04-10 - Usar URL de portal segun tipo de usuario</modified>
    /// </summary>
    public async Task<bool> EnviarEmailResetClaveAsync(string email, string nombreUsuario, string loginUsuario, string nuevaClave, int? idUsuario, string tipoUsuario = "I")
    {
        var subject = "Clave Restablecida - Sistema de Honorarios Medicos";
        string body;

        try
        {
            var templatePath = ObtenerRutaPlantilla("ResetClave.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("No se encontro la plantilla de email: {TemplatePath}", templatePath);
                return false;
            }

            body = await File.ReadAllTextAsync(templatePath);
            var urlSistema = tipoUsuario == "E"
                ? _configuration["AppSettings:UrlPortalCompaniaMedica"] ?? ""
                : _configuration["AppSettings:UrlPortalAdministrativo"] ?? "";

            body = body.Replace("{{NOMBRE_USUARIO}}", nombreUsuario)
                      .Replace("{{LOGIN_USUARIO}}", loginUsuario)
                      .Replace("{{NUEVA_CLAVE}}", nuevaClave)
                      .Replace("{{URL_SISTEMA}}", urlSistema)
                      .Replace("{{ANIO}}", DateTime.Now.Year.ToString());

            await EnviarEmailConLogAsync(
                toEmail: email,
                nombreDestino: nombreUsuario,
                subject: subject,
                body: body,
                tipoEmail: "RESET_CLAVE",
                isHtml: true,
                idUsuario: idUsuario,
                idEntidadMedica: null,
                entidadReferencia: "SHM_SEG_USUARIO",
                idReferencia: idUsuario);

            _logger.LogInformation("Email de reset de clave enviado a: {Email}, Usuario: {Login}", email, loginUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de reset de clave a: {Email}, Usuario: {Login}", email, loginUsuario);
            return false;
        }
    }

    /// <summary>
    /// Envia un correo electronico de bienvenida al nuevo usuario con sus credenciales de acceso.
    /// <modified>ADG Vladimir D - 2026-04-10 - Usar URL de portal segun tipo de usuario</modified>
    /// </summary>
    public async Task<bool> EnviarEmailNuevoUsuarioAsync(string email, string nombreUsuario, string loginUsuario, string claveUsuario, int? idUsuario, string tipoUsuario = "I")
    {
        var subject = "Credenciales de Acceso - Sistema de Honorarios Medicos";
        string body;

        try
        {
            var templatePath = ObtenerRutaPlantilla("NuevoUsuario.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("No se encontro la plantilla de email: {TemplatePath}", templatePath);
                return false;
            }

            body = await File.ReadAllTextAsync(templatePath);
            var urlSistema = tipoUsuario == "E"
                ? _configuration["AppSettings:UrlPortalCompaniaMedica"] ?? ""
                : _configuration["AppSettings:UrlPortalAdministrativo"] ?? "";

            body = body.Replace("{{NOMBRE_USUARIO}}", nombreUsuario)
                      .Replace("{{LOGIN_USUARIO}}", loginUsuario)
                      .Replace("{{CLAVE_USUARIO}}", claveUsuario)
                      .Replace("{{URL_SISTEMA}}", urlSistema)
                      .Replace("{{ANIO}}", DateTime.Now.Year.ToString());

            await EnviarEmailConLogAsync(
                toEmail: email,
                nombreDestino: nombreUsuario,
                subject: subject,
                body: body,
                tipoEmail: "NUEVO_USUARIO",
                isHtml: true,
                idUsuario: idUsuario,
                idEntidadMedica: null,
                entidadReferencia: "SHM_SEG_USUARIO",
                idReferencia: idUsuario);

            _logger.LogInformation("Email de nuevo usuario enviado a: {Email}, Usuario: {Login}", email, loginUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de nuevo usuario a: {Email}, Usuario: {Login}", email, loginUsuario);
            return false;
        }
    }

    /// <summary>
    /// Envia un correo electronico notificando al siguiente aprobador que tiene una orden de pago pendiente.
    /// <modified>ADG Vladimir D - 2026-04-10 - Usar URL del portal administrativo</modified>
    /// </summary>
    public async Task<bool> EnviarEmailNotificacionAprobacionAsync(
        string email,
        string nombreAprobador,
        string numeroOrdenPago,
        DateTime? fechaGeneracion,
        decimal? montoTotal,
        string nombrePerfil,
        int idOrdenPago)
    {
        var subject = $"Orden de Pago {numeroOrdenPago} - Pendiente de Aprobacion";
        string body;

        try
        {
            var templatePath = ObtenerRutaPlantilla("NotificacionAprobacion.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("No se encontro la plantilla de email: {TemplatePath}", templatePath);
                return false;
            }

            body = await File.ReadAllTextAsync(templatePath);

            body = body.Replace("{{NOMBRE_USUARIO}}", nombreAprobador)
                      .Replace("{{NUMERO_ORDEN_PAGO}}", numeroOrdenPago ?? "-")
                      .Replace("{{FECHA_GENERACION}}", fechaGeneracion?.ToString("dd/MM/yyyy") ?? "-")
                      .Replace("{{MONTO_TOTAL}}", montoTotal?.ToString("N2") ?? "0.00")
                      .Replace("{{NOMBRE_PERFIL}}", nombrePerfil ?? "-")
                      .Replace("{{URL_SISTEMA}}", _configuration["AppSettings:UrlPortalAdministrativo"] ?? "")
                      .Replace("{{ANIO}}", DateTime.Now.Year.ToString());

            await EnviarEmailConLogAsync(
                toEmail: email,
                nombreDestino: nombreAprobador,
                subject: subject,
                body: body,
                tipoEmail: "NOTIFICACION_APROBACION",
                isHtml: true,
                idUsuario: null,
                idEntidadMedica: null,
                entidadReferencia: "SHM_ORDEN_PAGO",
                idReferencia: idOrdenPago);

            _logger.LogInformation("Email de notificacion de aprobacion enviado a: {Email}, OrdenPago: {NumeroOP}, Perfil: {Perfil}",
                email, numeroOrdenPago, nombrePerfil);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de notificacion de aprobacion a: {Email}, OrdenPago: {NumeroOP}",
                email, numeroOrdenPago);
            return false;
        }
    }

    /// <summary>
    /// Obtiene la ruta completa de una plantilla HTML, buscando primero en el directorio actual
    /// (desarrollo) y luego en el directorio base de la aplicacion (publicado).
    /// </summary>
    private static string ObtenerRutaPlantilla(string nombreArchivo)
    {
        // Buscar primero en el directorio actual (desarrollo)
        var rutaDesarrollo = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot", "archivos", "plantillas_html", nombreArchivo);

        if (File.Exists(rutaDesarrollo))
            return rutaDesarrollo;

        // Buscar en el directorio base de la aplicacion (publicado)
        return Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory,
            "wwwroot", "archivos", "plantillas_html", nombreArchivo);
    }

    private static string GenerarPlantillaRecuperacion(string nombreUsuario, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #5a1160 0%, #f26522 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>Portal de Honorarios Médicos</h1>
    </div>

    <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Recuperación de Contraseña</h2>

        <p>Hola <strong>{nombreUsuario}</strong>,</p>

        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en el Portal de Honorarios Médicos.</p>

        <p>Haz clic en el siguiente botón para crear una nueva contraseña:</p>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetUrl}' style='background: linear-gradient(135deg, #5a1160 0%, #f26522 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>
                Restablecer Contraseña
            </a>
        </div>

        <p style='color: #666; font-size: 14px;'>
            <strong>Importante:</strong> Este enlace expirará en <strong>1 hora</strong> por motivos de seguridad.
        </p>

        <p style='color: #666; font-size: 14px;'>
            Si no solicitaste restablecer tu contraseña, puedes ignorar este correo. Tu cuenta permanecerá segura.
        </p>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <p style='color: #999; font-size: 12px; text-align: center;'>
            Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:<br>
            <a href='{resetUrl}' style='color: #5a1160; word-break: break-all;'>{resetUrl}</a>
        </p>

        <p style='color: #999; font-size: 12px; text-align: center; margin-top: 20px;'>
            Este es un correo automático, por favor no respondas a este mensaje.
        </p>
    </div>
</body>
</html>";
    }
}
