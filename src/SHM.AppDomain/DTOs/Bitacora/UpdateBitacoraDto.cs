using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.Bitacora;

/// <summary>
/// DTO para la actualizacion de un registro de bitacora existente.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UpdateBitacoraDto
{
    [MaxLength(100)]
    public string? Entidad { get; set; }

    [MaxLength(100)]
    public string? Accion { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public int? Activo { get; set; }
}
