namespace SHM.AppDomain.DTOs.EntidadCuentaBancaria;

/// <summary>
/// DTO para la creacion de una nueva cuenta bancaria de entidad.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateEntidadCuentaBancariaDto
{
    public int? IdEntidad { get; set; }
    public int? IdBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
}
