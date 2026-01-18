namespace SHM.AppDomain.DTOs.EntidadCuentaBancaria;

/// <summary>
/// DTO de respuesta con la informacion de una cuenta bancaria.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadCuentaBancariaResponseDto
{
    public int IdCuentaBancaria { get; set; }
    public int? IdEntidad { get; set; }
    public int? IdBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
