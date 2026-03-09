using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// DTO para la respuesta de autenticacion OAuth2 de SAP.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-04</created>
/// </summary>
public class SapTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
