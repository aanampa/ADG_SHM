namespace SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

/// <summary>
/// DTO para la actualizacion de una relacion orden de pago - liquidacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class UpdateOrdenPagoLiquidacionDto
{
    public int IdOrdenPagoLiquidacion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdProduccion { get; set; }
}
