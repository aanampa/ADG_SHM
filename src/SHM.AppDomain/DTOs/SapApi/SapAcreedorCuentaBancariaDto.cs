using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO para cuentas bancarias de acreedor del endpoint CTA_ACREEDORSet de SAP.
/// </summary>
/// <author>ADG Antonio</author>
/// <created>2026-03-15</created>
public class SapAcreedorCuentaBancariaDto
{
    [JsonPropertyName("CodAcreedor")]
    public string CodAcreedor { get; set; } = string.Empty;

    [JsonPropertyName("NroCuenta")]
    public string NroCuenta { get; set; } = string.Empty;

    [JsonPropertyName("NroCtaInterbancaria")]
    public string? NroCtaInterbancaria { get; set; }

    [JsonPropertyName("Moneda")]
    public string Moneda { get; set; } = string.Empty;

    [JsonPropertyName("CodigoBanco")]
    public string CodigoBanco { get; set; } = string.Empty;

    [JsonPropertyName("DescripcionBanco")]
    public string DescripcionBanco { get; set; } = string.Empty;
}
