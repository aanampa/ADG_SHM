using SHM.AppDomain.DTOs.TablaDetalle;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de detalles de tablas maestras, incluyendo operaciones CRUD y consultas por tabla padre
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaDetalleService : ITablaDetalleService
{
    private readonly ITablaDetalleRepository _tablaDetalleRepository;

    public TablaDetalleService(ITablaDetalleRepository tablaDetalleRepository)
    {
        _tablaDetalleRepository = tablaDetalleRepository;
    }

    /// <summary>
    /// Obtiene todos los detalles de tablas del sistema
    /// </summary>
    public async Task<IEnumerable<TablaDetalleResponseDto>> GetAllTablaDetallesAsync()
    {
        var tablaDetalles = await _tablaDetalleRepository.GetAllAsync();
        return tablaDetalles.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene los detalles de una tabla especifica por su identificador
    /// </summary>
    public async Task<IEnumerable<TablaDetalleResponseDto>> GetTablaDetallesByTablaIdAsync(int idTabla)
    {
        var tablaDetalles = await _tablaDetalleRepository.GetByTablaIdAsync(idTabla);
        return tablaDetalles.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene los detalles activos de una tabla especifica
    /// </summary>
    public async Task<IEnumerable<TablaDetalleResponseDto>> GetActivosByTablaIdAsync(int idTabla)
    {
        var tablaDetalles = await _tablaDetalleRepository.GetActivosByTablaIdAsync(idTabla);
        return tablaDetalles.Select(MapToResponseDto);
    }

    /// <summary>
    /// Lista los tipos de entidad medica disponibles
    /// </summary>
    public async Task<IEnumerable<TablaDetalleResponseDto>> ListarTipoEntidadMedicaAsync()
    {
        // ID_TABLA = 1 corresponde a los tipos de entidad médica
        var tablaDetalles = await _tablaDetalleRepository.GetActivosByTablaIdAsync(1);
        return tablaDetalles.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un detalle de tabla por su identificador unico
    /// </summary>
    public async Task<TablaDetalleResponseDto?> GetTablaDetalleByIdAsync(int id)
    {
        var tablaDetalle = await _tablaDetalleRepository.GetByIdAsync(id);
        return tablaDetalle != null ? MapToResponseDto(tablaDetalle) : null;
    }

    /// <summary>
    /// Obtiene un detalle de tabla por su GUID de registro
    /// </summary>
    public async Task<TablaDetalleResponseDto?> GetTablaDetalleByGuidAsync(string guidRegistro)
    {
        var tablaDetalle = await _tablaDetalleRepository.GetByGuidAsync(guidRegistro);
        return tablaDetalle != null ? MapToResponseDto(tablaDetalle) : null;
    }

    /// <summary>
    /// Obtiene un detalle de tabla por su codigo dentro de una tabla especifica
    /// </summary>
    public async Task<TablaDetalleResponseDto?> GetTablaDetalleByCodigoAsync(int idTabla, string codigo)
    {
        var tablaDetalle = await _tablaDetalleRepository.GetByCodigoAsync(idTabla, codigo);
        return tablaDetalle != null ? MapToResponseDto(tablaDetalle) : null;
    }

    /// <summary>
    /// Crea un nuevo detalle de tabla en el sistema
    /// </summary>
    public async Task<TablaDetalleResponseDto> CreateTablaDetalleAsync(CreateTablaDetalleDto createDto, int idCreador)
    {
        var tablaDetalle = new TablaDetalle
        {
            IdTabla = createDto.IdTabla,
            Codigo = createDto.Codigo,
            Descripcion = createDto.Descripcion,
            Orden = createDto.Orden,
            IdCreador = idCreador,
            Activo = 1
        };

        var idTablaDetalle = await _tablaDetalleRepository.CreateAsync(tablaDetalle);
        var createdTablaDetalle = await _tablaDetalleRepository.GetByIdAsync(idTablaDetalle);

        return MapToResponseDto(createdTablaDetalle!);
    }

    /// <summary>
    /// Actualiza los datos de un detalle de tabla existente
    /// </summary>
    public async Task<bool> UpdateTablaDetalleAsync(int id, UpdateTablaDetalleDto updateDto, int idModificador)
    {
        var tablaDetalleExistente = await _tablaDetalleRepository.GetByIdAsync(id);
        if (tablaDetalleExistente == null)
            return false;

        if (updateDto.IdTabla.HasValue)
            tablaDetalleExistente.IdTabla = updateDto.IdTabla.Value;

        if (!string.IsNullOrEmpty(updateDto.Codigo))
            tablaDetalleExistente.Codigo = updateDto.Codigo;

        if (updateDto.Descripcion != null)
            tablaDetalleExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Orden.HasValue)
            tablaDetalleExistente.Orden = updateDto.Orden;

        if (updateDto.Activo.HasValue)
            tablaDetalleExistente.Activo = updateDto.Activo.Value;

        tablaDetalleExistente.IdModificador = idModificador;

        return await _tablaDetalleRepository.UpdateAsync(id, tablaDetalleExistente);
    }

    /// <summary>
    /// Elimina un detalle de tabla por su identificador
    /// </summary>
    public async Task<bool> DeleteTablaDetalleAsync(int id)
    {
        var exists = await _tablaDetalleRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _tablaDetalleRepository.DeleteAsync(id);
    }

    private static TablaDetalleResponseDto MapToResponseDto(TablaDetalle tablaDetalle)
    {
        return new TablaDetalleResponseDto
        {
            IdTablaDetalle = tablaDetalle.IdTablaDetalle,
            IdTabla = tablaDetalle.IdTabla,
            Codigo = tablaDetalle.Codigo,
            Descripcion = tablaDetalle.Descripcion,
            Orden = tablaDetalle.Orden,
            Activo = tablaDetalle.Activo,
            GuidRegistro = tablaDetalle.GuidRegistro,
            FechaCreacion = tablaDetalle.FechaCreacion,
            FechaModificacion = tablaDetalle.FechaModificacion
        };
    }
}
