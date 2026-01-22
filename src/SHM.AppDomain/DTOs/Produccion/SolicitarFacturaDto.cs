namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para solicitar factura con fecha limite.
///
/// <author>ADG Vladimir D</author>
/// <created>2025-01-21</created>
/// </summary>
public class SolicitarFacturaDto
{
    /// <summary>
    /// GUID del registro de produccion
    /// </summary>
    public string GuidRegistro { get; set; } = string.Empty;

    /// <summary>
    /// Fecha limite para entrega de factura (formato: yyyy-MM-dd)
    /// </summary>
    public string Fecha { get; set; } = string.Empty;

    /// <summary>
    /// Hora limite para entrega de factura (formato: HH:mm)
    /// </summary>
    public string Hora { get; set; } = string.Empty;
}
