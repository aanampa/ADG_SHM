using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO para banco del endpoint COD_BANCOSet de SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-04</created>
/// </summary>
public class SapBancoDto
{
    [JsonPropertyName("CodigoBanco")]
    public string? CodigoBanco { get; set; }

    [JsonPropertyName("DescripcionBanco")]
    public string? DescripcionBanco { get; set; }
}
