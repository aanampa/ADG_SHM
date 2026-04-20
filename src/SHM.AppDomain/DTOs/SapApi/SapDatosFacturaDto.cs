using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO con los datos de una factura obtenidos desde SAP (DatosFacturaSet).
/// Incluye el estado de pago y la informacion bancaria del deposito.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-15</created>
/// </summary>
public class SapDatosFacturaDto
{
    [JsonPropertyName("CodigoAcreedor")]
    public string? CodigoAcreedor { get; set; }

    [JsonPropertyName("TipoComprobante")]
    public string? TipoComprobante { get; set; }

    [JsonPropertyName("Serie")]
    public string? Serie { get; set; }

    [JsonPropertyName("NumeroComprobante")]
    public string? NumeroComprobante { get; set; }

    [JsonPropertyName("Anio")]
    public string? Anio { get; set; }

    [JsonPropertyName("EstadoPago")]
    public string? EstadoPago { get; set; }

    [JsonPropertyName("FechaPago")]
    public string? FechaPago { get; set; }

    [JsonPropertyName("NumeroOperacion")]
    public string? NumeroOperacion { get; set; }

    [JsonPropertyName("Banco")]
    public string? Banco { get; set; }

    [JsonPropertyName("CtaBanDeposito")]
    public string? CtaBanDeposito { get; set; }

    [JsonPropertyName("MontoPagado")]
    public string? MontoPagado { get; set; }
}
