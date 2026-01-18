using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.TablaDetalle;

/// <summary>
/// DTO para la creacion de un nuevo detalle de tabla.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateTablaDetalleDto
{
    [Required]
    public int IdTabla { get; set; }

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descripcion { get; set; }

    public int? Orden { get; set; }
}
