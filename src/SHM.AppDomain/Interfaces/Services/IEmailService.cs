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

    /// <summary>
    /// Envia un correo electronico de solicitud de factura a la Cia Medica.
    /// </summary>
    /// <param name="email">Correo del destinatario</param>
    /// <param name="nombreDestinatario">Nombre del destinatario</param>
    /// <param name="codigoProduccion">Codigo de la produccion</param>
    /// <param name="razonSocial">Razon social de la Cia Medica</param>
    /// <param name="mtoTotal">Monto total de la produccion</param>
    /// <param name="fechaLimite">Fecha limite para enviar la factura</param>
    /// <param name="idEntidadMedica">ID de la entidad medica (para log)</param>
    /// <param name="idProduccion">ID de la produccion (para log)</param>
    Task<bool> EnviarEmailSolicitudFacturaAsync(
        string email,
        string nombreDestinatario,
        string codigoProduccion,
        string razonSocial,
        decimal? mtoTotal,
        DateTime fechaLimite,
        int? idEntidadMedica,
        int idProduccion);
}
