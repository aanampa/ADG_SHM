namespace SHM.AppDomain.DTOs.Usuario;

/// <summary>
/// DTO para solicitar recuperacion de clave.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class SolicitarRecuperacionDto
{
    public string EmailOrLogin { get; set; } = string.Empty;
}

/// <summary>
/// DTO para restablecer la clave con un token.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RestablecerClaveDto
{
    public string Token { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
    public string ConfirmarPassword { get; set; } = string.Empty;
}
