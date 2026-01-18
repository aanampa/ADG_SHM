using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Banco;

/// <summary>
/// DTO para la actualizacion de un banco existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateBancoDto
{
    [MaxLength(10)]
    public string? CodigoBanco { get; set; }

    [MaxLength(100)]
    public string? NombreBanco { get; set; }

    public int? Activo { get; set; }
}
