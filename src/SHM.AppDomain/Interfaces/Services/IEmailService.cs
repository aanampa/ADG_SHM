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

    /// <summary>
    /// Envia un correo electronico notificando al usuario que su clave fue restablecida.
    /// </summary>
    /// <param name="email">Correo del usuario</param>
    /// <param name="nombreUsuario">Nombre completo del usuario</param>
    /// <param name="loginUsuario">Login del usuario</param>
    /// <param name="nuevaClave">Nueva clave generada</param>
    /// <param name="idUsuario">ID del usuario (para log)</param>
    /// <param name="tipoUsuario">Tipo de usuario: "I" = Interno (portal admin), "E" = Externo (portal cia medica)</param>
    Task<bool> EnviarEmailResetClaveAsync(string email, string nombreUsuario, string loginUsuario, string nuevaClave, int? idUsuario, string tipoUsuario = "I");

    /// <summary>
    /// Envia un correo electronico de bienvenida al nuevo usuario con sus credenciales de acceso.
    /// </summary>
    /// <param name="email">Correo del usuario</param>
    /// <param name="nombreUsuario">Nombre completo del usuario</param>
    /// <param name="loginUsuario">Login del usuario</param>
    /// <param name="claveUsuario">Clave generada</param>
    /// <param name="idUsuario">ID del usuario (para log)</param>
    /// <param name="tipoUsuario">Tipo de usuario: "I" = Interno (portal admin), "E" = Externo (portal cia medica)</param>
    Task<bool> EnviarEmailNuevoUsuarioAsync(string email, string nombreUsuario, string loginUsuario, string claveUsuario, int? idUsuario, string tipoUsuario = "I");

    /// <summary>
    /// Envia un correo electronico notificando al siguiente aprobador que tiene una orden de pago pendiente.
    /// </summary>
    /// <param name="email">Correo del aprobador</param>
    /// <param name="nombreAprobador">Nombre completo del aprobador</param>
    /// <param name="numeroOrdenPago">Numero de la orden de pago</param>
    /// <param name="fechaGeneracion">Fecha de generacion de la orden</param>
    /// <param name="montoTotal">Monto total de la orden</param>
    /// <param name="nombrePerfil">Nombre del perfil de aprobacion</param>
    /// <param name="idOrdenPago">ID de la orden de pago (para log)</param>
    Task<bool> EnviarEmailNotificacionAprobacionAsync(
        string email,
        string nombreAprobador,
        string numeroOrdenPago,
        DateTime? fechaGeneracion,
        decimal? montoTotal,
        string nombrePerfil,
        int idOrdenPago);

    /// <summary>
    /// Envia un correo electronico al creador de la orden de pago notificando que fue rechazada.
    /// </summary>
    /// <param name="email">Correo del creador</param>
    /// <param name="nombreCreador">Nombre del creador</param>
    /// <param name="numeroOrdenPago">Numero de la orden de pago</param>
    /// <param name="fechaGeneracion">Fecha de generacion de la orden</param>
    /// <param name="montoTotal">Monto total de la orden</param>
    /// <param name="perfilRechazo">Nombre del perfil que rechazo</param>
    /// <param name="nombreAprobador">Nombre del aprobador que rechazo</param>
    /// <param name="comentario">Motivo del rechazo (opcional)</param>
    /// <param name="idOrdenPago">ID de la orden de pago (para log)</param>
    Task<bool> EnviarEmailNotificacionRechazoAsync(
        string email,
        string nombreCreador,
        string numeroOrdenPago,
        DateTime? fechaGeneracion,
        decimal? montoTotal,
        string perfilRechazo,
        string nombreAprobador,
        string? comentario,
        int idOrdenPago);

    /// <summary>
    /// Envia un correo electronico al area de Tesoreria notificando que una orden de pago fue completamente aprobada.
    /// </summary>
    /// <param name="email">Correo del destinatario en Tesoreria</param>
    /// <param name="numeroOrdenPago">Numero de la orden de pago</param>
    /// <param name="nombreSede">Nombre de la sede</param>
    /// <param name="nombreBanco">Nombre del banco</param>
    /// <param name="fechaGeneracion">Fecha de generacion de la orden</param>
    /// <param name="montoTotal">Monto total de la orden</param>
    /// <param name="tablaAprobadoresHtml">HTML con las filas de la tabla de aprobadores</param>
    /// <param name="idOrdenPago">ID de la orden de pago (para log)</param>
    Task<bool> EnviarEmailNotificacionTesoreriaAsync(
        string email,
        string numeroOrdenPago,
        string nombreSede,
        string nombreBanco,
        DateTime? fechaGeneracion,
        decimal? montoTotal,
        string tablaAprobadoresHtml,
        int idOrdenPago);
}
