using Microsoft.Extensions.Logging;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.OrdenPagoAprobacion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de aprobaciones de ordenes de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// <modified>ADG Antonio - 2026-02-15 - Notificacion por email al siguiente aprobador</modified>
/// </summary>
public class OrdenPagoAprobacionService : IOrdenPagoAprobacionService
{
    private readonly IOrdenPagoAprobacionRepository _repository;
    private readonly IOrdenPagoRepository _ordenPagoRepository;
    private readonly IPerfilAprobacionUsuarioRepository _perfilAprobacionUsuarioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrdenPagoAprobacionService> _logger;

    public OrdenPagoAprobacionService(
        IOrdenPagoAprobacionRepository repository,
        IOrdenPagoRepository ordenPagoRepository,
        IPerfilAprobacionUsuarioRepository perfilAprobacionUsuarioRepository,
        IUsuarioRepository usuarioRepository,
        IEmailService emailService,
        ILogger<OrdenPagoAprobacionService> logger)
    {
        _repository = repository;
        _ordenPagoRepository = ordenPagoRepository;
        _perfilAprobacionUsuarioRepository = perfilAprobacionUsuarioRepository;
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las aprobaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetAllActiveAsync()
    {
        var items = await _repository.GetAllActiveAsync();
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una aprobacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene una aprobacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto?> GetByGuidAsync(string guid)
    {
        var item = await _repository.GetByGuidAsync(guid);
        return item != null ? MapToResponseDto(item) : null;
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        var items = await _repository.GetByOrdenPagoIdAsync(idOrdenPago);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de un perfil de aprobacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion)
    {
        var items = await _repository.GetByPerfilAprobacionIdAsync(idPerfilAprobacion);
        return items.Select(MapToResponseDto);
    }

    /// <summary>
    /// Aprueba una orden de pago para el usuario actual.
    /// Valida que el usuario tenga el perfil correspondiente y que sea su turno.
    /// Si todos los niveles estan aprobados, cambia el estado de la orden a APROBADO.
    /// </summary>
    public async Task<(bool success, string message)> AprobarAsync(int idOrdenPago, int idUsuario)
    {
        // Buscar la aprobacion pendiente que corresponde al usuario
        var aprobacion = await _repository.GetPendingByOrdenPagoForUserAsync(idOrdenPago, idUsuario);
        if (aprobacion == null)
            return (false, "No tiene permisos para aprobar esta orden o ya fue procesada.");

        // Aprobar el nivel actual
        var aprobado = await _repository.AprobarAsync(aprobacion.IdOrdenPagoAprobacion, idUsuario, idUsuario);
        if (!aprobado)
            return (false, "No se pudo procesar la aprobación. La orden ya fue procesada por otro usuario.");

        // Verificar si quedan niveles pendientes
        var aprobaciones = await _repository.GetByOrdenPagoIdAsync(idOrdenPago);
        var pendientesList = aprobaciones
            .Where(a => a.Estado == EstadoDescripcion.Aprobacion.Pendiente)
            .OrderBy(a => a.Orden)
            .ToList();

        if (!pendientesList.Any())
        {
            // Todos los niveles aprobados: actualizar estado de la orden
            await _ordenPagoRepository.UpdateEstadoAsync(idOrdenPago, EstadoDescripcion.OrdenPago.Aprobado, idUsuario);
            return (true, "Orden de pago aprobada exitosamente. Todos los niveles han sido completados.");
        }

        // Notificar al siguiente nivel de aprobacion
        var siguienteAprobacion = pendientesList.First();
        await NotificarSiguienteAprobadorAsync(idOrdenPago, siguienteAprobacion);

        return (true, "Nivel aprobado exitosamente. Se notifico al siguiente aprobador.");
    }

    /// <summary>
    /// Notifica por email a los usuarios del siguiente perfil de aprobacion.
    /// Filtra por sede de la orden de pago.
    /// </summary>
    private async Task NotificarSiguienteAprobadorAsync(int idOrdenPago, OrdenPagoAprobacion siguienteAprobacion)
    {
        try
        {
            // Obtener datos de la orden de pago
            var ordenPago = await _ordenPagoRepository.GetByIdAsync(idOrdenPago);
            if (ordenPago == null) return;

            // Obtener usuarios asignados al perfil del siguiente nivel
            var usuariosPerfil = await _perfilAprobacionUsuarioRepository
                .GetByPerfilAprobacionIdAsync(siguienteAprobacion.IdPerfilAprobacion);

            // Filtrar por sede: usuarios sin sede asignada (aplica a todas) o con la sede de la orden
            var usuariosFiltrados = usuariosPerfil
                .Where(up => up.IdSede == null || up.IdSede == ordenPago.IdSede)
                .ToList();

            foreach (var usuarioPerfil in usuariosFiltrados)
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioPerfil.IdUsuario);
                if (usuario == null || string.IsNullOrEmpty(usuario.Email)) continue;

                var nombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}".Trim();

                await _emailService.EnviarEmailNotificacionAprobacionAsync(
                    usuario.Email,
                    nombreCompleto,
                    ordenPago.NumeroOrdenPago ?? "-",
                    ordenPago.FechaGeneracion,
                    ordenPago.MtoTotalAcum,
                    siguienteAprobacion.NombrePerfil ?? "-",
                    idOrdenPago);
            }
        }
        catch (Exception ex)
        {
            // No fallar la aprobacion si el envio de email falla
            _logger.LogError(ex, "Error al notificar al siguiente aprobador para la orden de pago {IdOrdenPago}", idOrdenPago);
        }
    }

    /// <summary>
    /// Rechaza una orden de pago para el usuario actual.
    /// Cambia el estado de la orden a RECHAZADO y registra el comentario.
    /// </summary>
    public async Task<(bool success, string message)> RechazarAsync(int idOrdenPago, int idUsuario, string? comentario)
    {
        // Buscar la aprobacion pendiente que corresponde al usuario
        var aprobacion = await _repository.GetPendingByOrdenPagoForUserAsync(idOrdenPago, idUsuario);
        if (aprobacion == null)
            return (false, "No tiene permisos para rechazar esta orden o ya fue procesada.");

        // Rechazar el nivel actual
        var rechazado = await _repository.RechazarAsync(aprobacion.IdOrdenPagoAprobacion, idUsuario, idUsuario);
        if (!rechazado)
            return (false, "No se pudo procesar el rechazo. La orden ya fue procesada por otro usuario.");

        // Actualizar estado de la orden a DEVUELTO
        var orden = await _ordenPagoRepository.GetByIdAsync(idOrdenPago);
        if (orden != null)
        {
            orden.Estado = EstadoDescripcion.OrdenPago.Devuelto;
            orden.Comentarios = comentario;
            orden.IdModificador = idUsuario;
            await _ordenPagoRepository.UpdateAsync(orden);
        }

        return (true, "Orden de pago rechazada.");
    }

    /// <summary>
    /// Crea una nueva aprobacion de orden de pago.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto> CreateAsync(CreateOrdenPagoAprobacionDto dto, int idCreador)
    {
        var entity = new OrdenPagoAprobacion
        {
            IdOrdenPago = dto.IdOrdenPago,
            IdPerfilAprobacion = dto.IdPerfilAprobacion,
            Orden = dto.Orden,
            Estado = dto.Estado ?? EstadoDescripcion.Aprobacion.Pendiente,
            IdCreador = idCreador,
            Activo = 1
        };

        var id = await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(id);

        return MapToResponseDto(created!);
    }

    /// <summary>
    /// Actualiza una aprobacion existente.
    /// </summary>
    public async Task<OrdenPagoAprobacionResponseDto?> UpdateAsync(UpdateOrdenPagoAprobacionDto dto, int idModificador)
    {
        var existing = await _repository.GetByIdAsync(dto.IdOrdenPagoAprobacion);
        if (existing == null)
            return null;

        existing.IdOrdenPago = dto.IdOrdenPago;
        existing.IdPerfilAprobacion = dto.IdPerfilAprobacion;
        existing.Orden = dto.Orden;
        existing.Estado = dto.Estado;
        existing.FechaAprobacion = dto.FechaAprobacion;
        existing.IdUsuarioAprobador = dto.IdUsuarioAprobador;
        existing.IdModificador = idModificador;

        var updated = await _repository.UpdateAsync(existing);
        if (!updated)
            return null;

        var result = await _repository.GetByIdAsync(dto.IdOrdenPagoAprobacion);
        return MapToResponseDto(result!);
    }

    /// <summary>
    /// Elimina logicamente una aprobacion por su GUID.
    /// </summary>
    public async Task<bool> DeleteAsync(string guid, int idModificador)
    {
        var item = await _repository.GetByGuidAsync(guid);
        if (item == null)
            return false;

        return await _repository.DeleteAsync(item.IdOrdenPagoAprobacion, idModificador);
    }

    /// <summary>
    /// Elimina logicamente todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        return await _repository.DeleteByOrdenPagoIdAsync(idOrdenPago, idModificador);
    }

    private static OrdenPagoAprobacionResponseDto MapToResponseDto(OrdenPagoAprobacion entity)
    {
        return new OrdenPagoAprobacionResponseDto
        {
            IdOrdenPagoAprobacion = entity.IdOrdenPagoAprobacion,
            IdOrdenPago = entity.IdOrdenPago,
            IdPerfilAprobacion = entity.IdPerfilAprobacion,
            Orden = entity.Orden,
            Estado = entity.Estado,
            FechaAprobacion = entity.FechaAprobacion,
            IdUsuarioAprobador = entity.IdUsuarioAprobador,
            NumeroOrdenPago = entity.NumeroOrdenPago,
            NombrePerfil = entity.NombrePerfil,
            NombreAprobador = entity.NombreAprobador,
            GuidRegistro = entity.GuidRegistro,
            Activo = entity.Activo,
            FechaCreacion = entity.FechaCreacion,
            FechaModificacion = entity.FechaModificacion
        };
    }
}
