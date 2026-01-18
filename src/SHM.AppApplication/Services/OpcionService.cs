using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de opciones del menu del sistema, incluyendo operaciones CRUD y consultas jerarquicas por opcion padre
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class OpcionService : IOpcionService
{
    private readonly IOpcionRepository _opcionRepository;

    public OpcionService(IOpcionRepository opcionRepository)
    {
        _opcionRepository = opcionRepository;
    }

    /// <summary>
    /// Obtiene todas las opciones del menu del sistema
    /// </summary>
    public async Task<IEnumerable<OpcionResponseDto>> GetAllOpcionesAsync()
    {
        var opciones = await _opcionRepository.GetAllAsync();
        return opciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una opcion por su identificador unico
    /// </summary>
    public async Task<OpcionResponseDto?> GetOpcionByIdAsync(int id)
    {
        var opcion = await _opcionRepository.GetByIdAsync(id);
        return opcion != null ? MapToResponseDto(opcion) : null;
    }

    /// <summary>
    /// Obtiene una opcion por su GUID de registro
    /// </summary>
    public async Task<OpcionResponseDto?> GetOpcionByGuidAsync(string guidRegistro)
    {
        var opcion = await _opcionRepository.GetByGuidAsync(guidRegistro);
        return opcion != null ? MapToResponseDto(opcion) : null;
    }

    /// <summary>
    /// Obtiene las opciones hijas de una opcion padre especifica
    /// </summary>
    public async Task<IEnumerable<OpcionResponseDto>> GetOpcionesByPadreAsync(int? idOpcionPadre)
    {
        var opciones = await _opcionRepository.GetByPadreAsync(idOpcionPadre);
        return opciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva opcion de menu en el sistema
    /// </summary>
    public async Task<OpcionResponseDto> CreateOpcionAsync(CreateOpcionDto createDto, int idCreador)
    {
        var opcion = new Opcion
        {
            Nombre = createDto.Nombre,
            Url = createDto.Url,
            Icono = createDto.Icono,
            Orden = createDto.Orden,
            IdOpcionPadre = createDto.IdOpcionPadre,
            IdCreador = idCreador,
            Activo = 1
        };

        var idOpcion = await _opcionRepository.CreateAsync(opcion);
        var createdOpcion = await _opcionRepository.GetByIdAsync(idOpcion);

        return MapToResponseDto(createdOpcion!);
    }

    /// <summary>
    /// Actualiza los datos de una opcion existente
    /// </summary>
    public async Task<bool> UpdateOpcionAsync(int id, UpdateOpcionDto updateDto, int idModificador)
    {
        var opcionExistente = await _opcionRepository.GetByIdAsync(id);
        if (opcionExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.Nombre))
            opcionExistente.Nombre = updateDto.Nombre;

        if (updateDto.Url != null)
            opcionExistente.Url = updateDto.Url;

        if (updateDto.Icono != null)
            opcionExistente.Icono = updateDto.Icono;

        if (updateDto.Orden.HasValue)
            opcionExistente.Orden = updateDto.Orden;

        if (updateDto.IdOpcionPadre.HasValue)
            opcionExistente.IdOpcionPadre = updateDto.IdOpcionPadre;

        if (updateDto.Activo.HasValue)
            opcionExistente.Activo = updateDto.Activo.Value;

        opcionExistente.IdModificador = idModificador;

        return await _opcionRepository.UpdateAsync(id, opcionExistente);
    }

    /// <summary>
    /// Elimina una opcion por su identificador
    /// </summary>
    public async Task<bool> DeleteOpcionAsync(int id, int idModificador)
    {
        var exists = await _opcionRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _opcionRepository.DeleteAsync(id, idModificador);
    }

    private static OpcionResponseDto MapToResponseDto(Opcion opcion)
    {
        return new OpcionResponseDto
        {
            IdOpcion = opcion.IdOpcion,
            Nombre = opcion.Nombre,
            Url = opcion.Url,
            Icono = opcion.Icono,
            Orden = opcion.Orden,
            IdOpcionPadre = opcion.IdOpcionPadre,
            Activo = opcion.Activo,
            GuidRegistro = opcion.GuidRegistro,
            FechaCreacion = opcion.FechaCreacion,
            FechaModificacion = opcion.FechaModificacion
        };
    }
}
