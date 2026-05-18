using SHM.AppDomain.DTOs.EntidadContacto;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de contactos de entidades medicas.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public interface IEntidadContactoService
{
    /// <summary>
    /// Obtiene contactos de una entidad medica. soloActivos=true para notificaciones, false para la lista UI.
    /// </summary>
    Task<IEnumerable<EntidadContactoResponseDto>> GetByEntidadMedicaAsync(int idEntidadMedica, bool soloActivos = true);

    /// <summary>
    /// Cambia el estado ACTIVO del contacto (1→0 o 0→1).
    /// </summary>
    Task<bool> ToggleActivoAsync(string guidRegistro, int idModificador);

    /// <summary>
    /// Obtiene un contacto por su GUID de registro.
    /// </summary>
    Task<EntidadContactoResponseDto?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Crea un nuevo contacto para una entidad medica.
    /// </summary>
    Task<EntidadContactoResponseDto> CreateAsync(CreateEntidadContactoDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un contacto existente.
    /// </summary>
    Task<bool> UpdateAsync(string guidRegistro, UpdateEntidadContactoDto updateDto, int idModificador);

    /// <summary>
    /// Elimina logicamente un contacto.
    /// </summary>
    Task<bool> DeleteAsync(string guidRegistro, int idModificador);

    /// <summary>
    /// Verifica si ya existe un contacto con el mismo email en la entidad medica.
    /// </summary>
    Task<bool> ExisteEmailEnEntidadAsync(int idEntidadMedica, string email, int? excludeId = null);
}
