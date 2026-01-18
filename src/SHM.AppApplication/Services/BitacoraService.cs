using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de la bitacora de auditoria del sistema, incluyendo registro de acciones y consultas por entidad
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class BitacoraService : IBitacoraService
{
    private readonly IBitacoraRepository _bitacoraRepository;

    public BitacoraService(IBitacoraRepository bitacoraRepository)
    {
        _bitacoraRepository = bitacoraRepository;
    }

    /// <summary>
    /// Obtiene todos los registros de bitacora del sistema
    /// </summary>
    public async Task<IEnumerable<BitacoraResponseDto>> GetAllBitacorasAsync()
    {
        var bitacoras = await _bitacoraRepository.GetAllAsync();
        return bitacoras.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un registro de bitacora por su identificador unico
    /// </summary>
    public async Task<BitacoraResponseDto?> GetBitacoraByIdAsync(int id)
    {
        var bitacora = await _bitacoraRepository.GetByIdAsync(id);
        return bitacora != null ? MapToResponseDto(bitacora) : null;
    }

    /// <summary>
    /// Obtiene los registros de bitacora de una entidad especifica
    /// </summary>
    public async Task<IEnumerable<BitacoraResponseDto>> GetBitacorasByEntidadAsync(string entidad)
    {
        var bitacoras = await _bitacoraRepository.GetByEntidadAsync(entidad);
        return bitacoras.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea un nuevo registro de bitacora en el sistema
    /// </summary>
    public async Task<BitacoraResponseDto> CreateBitacoraAsync(CreateBitacoraDto createDto, int idCreador)
    {
        var bitacora = new Bitacora
        {
            Entidad = createDto.Entidad,
            Accion = createDto.Accion,
            Descripcion = createDto.Descripcion,
            IdCreador = idCreador,
            Activo = 1
        };

        var idBitacora = await _bitacoraRepository.CreateAsync(bitacora);
        var createdBitacora = await _bitacoraRepository.GetByIdAsync(idBitacora);

        return MapToResponseDto(createdBitacora!);
    }

    /// <summary>
    /// Actualiza un registro de bitacora existente
    /// </summary>
    public async Task<bool> UpdateBitacoraAsync(int id, UpdateBitacoraDto updateDto, int idModificador)
    {
        var bitacoraExistente = await _bitacoraRepository.GetByIdAsync(id);
        if (bitacoraExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.Entidad))
            bitacoraExistente.Entidad = updateDto.Entidad;

        if (!string.IsNullOrEmpty(updateDto.Accion))
            bitacoraExistente.Accion = updateDto.Accion;

        if (updateDto.Descripcion != null)
            bitacoraExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Activo.HasValue)
            bitacoraExistente.Activo = updateDto.Activo.Value;

        bitacoraExistente.IdModificador = idModificador;

        return await _bitacoraRepository.UpdateAsync(id, bitacoraExistente);
    }

    /// <summary>
    /// Elimina un registro de bitacora por su identificador
    /// </summary>
    public async Task<bool> DeleteBitacoraAsync(int id, int idModificador)
    {
        var exists = await _bitacoraRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _bitacoraRepository.DeleteAsync(id, idModificador);
    }

    private static BitacoraResponseDto MapToResponseDto(Bitacora bitacora)
    {
        return new BitacoraResponseDto
        {
            IdBitacora = bitacora.IdBitacora,
            Entidad = bitacora.Entidad,
            Accion = bitacora.Accion,
            Descripcion = bitacora.Descripcion,
            Activo = bitacora.Activo,
            GuidRegistro = bitacora.GuidRegistro,
            FechaCreacion = bitacora.FechaCreacion,
            FechaModificacion = bitacora.FechaModificacion
        };
    }
}
