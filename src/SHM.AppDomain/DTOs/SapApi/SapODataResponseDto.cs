using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO generico para respuestas OData de SAP.
/// Formato: { "d": { "results": [...] } }
///
/// <author>ADG Antonio</author>
/// <created>2026-03-04</created>
/// </summary>
public class SapODataResponseDto<T>
{
    [JsonPropertyName("d")]
    public SapODataResultsDto<T>? D { get; set; }
}

public class SapODataResultsDto<T>
{
    [JsonPropertyName("results")]
    public List<T>? Results { get; set; }
}
