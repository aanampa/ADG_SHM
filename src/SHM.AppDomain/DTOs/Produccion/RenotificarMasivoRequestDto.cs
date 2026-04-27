namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para la renotificacion masiva de solicitudes de factura.
/// La fecha y hora son opcionales: si se proporcionan se actualiza la fecha limite antes de renotificar.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-21</created>
/// </summary>
public class RenotificarMasivoRequestDto
{
    /// <summary>
    /// Lista de GUIDs de producciones a renotificar.
    /// </summary>
    public List<string> Guids { get; set; } = new();

    /// <summary>
    /// Nueva fecha limite opcional (formato: yyyy-MM-dd). Si es null o vacía, se mantiene la fecha actual.
    /// </summary>
    public string? Fecha { get; set; }

    /// <summary>
    /// Nueva hora limite opcional (formato: HH:mm). Si es null o vacía, se mantiene la hora actual.
    /// </summary>
    public string? Hora { get; set; }

    /// <summary>
    /// Nueva fecha de vencimiento de pago (formato: yyyy-MM-dd).
    /// Solo aplica cuando ActualizaFecha es true.
    /// </summary>
    public string? FechaVencimiento { get; set; }

    /// <summary>
    /// Indica si se debe actualizar la fecha limite antes de renotificar.
    /// </summary>
    public bool ActualizaFecha => !string.IsNullOrEmpty(Fecha) && !string.IsNullOrEmpty(Hora);
}
