namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para la respuesta de login del API de San Pablo.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public class SanPabloLoginResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
}
