using SHM.AppDomain.DTOs.SanPabloApi;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para comunicarse con el API externo de San Pablo.
/// Permite obtener tokens de autenticacion y consultar datos de entidades medicas.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public interface ISanPabloApiService
{
    /// <summary>
    /// Obtiene un token de autenticacion del API de San Pablo.
    /// </summary>
    /// <returns>Token JWT si la autenticacion es exitosa, null en caso contrario.</returns>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// Obtiene los datos de una entidad medica desde el API de San Pablo.
    /// </summary>
    /// <param name="codigoSede">Codigo de la sede.</param>
    /// <param name="tipoEntidad">Tipo de entidad medica (C=Compania, M=Medico).</param>
    /// <param name="codigoEntidad">Codigo de la entidad medica.</param>
    /// <returns>Datos de la entidad medica si existe, null en caso contrario.</returns>
    Task<SanPabloEntidadMedicaDto?> GetEntidadMedicaAsync(string codigoSede, string tipoEntidad, string codigoEntidad);
}
