namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// Configuracion para el API externo de San Pablo.
/// Se mapea desde appsettings.json seccion "SanPabloApi".
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public class SanPabloApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string EndpointLogin { get; set; } = "/api/Usuario/Login";
    public string EndpointObtenerEntidad { get; set; } = "/api/HHMM/v1/ObtenerEntidad";
    public string EndpointObtenerSede { get; set; } = "/api/HHMM/v1/ObtenerSede";
    public string EndpointRegistrarComprobante { get; set; } = "/api/HHMM/v1/RegistrarComprobante";
}
