namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa un banco del sistema financiero.
/// Mapea a la tabla SHM_BANCO en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class Banco
{
    public int IdBanco { get; set; }
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? GuidRegistro { get; set; }
    public int Activo { get; set; }
    public int IdCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? IdModificador { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
