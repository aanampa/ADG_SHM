namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// Configuracion para el servicio de integracion con SAP OData API.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-04</created>
/// <modified>ADG Antonio - 2026-04-15 - Agregado EndpointDatosFactura</modified>
/// </summary>
public class SapApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string EndpointToken { get; set; } = "/sap/bc/sec/oauth2/token";
    public string EndpointBancos { get; set; } = "/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/COD_BANCOSet";
    public string EndpointCuentasAcreedor { get; set; } = "/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/CTA_ACREEDORSet";
    public string EndpointDatosFactura { get; set; } = "/sap/opu/odata/sap/ZODGS_PORTAL_HHMM_SRV/DatosFacturaSet";
}
