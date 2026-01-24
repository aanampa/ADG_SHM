using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Bitacora;

/// <summary>
/// DTO para la creacion de un nuevo registro de bitacora.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos IdEntidad y FechaAccion</modified>
/// </summary>
public class CreateBitacoraDto
{
    public int? IdEntidad { get; set; }

    [Required]
    [MaxLength(100)]
    public string Entidad { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public DateTime? FechaAccion { get; set; }
}
