namespace SHM.AppDomain.DTOs.OrdenPagoAprobacion;

/// <summary>
/// DTO para la creacion de una aprobacion de orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class CreateOrdenPagoAprobacionDto
{
    public int IdOrdenPago { get; set; }
    public int IdRol { get; set; }
}
