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
/// <modified>ADG Antonio - 2026-04-23 - Notificacion a Tesoreria cuando todos los niveles son aprobados</modified>
/// </summary>
public class OrdenPagoAprobacionService : IOrdenPagoAprobacionService
{
    private readonly IOrdenPagoAprobacionRepository _repository;
    private readonly IOrdenPagoRepository _ordenPagoRepository;
    private readonly IPerfilAprobacionUsuarioRepository _perfilAprobacionUsuarioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly IParametroRepository _parametroRepository;
    private readonly ILogger<OrdenPagoAprobacionService> _logger;

    public OrdenPagoAprobacionService(
        IOrdenPagoAprobacionRepository repository,
        IOrdenPagoRepository ordenPagoRepository,
        IPerfilAprobacionUsuarioRepository perfilAprobacionUsuarioRepository,
        IUsuarioRepository usuarioRepository,
        IEmailService emailService,
        IParametroRepository parametroRepository,
        ILogger<OrdenPagoAprobacionService> logger)
    {
        _repository = repository;
        _ordenPagoRepository = ordenPagoRepository;
        _perfilAprobacionUsuarioRepository = perfilAprobacionUsuarioRepository;
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
        _parametroRepository = parametroRepository;
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
            // Todos los niveles aprobados: actualizar estado de la orden y notificar a Tesoreria
            await _ordenPagoRepository.UpdateEstadoAsync(idOrdenPago, EstadoDescripcion.OrdenPago.Aprobado, idUsuario);
            await NotificarTesoreriaAsync(idOrdenPago, aprobaciones.ToList());
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
    /// Notifica por email al creador de la orden de pago que fue rechazada.
    /// </summary>
    private async Task NotificarCreadorRechazoAsync(int idOrdenPago, OrdenPagoAprobacion aprobacion, string? comentario)
    {
        try
        {
            var ordenPago = await _ordenPagoRepository.GetByIdAsync(idOrdenPago);
            if (ordenPago == null) return;

            var creador = await _usuarioRepository.GetByIdAsync(ordenPago.IdCreador);
            if (creador == null || string.IsNullOrEmpty(creador.Email)) return;

            var nombreCreador = $"{creador.Nombres} {creador.ApellidoPaterno} {creador.ApellidoMaterno}".Trim();

            await _emailService.EnviarEmailNotificacionRechazoAsync(
                creador.Email,
                nombreCreador,
                ordenPago.NumeroOrdenPago ?? "-",
                ordenPago.FechaGeneracion,
                ordenPago.MtoTotalAcum,
                aprobacion.NombrePerfil ?? "-",
                aprobacion.NombreAprobador ?? "-",
                comentario,
                idOrdenPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar al creador del rechazo para la orden de pago {IdOrdenPago}", idOrdenPago);
        }
    }

    /// <summary>
    /// Notifica al area de Tesoreria que una orden de pago ha sido completamente aprobada.
    /// Lee los destinatarios desde el parametro TESORERIA_EMAILS (separados por punto y coma).
    /// </summary>
    private async Task NotificarTesoreriaAsync(int idOrdenPago, List<OrdenPagoAprobacion> aprobaciones)
    {
        try
        {
            var parametro = await _parametroRepository.GetByCodigoAsync("TESORERIA_EMAILS");
            if (parametro == null || string.IsNullOrWhiteSpace(parametro.Valor))
            {
                _logger.LogWarning("Parametro TESORERIA_EMAILS no encontrado o vacio. No se enviara notificacion a Tesoreria.");
                return;
            }

            var ordenPago = await _ordenPagoRepository.GetByIdAsync(idOrdenPago);
            if (ordenPago == null) return;

            // Construir filas HTML del historial de aprobaciones
            var aprobadas = aprobaciones
                .Where(a => a.Estado == EstadoDescripcion.Aprobacion.Aprobado && a.FechaAprobacion.HasValue)
                .OrderBy(a => a.Orden)
                .ToList();

            var filasHtml = string.Join("", aprobadas.Select((a, i) =>
            {
                var bgFila = i % 2 == 0 ? "#f9f9f9" : "#ffffff";
                return $"""
                    <tr style="background-color: {bgFila};">
                        <td style="padding: 9px 12px; border-bottom: 1px solid #eeeeee;">{System.Net.WebUtility.HtmlEncode(a.NombrePerfil ?? "-")}</td>
                        <td style="padding: 9px 12px; border-bottom: 1px solid #eeeeee;">{System.Net.WebUtility.HtmlEncode(a.NombreAprobador ?? "-")}</td>
                        <td style="padding: 9px 12px; border-bottom: 1px solid #eeeeee; text-align: center;">{a.FechaAprobacion?.ToString("dd/MM/yyyy HH:mm") ?? "-"}</td>
                    </tr>
                    """;
            }));

            var emails = parametro.Valor
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            foreach (var email in emails)
            {
                await _emailService.EnviarEmailNotificacionTesoreriaAsync(
                    email,
                    ordenPago.NumeroOrdenPago ?? "-",
                    ordenPago.NombreSede ?? "-",
                    ordenPago.NombreBanco ?? "-",
                    ordenPago.FechaGeneracion,
                    ordenPago.MtoTotalAcum,
                    filasHtml,
                    idOrdenPago);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar a Tesoreria para la orden de pago {IdOrdenPago}", idOrdenPago);
        }
    }

    /// <summary>
    /// Notifica por email a los usuarios del primer nivel de aprobacion pendiente.
    /// Se invoca al generar una nueva Orden de Pago.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-17</created>
    /// </summary>
    public async Task NotificarPrimerAprobadorAsync(int idOrdenPago)
    {
        var aprobaciones = await _repository.GetByOrdenPagoIdAsync(idOrdenPago);
        var primerPendiente = aprobaciones
            .Where(a => a.Estado == EstadoDescripcion.Aprobacion.Pendiente)
            .OrderBy(a => a.Orden)
            .FirstOrDefault();

        if (primerPendiente != null)
        {
            await NotificarSiguienteAprobadorAsync(idOrdenPago, primerPendiente);
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

        await NotificarCreadorRechazoAsync(idOrdenPago, aprobacion, comentario);

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
