namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa la aprobacion de una orden de pago por rol.
/// Mapea a la tabla SHM_ORDEN_PAGO_APROBACION en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoAprobacion
{
    public int IdOrdenPagoAprobacion { get; set; }
    public int IdOrdenPago { get; set; }
    public int IdRol { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // Propiedades de navegacion
    public string? NumeroOrdenPago { get; set; }
    public string? NombreRol { get; set; }
    public string? NombreAprobador { get; set; }
}
