namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para el modal de ReNotificar Factura (FACTURA_SOLICITADA / FACTURA_DEVUELTA).
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-26</created>
/// </summary>
public class ReNotificarModalRequestDto
{
    /// <summary>Lista de GUIDs de producciones a procesar.</summary>
    public List<string> Guids { get; set; } = new();

    /// <summary>"solo" = solo renotificar | "cambiar" = cambiar fechas y renotificar.</summary>
    public string Accion { get; set; } = "solo";

    /// <summary>Nueva Fecha Límite (yyyy-MM-dd). Null = mantener la actual.</summary>
    public string? Fecha { get; set; }

    /// <summary>Nueva Hora Límite (HH:mm). Null = mantener la actual.</summary>
    public string? Hora { get; set; }

    /// <summary>Nueva Fecha de Vencimiento (yyyy-MM-dd). Null = mantener la actual.</summary>
    public string? FechaVencimiento { get; set; }
}
