using SHM.AppDomain.DTOs.SanPabloApi;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para comunicarse con el API externo de San Pablo.
/// Permite obtener tokens de autenticacion y consultar datos de entidades medicas y sedes.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// <modified>ADG Antonio - 2026-02-24 - Agregado metodo GetSedeAsync</modified>
/// <modified>ADG Antonio - 2026-02-25 - Agregado metodo RegistrarComprobanteAsync</modified>
/// <modified>ADG Antonio - 2026-03-21 - Agregado metodo CheckConnectionAsync</modified>
/// </summary>
public interface ISanPabloApiService
{
    /// <summary>
    /// Obtiene un token de autenticacion del API de San Pablo.
    /// </summary>
    /// <returns>Token JWT si la autenticacion es exitosa, null en caso contrario.</returns>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// Verifica la conectividad con el API de San Pablo intentando obtener un token de acceso.
    /// </summary>
    Task<(bool Ok, string Mensaje)> CheckConnectionAsync();

    /// <summary>
    /// Obtiene los datos de una entidad medica desde el API de San Pablo.
    /// </summary>
    /// <param name="codigoSede">Codigo de la sede.</param>
    /// <param name="tipoEntidad">Tipo de entidad medica (C=Compania, M=Medico).</param>
    /// <param name="codigoEntidad">Codigo de la entidad medica.</param>
    /// <returns>Datos de la entidad medica si existe, null en caso contrario.</returns>
    Task<SanPabloEntidadMedicaDto?> GetEntidadMedicaAsync(string codigoSede, string tipoEntidad, string codigoEntidad);

    /// <summary>
    /// Obtiene los datos de una sede desde el API de San Pablo.
    /// </summary>
    /// <param name="codigo">Codigo de la sede.</param>
    /// <returns>Datos de la sede si existe, null en caso contrario.</returns>
    Task<SanPabloSedeDto?> GetSedeAsync(string codigo);

    /// <summary>
    /// Obtiene todas las sedes desde el API de San Pablo (Codigo=X).
    /// </summary>
    /// <returns>Lista de sedes, o lista vacia si ocurre un error.</returns>
    Task<List<SanPabloSedeDto>> GetAllSedesAsync();

    /// <summary>
    /// Registra un comprobante en el API de San Pablo.
    /// </summary>
    /// <param name="request">Datos del comprobante a registrar.</param>
    /// <returns>Respuesta del API con IsSuccess, Title y Message.</returns>
    Task<SanPabloComprobanteResponseDto> RegistrarComprobanteAsync(SanPabloComprobanteRequestDto request);
}
