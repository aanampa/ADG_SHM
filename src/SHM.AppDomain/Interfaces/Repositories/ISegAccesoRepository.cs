using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Contrato para el repositorio de log de accesos al sistema.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-25</created>
/// </summary>
public interface ISegAccesoRepository
{
    Task<int> RegistrarAsync(SegAcceso acceso);

    /// <summary>
    /// Retorna la fecha del ultimo LOGIN_OK para cada usuario solicitado.
    /// Clave: IdUsuario, Valor: fecha del ultimo acceso exitoso.
    /// </summary>
    Task<Dictionary<int, DateTime>> GetUltimosAccesosAsync(IEnumerable<int> idUsuarios);
}
