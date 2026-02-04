namespace SHM.AppDomain.DTOs.OrdenPagoAprobacion;

/// <summary>
/// DTO para la actualizacion de una aprobacion de orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class UpdateOrdenPagoAprobacionDto
{
    public int IdOrdenPagoAprobacion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdRol { get; set; }
}
