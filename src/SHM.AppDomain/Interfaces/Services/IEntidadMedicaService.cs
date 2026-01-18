using SHM.AppDomain.DTOs.EntidadMedica;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de entidades medicas en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IEntidadMedicaService
{
    /// <summary>
    /// Obtiene todas las entidades medicas registradas en el sistema.
    /// </summary>
    Task<IEnumerable<EntidadMedicaResponseDto>> GetAllEntidadesMedicasAsync();

    /// <summary>
    /// Obtiene una entidad medica por su identificador unico.
    /// </summary>
    Task<EntidadMedicaResponseDto?> GetEntidadMedicaByIdAsync(int id);

    /// <summary>
    /// Obtiene una entidad medica por su codigo.
    /// </summary>
    Task<EntidadMedicaResponseDto?> GetEntidadMedicaByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una entidad medica por su numero de RUC.
    /// </summary>
    Task<EntidadMedicaResponseDto?> GetEntidadMedicaByRucAsync(string ruc);

    /// <summary>
    /// Crea una nueva entidad medica en el sistema.
    /// </summary>
    Task<EntidadMedicaResponseDto> CreateEntidadMedicaAsync(CreateEntidadMedicaDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una entidad medica existente.
    /// </summary>
    Task<bool> UpdateEntidadMedicaAsync(int id, UpdateEntidadMedicaDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una entidad medica registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteEntidadMedicaAsync(int id, int idModificador);

    /// <summary>
    /// Obtiene una entidad medica por su GUID de registro.
    /// </summary>
    Task<EntidadMedicaResponseDto?> GetEntidadMedicaByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene entidades medicas de forma paginada con opcion de busqueda.
    /// </summary>
    Task<(IEnumerable<EntidadMedicaResponseDto> Items, int TotalCount)> GetPaginatedAsync(string? searchTerm, int pageNumber, int pageSize);
}
