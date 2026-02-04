namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa una orden de pago.
/// Mapea a la tabla SHM_ORDEN_PAGO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPago
{
    public int IdOrdenPago { get; set; }
    public int IdBanco { get; set; }
    public string? NumeroOrdenPago { get; set; }
    public DateTime? FechaGeneracion { get; set; }
    public string? Estado { get; set; }
    public decimal? MtoConsumoAcum { get; set; }
    public decimal? MtoDescuentoAcum { get; set; }
    public decimal? MtoSubtotalAcum { get; set; }
    public decimal? MtoRentaAcum { get; set; }
    public decimal? MtoIgvAcum { get; set; }
    public decimal? MtoTotalAcum { get; set; }
    public int? CantComprobantes { get; set; }
    public int? CantLiquidaciones { get; set; }
    public string? Comentarios { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // Propiedades de navegacion
    public string? NombreBanco { get; set; }
}
