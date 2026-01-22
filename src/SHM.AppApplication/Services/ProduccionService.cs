using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de producciones medicas, incluyendo operaciones CRUD y consultas por sede, entidad medica y periodo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-20 - Agregado metodo de listado paginado con filtros</modified>
/// </summary>
public class ProduccionService : IProduccionService
{
    private readonly IProduccionRepository _produccionRepository;

    public ProduccionService(IProduccionRepository produccionRepository)
    {
        _produccionRepository = produccionRepository;
    }

    /// <summary>
    /// Obtiene todas las producciones del sistema
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetAllProduccionesAsync()
    {
        var producciones = await _produccionRepository.GetAllAsync();
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una produccion por su identificador unico
    /// </summary>
    public async Task<ProduccionResponseDto?> GetProduccionByIdAsync(int id)
    {
        var produccion = await _produccionRepository.GetByIdAsync(id);
        return produccion != null ? MapToResponseDto(produccion) : null;
    }

    /// <summary>
    /// Obtiene una produccion por su codigo
    /// </summary>
    public async Task<ProduccionResponseDto?> GetProduccionByCodigoAsync(string codigo)
    {
        var produccion = await _produccionRepository.GetByCodigoAsync(codigo);
        return produccion != null ? MapToResponseDto(produccion) : null;
    }

    /// <summary>
    /// Obtiene una produccion por su GUID de registro con datos relacionados
    ///
    /// <modified>ADG Vladimir D - 2025-01-21 - Cambiado para devolver ProduccionListaResponseDto con JOINs</modified>
    /// </summary>
    public async Task<ProduccionListaResponseDto?> GetProduccionByGuidAsync(string guidRegistro)
    {
        return await _produccionRepository.GetByGuidWithDetailsAsync(guidRegistro);
    }

    /// <summary>
    /// Obtiene las producciones de una sede especifica
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesBySedeAsync(int idSede)
    {
        var producciones = await _produccionRepository.GetBySedeAsync(idSede);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene las producciones de una entidad medica especifica
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByEntidadMedicaAsync(int idEntidadMedica)
    {
        var producciones = await _produccionRepository.GetByEntidadMedicaAsync(idEntidadMedica);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene las producciones de un periodo especifico
    /// </summary>
    public async Task<IEnumerable<ProduccionResponseDto>> GetProduccionesByPeriodoAsync(string periodo)
    {
        var producciones = await _produccionRepository.GetByPeriodoAsync(periodo);
        return producciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva produccion en el sistema
    /// </summary>
    public async Task<ProduccionResponseDto> CreateProduccionAsync(CreateProduccionDto createDto, int idCreador)
    {
        var produccion = new Produccion
        {
            IdSede = createDto.IdSede,
            IdEntidadMedica = createDto.IdEntidadMedica,
            CodigoProduccion = createDto.CodigoProduccion,
            TipoProduccion = createDto.TipoProduccion,
            TipoMedico = createDto.TipoMedico,
            TipoRubro = createDto.TipoRubro,
            Descripcion = createDto.Descripcion,
            Periodo = createDto.Periodo,
            EstadoProduccion = createDto.EstadoProduccion,
            MtoConsumo = createDto.MtoConsumo,
            MtoDescuento = createDto.MtoDescuento,
            MtoSubtotal = createDto.MtoSubtotal,
            MtoRenta = createDto.MtoRenta,
            MtoIgv = createDto.MtoIgv,
            MtoTotal = createDto.MtoTotal,
            TipoComprobante = createDto.TipoComprobante,
            Serie = createDto.Serie,
            Numero = createDto.Numero,
            FechaEmision = createDto.FechaEmision,
            Glosa = createDto.Glosa,
            EstadoComprobante = createDto.EstadoComprobante,
            Concepto = createDto.Concepto,
            FechaLimite = createDto.FechaLimite,
            Estado = createDto.Estado,
            IdCreador = idCreador,
            Activo = 1
        };

        var idProduccion = await _produccionRepository.CreateAsync(produccion);
        var createdProduccion = await _produccionRepository.GetByIdAsync(idProduccion);

        return MapToResponseDto(createdProduccion!);
    }

    /// <summary>
    /// Actualiza los datos de una produccion existente
    /// </summary>
    public async Task<bool> UpdateProduccionAsync(int id, UpdateProduccionDto updateDto, int idModificador)
    {
        var produccionExistente = await _produccionRepository.GetByIdAsync(id);
        if (produccionExistente == null)
            return false;

        if (updateDto.IdSede.HasValue)
            produccionExistente.IdSede = updateDto.IdSede.Value;

        if (updateDto.IdEntidadMedica.HasValue)
            produccionExistente.IdEntidadMedica = updateDto.IdEntidadMedica.Value;

        if (updateDto.CodigoProduccion != null)
            produccionExistente.CodigoProduccion = updateDto.CodigoProduccion;

        if (updateDto.TipoProduccion != null)
            produccionExistente.TipoProduccion = updateDto.TipoProduccion;

        if (updateDto.TipoMedico != null)
            produccionExistente.TipoMedico = updateDto.TipoMedico;

        if (updateDto.TipoRubro != null)
            produccionExistente.TipoRubro = updateDto.TipoRubro;

        if (updateDto.Descripcion != null)
            produccionExistente.Descripcion = updateDto.Descripcion;

        if (updateDto.Periodo != null)
            produccionExistente.Periodo = updateDto.Periodo;

        if (updateDto.EstadoProduccion != null)
            produccionExistente.EstadoProduccion = updateDto.EstadoProduccion;

        if (updateDto.MtoConsumo.HasValue)
            produccionExistente.MtoConsumo = updateDto.MtoConsumo;

        if (updateDto.MtoDescuento.HasValue)
            produccionExistente.MtoDescuento = updateDto.MtoDescuento;

        if (updateDto.MtoSubtotal.HasValue)
            produccionExistente.MtoSubtotal = updateDto.MtoSubtotal;

        if (updateDto.MtoRenta.HasValue)
            produccionExistente.MtoRenta = updateDto.MtoRenta;

        if (updateDto.MtoIgv.HasValue)
            produccionExistente.MtoIgv = updateDto.MtoIgv;

        if (updateDto.MtoTotal.HasValue)
            produccionExistente.MtoTotal = updateDto.MtoTotal;

        if (updateDto.TipoComprobante != null)
            produccionExistente.TipoComprobante = updateDto.TipoComprobante;

        if (updateDto.Serie != null)
            produccionExistente.Serie = updateDto.Serie;

        if (updateDto.Numero != null)
            produccionExistente.Numero = updateDto.Numero;

        if (updateDto.FechaEmision.HasValue)
            produccionExistente.FechaEmision = updateDto.FechaEmision;

        if (updateDto.Glosa != null)
            produccionExistente.Glosa = updateDto.Glosa;

        if (updateDto.EstadoComprobante != null)
            produccionExistente.EstadoComprobante = updateDto.EstadoComprobante;

        if (updateDto.Concepto != null)
            produccionExistente.Concepto = updateDto.Concepto;

        if (updateDto.FechaLimite.HasValue)
            produccionExistente.FechaLimite = updateDto.FechaLimite;

        if (updateDto.Estado != null)
            produccionExistente.Estado = updateDto.Estado;

        if (updateDto.Activo.HasValue)
            produccionExistente.Activo = updateDto.Activo.Value;

        produccionExistente.IdModificador = idModificador;

        return await _produccionRepository.UpdateAsync(id, produccionExistente);
    }

    /// <summary>
    /// Elimina una produccion por su identificador
    /// </summary>
    public async Task<bool> DeleteProduccionAsync(int id, int idModificador)
    {
        var exists = await _produccionRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _produccionRepository.DeleteAsync(id, idModificador);
    }

    /// <summary>
    /// Obtiene el listado paginado de producciones con datos relacionados y filtro por estado.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// </summary>
    public async Task<(IEnumerable<ProduccionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        string? estado, int pageNumber, int pageSize)
    {
        return await _produccionRepository.GetPaginatedListAsync(estado, pageNumber, pageSize);
    }

    /// <summary>
    /// Solicita factura actualizando la fecha limite y cambiando el estado a FACTURA_ENVIADA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-21</created>
    /// </summary>
    public async Task<bool> SolicitarFacturaAsync(SolicitarFacturaDto solicitudDto, int idModificador)
    {
        // Combinar fecha y hora en un DateTime
        if (!DateTime.TryParse($"{solicitudDto.Fecha} {solicitudDto.Hora}", out DateTime fechaLimite))
        {
            return false;
        }

        const string nuevoEstado = "FACTURA_ENVIADA";

        return await _produccionRepository.UpdateFechaLimiteEstadoAsync(
            solicitudDto.GuidRegistro,
            fechaLimite,
            nuevoEstado,
            idModificador);
    }

    private static ProduccionResponseDto MapToResponseDto(Produccion produccion)
    {
        return new ProduccionResponseDto
        {
            IdProduccion = produccion.IdProduccion,
            IdSede = produccion.IdSede,
            IdEntidadMedica = produccion.IdEntidadMedica,
            CodigoProduccion = produccion.CodigoProduccion,
            TipoProduccion = produccion.TipoProduccion,
            TipoMedico = produccion.TipoMedico,
            TipoRubro = produccion.TipoRubro,
            Descripcion = produccion.Descripcion,
            Periodo = produccion.Periodo,
            EstadoProduccion = produccion.EstadoProduccion,
            MtoConsumo = produccion.MtoConsumo,
            MtoDescuento = produccion.MtoDescuento,
            MtoSubtotal = produccion.MtoSubtotal,
            MtoRenta = produccion.MtoRenta,
            MtoIgv = produccion.MtoIgv,
            MtoTotal = produccion.MtoTotal,
            TipoComprobante = produccion.TipoComprobante,
            Serie = produccion.Serie,
            Numero = produccion.Numero,
            FechaEmision = produccion.FechaEmision,
            Glosa = produccion.Glosa,
            EstadoComprobante = produccion.EstadoComprobante,
            Concepto = produccion.Concepto,
            FechaLimite = produccion.FechaLimite,
            Estado = produccion.Estado,
            Activo = produccion.Activo,
            GuidRegistro = produccion.GuidRegistro,
            FechaCreacion = produccion.FechaCreacion,
            FechaModificacion = produccion.FechaModificacion
        };
    }
}
