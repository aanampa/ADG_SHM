namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para la solicitud masiva de facturas.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-03</created>
/// </summary>
public class SolicitudMasivaRequestDto
{
    /// <summary>
    /// Lista de GUIDs de producciones a procesar.
    /// </summary>
    public List<string> Guids { get; set; } = new();

    /// <summary>
    /// Fecha límite para entrega de facturas (formato: yyyy-MM-dd).
    /// </summary>
    public string Fecha { get; set; } = string.Empty;

    /// <summary>
    /// Hora límite para entrega de facturas (formato: HH:mm).
    /// </summary>
    public string Hora { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vencimiento de pago que debe figurar en la factura (formato: yyyy-MM-dd).
    /// </summary>
    public string FechaVencimiento { get; set; } = string.Empty;
}
