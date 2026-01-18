namespace SHM.AppDomain.DTOs.EntidadCuentaBancaria;

/// <summary>
/// DTO para la actualizacion de una cuenta bancaria existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateEntidadCuentaBancariaDto
{
    public int? IdEntidad { get; set; }
    public int? IdBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
    public int? Activo { get; set; }
}
