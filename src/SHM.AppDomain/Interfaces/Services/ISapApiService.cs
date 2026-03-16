using SHM.AppDomain.DTOs.SapApi;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de integracion con SAP OData API.
/// </summary>
/// <author>ADG Antonio</author>
/// <created>2026-03-04</created>
public interface ISapApiService
{
    /// <summary>
    /// Obtiene un token de acceso OAuth2 de SAP.
    /// </summary>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// Obtiene la lista de bancos desde SAP (COD_BANCOSet).
    /// </summary>
    Task<List<SapBancoDto>> GetBancosAsync();

    /// <summary>
    /// Obtiene las cuentas bancarias de un acreedor desde SAP (CTA_ACREEDORSet).
    /// </summary>
    /// <param name="codAcreedor">Codigo del acreedor en SAP.</param>
    Task<List<SapAcreedorCuentaBancariaDto>> GetCuentasBancariasByAcreedorAsync(string codAcreedor);
}
