using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.TablaDetalle;

/// <summary>
/// DTO para la actualizacion de un detalle de tabla existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateTablaDetalleDto
{
    public int? IdTabla { get; set; }

    [MaxLength(50)]
    public string? Codigo { get; set; }

    [MaxLength(250)]
    public string? Descripcion { get; set; }

    public int? Orden { get; set; }

    public int? Activo { get; set; }
}
