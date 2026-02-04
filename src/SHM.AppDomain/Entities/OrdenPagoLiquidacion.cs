namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa la relacion entre una orden de pago y una liquidacion (produccion).
/// Mapea a la tabla SHM_ORDEN_PAGO_LIQUIDACION en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoLiquidacion
{
    public int IdOrdenPagoLiquidacion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdProduccion { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // Propiedades de navegacion
    public string? NumeroOrdenPago { get; set; }
    public string? NumeroLiquidacion { get; set; }
}
