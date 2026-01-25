namespace SHM.AppDomain.DTOs.EmailLog;

/// <summary>
/// DTO para la creacion de un registro de log de email.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-25</created>
/// </summary>
public class CreateEmailLogDto
{
    // Informacion del destinatario
    public string EmailDestino { get; set; } = string.Empty;
    public string? NombreDestino { get; set; }

    // Informacion del mensaje
    public string Asunto { get; set; } = string.Empty;
    public string TipoEmail { get; set; } = string.Empty;
    public string? Contenido { get; set; }
    public bool EsHtml { get; set; }

    // Estado del envio
    public string Estado { get; set; } = string.Empty;
    public string? MensajeError { get; set; }

    // Referencia opcional a entidades relacionadas
    public int? IdUsuario { get; set; }
    public int? IdEntidadMedica { get; set; }
    public string? EntidadReferencia { get; set; }
    public int? IdReferencia { get; set; }

    // Informacion tecnica
    public string? ServidorSmtp { get; set; }
    public string? IpOrigen { get; set; }
}
