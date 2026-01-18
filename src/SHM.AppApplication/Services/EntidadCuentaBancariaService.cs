using SHM.AppDomain.DTOs.EntidadCuentaBancaria;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de cuentas bancarias de entidades medicas, incluyendo operaciones CRUD y consultas por entidad
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadCuentaBancariaService : IEntidadCuentaBancariaService
{
    private readonly IEntidadCuentaBancariaRepository _entidadCuentaBancariaRepository;

    public EntidadCuentaBancariaService(IEntidadCuentaBancariaRepository entidadCuentaBancariaRepository)
    {
        _entidadCuentaBancariaRepository = entidadCuentaBancariaRepository;
    }

    /// <summary>
    /// Obtiene todas las cuentas bancarias de entidades
    /// </summary>
    public async Task<IEnumerable<EntidadCuentaBancariaResponseDto>> GetAllEntidadCuentasBancariasAsync()
    {
        var cuentas = await _entidadCuentaBancariaRepository.GetAllAsync();
        return cuentas.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una cuenta bancaria por su identificador unico
    /// </summary>
    public async Task<EntidadCuentaBancariaResponseDto?> GetEntidadCuentaBancariaByIdAsync(int id)
    {
        var cuenta = await _entidadCuentaBancariaRepository.GetByIdAsync(id);
        return cuenta != null ? MapToResponseDto(cuenta) : null;
    }

    /// <summary>
    /// Obtiene las cuentas bancarias de una entidad especifica
    /// </summary>
    public async Task<IEnumerable<EntidadCuentaBancariaResponseDto>> GetEntidadCuentasBancariasByEntidadIdAsync(int idEntidad)
    {
        var cuentas = await _entidadCuentaBancariaRepository.GetByEntidadIdAsync(idEntidad);
        return cuentas.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva cuenta bancaria para una entidad
    /// </summary>
    public async Task<EntidadCuentaBancariaResponseDto> CreateEntidadCuentaBancariaAsync(CreateEntidadCuentaBancariaDto createDto, int idCreador)
    {
        var entidadCuentaBancaria = new EntidadCuentaBancaria
        {
            IdEntidad = createDto.IdEntidad,
            IdBanco = createDto.IdBanco,
            CuentaCorriente = createDto.CuentaCorriente,
            CuentaCci = createDto.CuentaCci,
            Moneda = createDto.Moneda,
            IdCreador = idCreador,
            Activo = 1
        };

        var idCuentaBancaria = await _entidadCuentaBancariaRepository.CreateAsync(entidadCuentaBancaria);
        var createdCuenta = await _entidadCuentaBancariaRepository.GetByIdAsync(idCuentaBancaria);

        return MapToResponseDto(createdCuenta!);
    }

    /// <summary>
    /// Actualiza los datos de una cuenta bancaria existente
    /// </summary>
    public async Task<bool> UpdateEntidadCuentaBancariaAsync(int id, UpdateEntidadCuentaBancariaDto updateDto, int idModificador)
    {
        var cuentaExistente = await _entidadCuentaBancariaRepository.GetByIdAsync(id);
        if (cuentaExistente == null)
            return false;

        if (updateDto.IdEntidad.HasValue)
            cuentaExistente.IdEntidad = updateDto.IdEntidad;

        if (updateDto.IdBanco.HasValue)
            cuentaExistente.IdBanco = updateDto.IdBanco;

        if (updateDto.CuentaCorriente != null)
            cuentaExistente.CuentaCorriente = updateDto.CuentaCorriente;

        if (updateDto.CuentaCci != null)
            cuentaExistente.CuentaCci = updateDto.CuentaCci;

        if (updateDto.Moneda != null)
            cuentaExistente.Moneda = updateDto.Moneda;

        if (updateDto.Activo.HasValue)
            cuentaExistente.Activo = updateDto.Activo.Value;

        cuentaExistente.IdModificador = idModificador;

        return await _entidadCuentaBancariaRepository.UpdateAsync(id, cuentaExistente);
    }

    /// <summary>
    /// Elimina una cuenta bancaria por su identificador
    /// </summary>
    public async Task<bool> DeleteEntidadCuentaBancariaAsync(int id, int idModificador)
    {
        var exists = await _entidadCuentaBancariaRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _entidadCuentaBancariaRepository.DeleteAsync(id, idModificador);
    }

    /// <summary>
    /// Obtiene una cuenta bancaria por su GUID de registro
    /// </summary>
    public async Task<EntidadCuentaBancariaResponseDto?> GetEntidadCuentaBancariaByGuidAsync(string guidRegistro)
    {
        var cuenta = await _entidadCuentaBancariaRepository.GetByGuidAsync(guidRegistro);
        return cuenta != null ? MapToResponseDto(cuenta) : null;
    }

    private static EntidadCuentaBancariaResponseDto MapToResponseDto(EntidadCuentaBancaria cuenta)
    {
        return new EntidadCuentaBancariaResponseDto
        {
            IdCuentaBancaria = cuenta.IdCuentaBancaria,
            IdEntidad = cuenta.IdEntidad,
            IdBanco = cuenta.IdBanco,
            CuentaCorriente = cuenta.CuentaCorriente,
            CuentaCci = cuenta.CuentaCci,
            Moneda = cuenta.Moneda,
            GuidRegistro = cuenta.GuidRegistro,
            Activo = cuenta.Activo,
            FechaCreacion = cuenta.FechaCreacion,
            FechaModificacion = cuenta.FechaModificacion
        };
    }
}
