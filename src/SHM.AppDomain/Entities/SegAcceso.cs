namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un registro de acceso al sistema.
/// Mapea a la tabla SHM_SEG_ACCESO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-25</created>
/// </summary>
public class SegAcceso
{
    public int     IdAcceso      { get; set; }
    public int?    IdUsuario     { get; set; }
    public string  LoginIntento  { get; set; } = string.Empty;
    public string  TipoEvento    { get; set; } = string.Empty;
    public string  IpAcceso      { get; set; } = string.Empty;
    public string? UserAgent     { get; set; }
    public string? Detalle       { get; set; }
    public string  GuidRegistro  { get; set; } = string.Empty;
    public DateTime FechaAcceso  { get; set; }
}

/// <summary>
/// Valores posibles del campo TipoEvento en SHM_SEG_ACCESO.
/// </summary>
public static class TipoEventoAcceso
{
    public const string LoginOk       = "LOGIN_OK";
    public const string LoginFail     = "LOGIN_FAIL";
    public const string Logout        = "LOGOUT";
    public const string SesionExpirada = "SESSION_EXPIRE";
}
