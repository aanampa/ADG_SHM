using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para registrar eventos de acceso al sistema (login, logout, sesion expirada).
///
/// <author>ADG Antonio</author>
/// <created>2026-04-25</created>
/// </summary>
public class SegAccesoService : ISegAccesoService
{
    private readonly ISegAccesoRepository _repository;
    private readonly ILogger<SegAccesoService> _logger;

    public SegAccesoService(ISegAccesoRepository repository, ILogger<SegAccesoService> logger)
    {
        _repository = repository;
        _logger     = logger;
    }

    /// <summary>
    /// Registra un evento de acceso. No lanza excepciones para no interrumpir el flujo de autenticacion.
    /// </summary>
    public async Task RegistrarAsync(
        string  tipoEvento,
        string  loginIntento,
        string  ipAcceso,
        string? userAgent,
        int?    idUsuario,
        string? detalle = null)
    {
        try
        {
            var acceso = new SegAcceso
            {
                TipoEvento   = tipoEvento,
                LoginIntento = loginIntento,
                IpAcceso     = ipAcceso,
                UserAgent    = userAgent,
                IdUsuario    = idUsuario,
                Detalle      = detalle,
                GuidRegistro = Guid.NewGuid().ToString()
            };

            await _repository.RegistrarAsync(acceso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar evento de acceso: Tipo={TipoEvento} Login={Login}", tipoEvento, loginIntento);
        }
    }
}
