using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Parametro;

/// <summary>
/// DTO para la actualizacion de un parametro existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateParametroDto
{
    [MaxLength(100)]
    public string? Codigo { get; set; }

    [MaxLength(512)]
    public string? Valor { get; set; }

    public int? Activo { get; set; }
}
