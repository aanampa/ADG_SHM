namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para grabar fechas límite y vencimiento en producciones FACTURA_PENDIENTE
/// sin cambiar el estado. Paso 1 del flujo de Solicitud de Factura.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-26</created>
/// </summary>
public class GrabarFechasProduccionDto
{
    /// <summary>
    /// Lista de GUIDs de producciones a actualizar.
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
    /// Fecha de vencimiento de pago que figurará en la factura (formato: yyyy-MM-dd).
    /// </summary>
    public string FechaVencimiento { get; set; } = string.Empty;
}
