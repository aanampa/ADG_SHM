namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un registro de log de correo electronico enviado por el sistema.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-25</created>
/// </summary>
public class EmailLog
{
    public int IdEmailLog { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;

    // Informacion del remitente
    public string? EmailOrigen { get; set; }
    public string? NombreOrigen { get; set; }

    // Informacion del destinatario (TO) - soporta lista separada por coma
    public string EmailDestino { get; set; } = string.Empty;
    public string? NombreDestino { get; set; }

    // Destinatarios en copia (CC)
    public string? EmailCcLista { get; set; }
    public string? NombreCcLista { get; set; }

    // Informacion del mensaje
    public string Asunto { get; set; } = string.Empty;
    public string TipoEmail { get; set; } = string.Empty;
    public string? Contenido { get; set; }
    public int EsHtml { get; set; }

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

    // Campos de auditoria
    public int Activo { get; set; } = 1;
    public int? IdCreador { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
