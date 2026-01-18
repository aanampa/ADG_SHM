namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para el envio de correos electronicos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia un correo electronico para recuperacion de contrasena.
    /// </summary>
    Task<bool> EnviarEmailRecuperacionAsync(string email, string nombreUsuario, string token, string baseUrl);
}
