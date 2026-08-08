namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO de respuesta del endpoint estado-pago-orden.
/// Contiene el numero de orden y el detalle de estado de pago por comprobante.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-15</created>
/// </summary>
public class SapEstadoPagoOrdenDto
{
    public string? GuidOrdenPago { get; set; }
    public int TotalComprobantes { get; set; }
    public int ConsultadosEnSap { get; set; }
    public int SinComprobante { get; set; }
    public int Errores { get; set; }
    public List<SapEstadoPagoItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO con el estado de pago de un comprobante individual de la orden de pago.
/// </summary>
public class SapEstadoPagoItemDto
{
    // Datos del comprobante en el sistema
    public int IdProduccion { get; set; }
    public string? GuidProduccion { get; set; }
    public string? TipoComprobante { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }

    // Estado de la consulta a SAP
    public string Estado { get; set; } = string.Empty;
    public string? Mensaje { get; set; }

    // Datos obtenidos desde SAP (null si no se pudo consultar)
    public string? EstadoPago { get; set; }
    public string? FechaPago { get; set; }
    public string? MontoPagado { get; set; }
    public string? NumeroOperacion { get; set; }
    public string? Banco { get; set; }
    public string? CtaBanDeposito { get; set; }
}

/// <summary>
/// DTO de respuesta del endpoint actualizar-estado-pago-masivo.
/// Contiene el resumen global y el detalle agrupado por orden de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-07-18</created>
/// </summary>
public class SapActualizarEstadoPagoMasivoDto
{
    public int TotalOrdenes { get; set; }
    public int TotalComprobantes { get; set; }
    public int ConsultadosEnSap { get; set; }
    public int Actualizados { get; set; }
    public int SinComprobante { get; set; }
    public int Errores { get; set; }
    public List<SapEstadoPagoOrdenDto> Ordenes { get; set; } = new();
}
