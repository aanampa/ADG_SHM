using SHM.AppDomain.DTOs.EntidadMedica;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de entidades medicas, incluyendo operaciones CRUD, busquedas por codigo y RUC, y paginacion
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadMedicaService : IEntidadMedicaService
{
    private readonly IEntidadMedicaRepository _entidadMedicaRepository;

    public EntidadMedicaService(IEntidadMedicaRepository entidadMedicaRepository)
    {
        _entidadMedicaRepository = entidadMedicaRepository;
    }

    /// <summary>
    /// Obtiene todas las entidades medicas del sistema
    /// </summary>
    public async Task<IEnumerable<EntidadMedicaResponseDto>> GetAllEntidadesMedicasAsync()
    {
        var entidades = await _entidadMedicaRepository.GetAllAsync();
        return entidades.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una entidad medica por su identificador unico
    /// </summary>
    public async Task<EntidadMedicaResponseDto?> GetEntidadMedicaByIdAsync(int id)
    {
        var entidad = await _entidadMedicaRepository.GetByIdAsync(id);
        return entidad != null ? MapToResponseDto(entidad) : null;
    }

    /// <summary>
    /// Obtiene una entidad medica por su codigo
    /// </summary>
    public async Task<EntidadMedicaResponseDto?> GetEntidadMedicaByCodigoAsync(string codigo)
    {
        var entidad = await _entidadMedicaRepository.GetByCodigoAsync(codigo);
        return entidad != null ? MapToResponseDto(entidad) : null;
    }

    /// <summary>
    /// Obtiene una entidad medica por su RUC
    /// </summary>
    public async Task<EntidadMedicaResponseDto?> GetEntidadMedicaByRucAsync(string ruc)
    {
        var entidad = await _entidadMedicaRepository.GetByRucAsync(ruc);
        return entidad != null ? MapToResponseDto(entidad) : null;
    }

    /// <summary>
    /// Crea una nueva entidad medica en el sistema
    /// </summary>
    public async Task<EntidadMedicaResponseDto> CreateEntidadMedicaAsync(CreateEntidadMedicaDto createDto, int idCreador)
    {
        var entidadMedica = new EntidadMedica
        {
            CodigoEntidad = createDto.CodigoEntidad,
            RazonSocial = createDto.RazonSocial,
            Ruc = createDto.Ruc,
            TipoEntidadMedica = createDto.TipoEntidadMedica,
            Telefono = createDto.Telefono,
            Celular = createDto.Celular,
            CodigoAcreedor = createDto.CodigoAcreedor,
            CodigoCorrientista = createDto.CodigoCorrientista,
            Direccion = createDto.Direccion,
            IdCreador = idCreador,
            Activo = 1
        };

        var idEntidadMedica = await _entidadMedicaRepository.CreateAsync(entidadMedica);
        var createdEntidad = await _entidadMedicaRepository.GetByIdAsync(idEntidadMedica);

        return MapToResponseDto(createdEntidad!);
    }

    /// <summary>
    /// Actualiza los datos de una entidad medica existente
    /// </summary>
    public async Task<bool> UpdateEntidadMedicaAsync(int id, UpdateEntidadMedicaDto updateDto, int idModificador)
    {
        var entidadExistente = await _entidadMedicaRepository.GetByIdAsync(id);
        if (entidadExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.CodigoEntidad))
            entidadExistente.CodigoEntidad = updateDto.CodigoEntidad;

        if (updateDto.RazonSocial != null)
            entidadExistente.RazonSocial = updateDto.RazonSocial;

        if (updateDto.Ruc != null)
            entidadExistente.Ruc = updateDto.Ruc;

        if (updateDto.TipoEntidadMedica != null)
            entidadExistente.TipoEntidadMedica = updateDto.TipoEntidadMedica;

        if (updateDto.Telefono != null)
            entidadExistente.Telefono = updateDto.Telefono;

        if (updateDto.Celular != null)
            entidadExistente.Celular = updateDto.Celular;

        if (updateDto.CodigoAcreedor != null)
            entidadExistente.CodigoAcreedor = updateDto.CodigoAcreedor;

        if (updateDto.CodigoCorrientista != null)
            entidadExistente.CodigoCorrientista = updateDto.CodigoCorrientista;

        if (updateDto.Direccion != null)
            entidadExistente.Direccion = updateDto.Direccion;

        if (updateDto.Activo.HasValue)
            entidadExistente.Activo = updateDto.Activo.Value;

        entidadExistente.IdModificador = idModificador;

        return await _entidadMedicaRepository.UpdateAsync(id, entidadExistente);
    }

    /// <summary>
    /// Elimina una entidad medica por su identificador
    /// </summary>
    public async Task<bool> DeleteEntidadMedicaAsync(int id, int idModificador)
    {
        var exists = await _entidadMedicaRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _entidadMedicaRepository.DeleteAsync(id, idModificador);
    }

    /// <summary>
    /// Obtiene una entidad medica por su GUID de registro
    /// </summary>
    public async Task<EntidadMedicaResponseDto?> GetEntidadMedicaByGuidAsync(string guidRegistro)
    {
        var entidad = await _entidadMedicaRepository.GetByGuidAsync(guidRegistro);
        return entidad != null ? MapToResponseDto(entidad) : null;
    }

    /// <summary>
    /// Obtiene entidades medicas paginadas con opcion de busqueda
    /// </summary>
    public async Task<(IEnumerable<EntidadMedicaResponseDto> Items, int TotalCount)> GetPaginatedAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _entidadMedicaRepository.GetPaginatedAsync(searchTerm, pageNumber, pageSize);
        var dtos = items.Select(MapToResponseDto);
        return (dtos, totalCount);
    }

    private static EntidadMedicaResponseDto MapToResponseDto(EntidadMedica entidad)
    {
        return new EntidadMedicaResponseDto
        {
            IdEntidadMedica = entidad.IdEntidadMedica,
            CodigoEntidad = entidad.CodigoEntidad,
            RazonSocial = entidad.RazonSocial,
            Ruc = entidad.Ruc,
            TipoEntidadMedica = entidad.TipoEntidadMedica,
            Telefono = entidad.Telefono,
            Celular = entidad.Celular,
            CodigoAcreedor = entidad.CodigoAcreedor,
            CodigoCorrientista = entidad.CodigoCorrientista,
            Direccion = entidad.Direccion,
            GuidRegistro = entidad.GuidRegistro,
            Activo = entidad.Activo,
            FechaCreacion = entidad.FechaCreacion,
            FechaModificacion = entidad.FechaModificacion
        };
    }
}
