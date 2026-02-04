namespace SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

/// <summary>
/// DTO para la creacion de una relacion orden de pago - liquidacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class CreateOrdenPagoLiquidacionDto
{
    public int IdOrdenPago { get; set; }
    public int IdProduccion { get; set; }
}
