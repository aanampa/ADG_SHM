using System.Text.Json.Serialization;

namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para la respuesta de login del API de San Pablo.
/// Mapea los campos access_token y userInfo del API externo.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// <modified>ADG Antonio - 2026-02-02 - Actualizado segun formato real del API</modified>
/// </summary>
public class SanPabloLoginResponseDto
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("userInfo")]
    public SanPabloUserInfoDto? UserInfo { get; set; }
}

/// <summary>
/// DTO para la informacion del usuario en la respuesta de login.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public class SanPabloUserInfoDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("usuario")]
    public string? Usuario { get; set; }

    [JsonPropertyName("nombres")]
    public string? Nombres { get; set; }

    [JsonPropertyName("apellidos")]
    public string? Apellidos { get; set; }

    [JsonPropertyName("correo")]
    public string? Correo { get; set; }

    [JsonPropertyName("rol")]
    public string? Rol { get; set; }
}
