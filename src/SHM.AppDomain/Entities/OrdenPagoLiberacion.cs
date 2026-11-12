namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa el resultado de la liberacion en SAP de un comprobante
/// de una orden de pago. Mapea a la tabla SHM_ORDEN_PAGO_LIBERACION.
/// Una fila por orden de pago y comprobante (produccion).
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public class OrdenPagoLiberacion
{
    public int IdOrdenPagoLiberacion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdProduccion { get; set; }
    public string? Sociedad { get; set; }
    public string? Documento { get; set; }
    public string? Ejercicio { get; set; }
    public string? DocumentoDt { get; set; }
    public string EstadoLiberacion { get; set; } = string.Empty;
    public int Intentos { get; set; }
    public DateTime? FechaLiberacion { get; set; }
    public string? MensajeSap { get; set; }
    public string? RespuestaSap { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
