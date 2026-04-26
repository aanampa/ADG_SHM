namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Contrato para el servicio de log de accesos al sistema.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-25</created>
/// </summary>
public interface ISegAccesoService
{
    Task RegistrarAsync(
        string tipoEvento,
        string loginIntento,
        string ipAcceso,
        string? userAgent,
        int?   idUsuario,
        string? detalle = null);
}
