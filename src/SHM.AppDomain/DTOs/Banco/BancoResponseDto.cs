namespace SHM.AppDomain.DTOs.Banco;

/// <summary>
/// DTO de respuesta con la informacion de un banco.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class BancoResponseDto
{
    public int IdBanco { get; set; }
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public int Activo { get; set; }
    public string? GuidRegistro { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
