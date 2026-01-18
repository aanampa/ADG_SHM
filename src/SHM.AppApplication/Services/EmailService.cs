using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SHM.AppDomain.Configurations;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para el envio de correos electronicos del sistema, incluyendo emails de recuperacion de clave con plantillas HTML
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Envia un correo electronico de recuperacion de clave al usuario
    /// </summary>
    public async Task<bool> EnviarEmailRecuperacionAsync(string email, string nombreUsuario, string token, string baseUrl)
    {
        try
        {
            var resetUrl = $"{baseUrl}/Auth/RestablecerClave?token={token}";

            var subject = "Recuperación de Contraseña - Portal de Honorarios";

            // Intentar cargar plantilla desde archivo
            var templatePath = Path.Combine(baseUrl.Contains("localhost")
                ? Directory.GetCurrentDirectory()
                : System.AppDomain.CurrentDomain.BaseDirectory,
                "wwwroot", "archivos", "plantillas_html", "RecuperarClave.html");

            string body;
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

            await EnviarEmailAsync(email, subject, body, isHtml: true);

            _logger.LogInformation("Email de recuperación enviado a: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de recuperación a: {Email}", email);
            return false;
        }
    }

    private async Task EnviarEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
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
