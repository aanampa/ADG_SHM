using SHM.AppDomain.DTOs.Tabla;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de tablas maestras del sistema, incluyendo operaciones CRUD y consultas por codigo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaService : ITablaService
{
    private readonly ITablaRepository _tablaRepository;

    public TablaService(ITablaRepository tablaRepository)
    {
        _tablaRepository = tablaRepository;
    }

    /// <summary>
    /// Obtiene todas las tablas maestras del sistema
    /// </summary>
    public async Task<IEnumerable<TablaResponseDto>> GetAllTablasAsync()
    {
        var tablas = await _tablaRepository.GetAllAsync();
        return tablas.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una tabla por su identificador unico
    /// </summary>
    public async Task<TablaResponseDto?> GetTablaByIdAsync(int id)
    {
        var tabla = await _tablaRepository.GetByIdAsync(id);
        return tabla != null ? MapToResponseDto(tabla) : null;
    }

    /// <summary>
    /// Obtiene una tabla por su GUID de registro
    /// </summary>
    public async Task<TablaResponseDto?> GetTablaByGuidAsync(string guidRegistro)
    {
        var tabla = await _tablaRepository.GetByGuidAsync(guidRegistro);
        return tabla != null ? MapToResponseDto(tabla) : null;
    }

    /// <summary>
    /// Obtiene una tabla por su codigo
    /// </summary>
    public async Task<TablaResponseDto?> GetTablaByCodigoAsync(string codigo)
    {
        var tabla = await _tablaRepository.GetByCodigoAsync(codigo);
        return tabla != null ? MapToResponseDto(tabla) : null;
    }

    /// <summary>
    /// Crea una nueva tabla maestra en el sistema
    /// </summary>
    public async Task<TablaResponseDto> CreateTablaAsync(CreateTablaDto createDto, int idCreador)
    {
        var tabla = new Tabla
        {
            Codigo = createDto.Codigo,
            Descripcion = createDto.Descripcion,
            IdCreador = idCreador,
            Activo = 1
        };

        var idTabla = await _tablaRepository.CreateAsync(tabla);
        var createdTabla = await _tablaRepository.GetByIdAsync(idTabla);

        return MapToResponseDto(createdTabla!);
    }

    /// <summary>
    /// Actualiza los datos de una tabla existente
    /// </summary>
    public async Task<bool> UpdateTablaAsync(int id, UpdateTablaDto updateDto, int idModificador)
    {
        var tablaExistente = await _tablaRepository.GetByIdAsync(id);
        if (tablaExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.Codigo))
            tablaExistente.Codigo = updateDto.Codigo;

        if (updateDto.Descripcion != null)
            tablaExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Activo.HasValue)
            tablaExistente.Activo = updateDto.Activo.Value;

        tablaExistente.IdModificador = idModificador;

        return await _tablaRepository.UpdateAsync(id, tablaExistente);
    }

    /// <summary>
    /// Elimina una tabla por su identificador
    /// </summary>
    public async Task<bool> DeleteTablaAsync(int id)
    {
        var exists = await _tablaRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _tablaRepository.DeleteAsync(id);
    }

    private static TablaResponseDto MapToResponseDto(Tabla tabla)
    {
        return new TablaResponseDto
        {
            IdTabla = tabla.IdTabla,
            Codigo = tabla.Codigo,
            Descripcion = tabla.Descripcion,
            Activo = tabla.Activo,
            GuidRegistro = tabla.GuidRegistro,
            FechaCreacion = tabla.FechaCreacion,
            FechaModificacion = tabla.FechaModificacion
        };
    }
}
