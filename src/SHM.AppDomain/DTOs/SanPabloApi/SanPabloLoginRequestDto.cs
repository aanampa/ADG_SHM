namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para la solicitud de login al API de San Pablo.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public class SanPabloLoginRequestDto
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
