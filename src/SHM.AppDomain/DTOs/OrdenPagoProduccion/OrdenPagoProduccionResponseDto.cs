namespace SHM.AppDomain.DTOs.OrdenPagoProduccion;

/// <summary>
/// DTO de respuesta para la relacion orden de pago - produccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoProduccionResponseDto
{
    public int IdOrdenPagoProduccion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdProduccion { get; set; }
    public string? NumeroOrdenPago { get; set; }
    public string? NumeroLiquidacion { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
