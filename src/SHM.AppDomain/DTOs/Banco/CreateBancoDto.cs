using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Banco;

/// <summary>
/// DTO para la creacion de un nuevo banco.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateBancoDto
{
    [Required]
    [MaxLength(10)]
    public string CodigoBanco { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NombreBanco { get; set; } = string.Empty;
}
