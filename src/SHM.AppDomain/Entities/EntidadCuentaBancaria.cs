namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa una cuenta bancaria de una entidad medica.
/// Mapea a la tabla SHM_ENTIDAD_CUENTA_BANCARIA en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadCuentaBancaria
{
    public int IdCuentaBancaria { get; set; }
    public int? IdEntidad { get; set; }
    public int? IdBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
