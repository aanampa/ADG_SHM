using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO generico para respuestas OData de SAP que devuelven un objeto unico.
/// Formato: { "d": { ...campos... } }
/// A diferencia de SapODataResponseDto que usa "d": { "results": [...] }.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-15</created>
/// </summary>
public class SapODataSingleResponseDto<T>
{
    [JsonPropertyName("d")]
    public T? D { get; set; }
}
