using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Parametro;

/// <summary>
/// DTO para la creacion de un nuevo parametro.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateParametroDto
{
    [Required]
    [MaxLength(100)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Descripcion { get; set; }

    [MaxLength(512)]
    public string? Valor { get; set; }
}
