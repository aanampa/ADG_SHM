using SHM.AppDomain.DTOs.EntidadContacto;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de contactos de notificacion de entidades medicas.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public class EntidadContactoService : IEntidadContactoService
{
    private readonly IEntidadContactoRepository _repository;

    public EntidadContactoService(IEntidadContactoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EntidadContactoResponseDto>> GetByEntidadMedicaAsync(int idEntidadMedica, bool soloActivos = true)
    {
        var contactos = await _repository.GetByEntidadMedicaAsync(idEntidadMedica, soloActivos);
        return contactos.Select(MapToDto);
    }

    public async Task<EntidadContactoResponseDto?> GetByGuidAsync(string guidRegistro)
    {
        var contacto = await _repository.GetByGuidAsync(guidRegistro);
        return contacto != null ? MapToDto(contacto) : null;
    }

    public async Task<EntidadContactoResponseDto> CreateAsync(CreateEntidadContactoDto createDto, int idCreador)
    {
        var contacto = new EntidadContacto
        {
            IdEntidadMedica = createDto.IdEntidadMedica,
            ApellidoPaterno = createDto.ApellidoPaterno,
            ApellidoMaterno = createDto.ApellidoMaterno,
            Nombres         = createDto.Nombres,
            Celular         = createDto.Celular,
            Email           = createDto.Email,
            Cargo           = createDto.Cargo,
            Activo          = 1,
            IdCreador       = idCreador
        };

        var id = await _repository.CreateAsync(contacto);
        var created = await _repository.GetByIdAsync(id);
        return MapToDto(created!);
    }

    public async Task<bool> UpdateAsync(string guidRegistro, UpdateEntidadContactoDto updateDto, int idModificador)
    {
        var contacto = await _repository.GetByGuidAsync(guidRegistro);
        if (contacto == null) return false;

        if (updateDto.ApellidoPaterno != null) contacto.ApellidoPaterno = updateDto.ApellidoPaterno;
        if (updateDto.ApellidoMaterno != null) contacto.ApellidoMaterno = updateDto.ApellidoMaterno;
        if (updateDto.Nombres        != null) contacto.Nombres          = updateDto.Nombres;
        if (updateDto.Celular        != null) contacto.Celular          = updateDto.Celular;
        if (updateDto.Email          != null) contacto.Email            = updateDto.Email;
        if (updateDto.Cargo          != null) contacto.Cargo            = updateDto.Cargo;
        if (updateDto.Activo.HasValue)        contacto.Activo           = updateDto.Activo.Value;

        contacto.IdModificador = idModificador;

        return await _repository.UpdateAsync(contacto.IdEntidadContacto, contacto);
    }

    public async Task<bool> DeleteAsync(string guidRegistro, int idModificador)
    {
        var contacto = await _repository.GetByGuidAsync(guidRegistro);
        if (contacto == null) return false;

        return await _repository.DeleteAsync(contacto.IdEntidadContacto, idModificador);
    }

    public Task<bool> ExisteEmailEnEntidadAsync(int idEntidadMedica, string email, int? excludeId = null)
        => _repository.ExisteEmailEnEntidadAsync(idEntidadMedica, email, excludeId);

    public async Task<bool> ToggleActivoAsync(string guidRegistro, int idModificador)
    {
        var contacto = await _repository.GetByGuidAsync(guidRegistro);
        if (contacto == null) return false;

        contacto.Activo         = contacto.Activo == 1 ? 0 : 1;
        contacto.IdModificador  = idModificador;

        return await _repository.UpdateAsync(contacto.IdEntidadContacto, contacto);
    }

    private static EntidadContactoResponseDto MapToDto(EntidadContacto c) => new()
    {
        IdEntidadContacto = c.IdEntidadContacto,
        IdEntidadMedica   = c.IdEntidadMedica,
        ApellidoPaterno   = c.ApellidoPaterno,
        ApellidoMaterno   = c.ApellidoMaterno,
        Nombres           = c.Nombres,
        Celular           = c.Celular,
        Email             = c.Email,
        Cargo             = c.Cargo,
        GuidRegistro      = c.GuidRegistro,
        Activo            = c.Activo,
        FechaCreacion     = c.FechaCreacion,
        FechaModificacion = c.FechaModificacion
    };
}
