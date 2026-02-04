namespace SHM.AppDomain.DTOs.OrdenPagoAprobacion;

/// <summary>
/// DTO de respuesta para la aprobacion de orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoAprobacionResponseDto
{
    public int IdOrdenPagoAprobacion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdRol { get; set; }
    public string? NumeroOrdenPago { get; set; }
    public string? NombreRol { get; set; }
    public string? NombreAprobador { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
