namespace SHM.AppDomain.DTOs.OrdenPagoProduccion;

/// <summary>
/// DTO para la actualizacion de una relacion orden de pago - produccion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class UpdateOrdenPagoProduccionDto
{
    public int IdOrdenPagoProduccion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdProduccion { get; set; }
}
