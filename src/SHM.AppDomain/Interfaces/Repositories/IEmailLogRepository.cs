using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio para la gestion de logs de email.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-25</created>
/// </summary>
public interface IEmailLogRepository
{
    /// <summary>
    /// Crea un nuevo registro de log de email.
    /// </summary>
    Task<int> CreateAsync(EmailLog emailLog);

    /// <summary>
    /// Obtiene un log de email por su ID.
    /// </summary>
    Task<EmailLog?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los logs de email.
    /// </summary>
    Task<IEnumerable<EmailLog>> GetAllAsync();

    /// <summary>
    /// Obtiene logs de email por tipo.
    /// </summary>
    Task<IEnumerable<EmailLog>> GetByTipoAsync(string tipoEmail);

    /// <summary>
    /// Obtiene logs de email por estado.
    /// </summary>
    Task<IEnumerable<EmailLog>> GetByEstadoAsync(string estado);

    /// <summary>
    /// Obtiene logs de email por destinatario.
    /// </summary>
    Task<IEnumerable<EmailLog>> GetByEmailDestinoAsync(string emailDestino);
}
