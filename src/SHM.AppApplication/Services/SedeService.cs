using SHM.AppDomain.DTOs.Sede;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de sedes de la corporacion, incluyendo operaciones CRUD y consultas por codigo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class SedeService : ISedeService
{
    private readonly ISedeRepository _sedeRepository;

    public SedeService(ISedeRepository sedeRepository)
    {
        _sedeRepository = sedeRepository;
    }

    /// <summary>
    /// Obtiene todas las sedes del sistema
    /// </summary>
    public async Task<IEnumerable<SedeResponseDto>> GetAllSedesAsync()
    {
        var sedes = await _sedeRepository.GetAllAsync();
        return sedes.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una sede por su identificador unico
    /// </summary>
    public async Task<SedeResponseDto?> GetSedeByIdAsync(int id)
    {
        var sede = await _sedeRepository.GetByIdAsync(id);
        return sede != null ? MapToResponseDto(sede) : null;
    }

    /// <summary>
    /// Obtiene una sede por su codigo
    /// </summary>
    public async Task<SedeResponseDto?> GetSedeByCodigoAsync(string codigo)
    {
        var sede = await _sedeRepository.GetByCodigoAsync(codigo);
        return sede != null ? MapToResponseDto(sede) : null;
    }

    /// <summary>
    /// Obtiene una sede por su GUID de registro
    /// </summary>
    public async Task<SedeResponseDto?> GetSedeByGuidAsync(string guidRegistro)
    {
        var sede = await _sedeRepository.GetByGuidAsync(guidRegistro);
        return sede != null ? MapToResponseDto(sede) : null;
    }

    /// <summary>
    /// Crea una nueva sede en el sistema
    /// </summary>
    public async Task<SedeResponseDto> CreateSedeAsync(CreateSedeDto createDto, int idCreador)
    {
        var sede = new Sede
        {
            IdCorporacion = createDto.IdCorporacion,
            Codigo = createDto.Codigo,
            Nombre = createDto.Nombre,
            Ruc = createDto.Ruc,
            Direccion = createDto.Direccion,
            IdCreador = idCreador,
            Activo = 1
        };

        var idSede = await _sedeRepository.CreateAsync(sede);
        var createdSede = await _sedeRepository.GetByIdAsync(idSede);

        return MapToResponseDto(createdSede!);
    }

    /// <summary>
    /// Actualiza los datos de una sede existente
    /// </summary>
    public async Task<bool> UpdateSedeAsync(int id, UpdateSedeDto updateDto, int idModificador)
    {
        var sedeExistente = await _sedeRepository.GetByIdAsync(id);
        if (sedeExistente == null)
            return false;

        if (updateDto.IdCorporacion.HasValue)
            sedeExistente.IdCorporacion = updateDto.IdCorporacion;

        if (!string.IsNullOrEmpty(updateDto.Codigo))
            sedeExistente.Codigo = updateDto.Codigo;

        if (!string.IsNullOrEmpty(updateDto.Nombre))
            sedeExistente.Nombre = updateDto.Nombre;

        if (updateDto.Ruc != null)
            sedeExistente.Ruc = updateDto.Ruc;

        if (updateDto.Direccion != null)
            sedeExistente.Direccion = updateDto.Direccion;

        if (updateDto.Activo.HasValue)
            sedeExistente.Activo = updateDto.Activo.Value;

        sedeExistente.IdModificador = idModificador;

        return await _sedeRepository.UpdateAsync(id, sedeExistente);
    }

    /// <summary>
    /// Elimina una sede por su identificador
    /// </summary>
    public async Task<bool> DeleteSedeAsync(int id, int idModificador)
    {
        var exists = await _sedeRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _sedeRepository.DeleteAsync(id, idModificador);
    }

    private static SedeResponseDto MapToResponseDto(Sede sede)
    {
        return new SedeResponseDto
        {
            IdSede = sede.IdSede,
            IdCorporacion = sede.IdCorporacion,
            Codigo = sede.Codigo,
            Nombre = sede.Nombre,
            Ruc = sede.Ruc,
            Direccion = sede.Direccion,
            Activo = sede.Activo,
            GuidRegistro = sede.GuidRegistro,
            FechaCreacion = sede.FechaCreacion,
            FechaModificacion = sede.FechaModificacion
        };
    }
}
