using System.ComponentModel.DataAnnotations;

namespace SHM.AppDomain.DTOs.ArchivoComprobante;

/// <summary>
/// DTO para la creacion de una nueva relacion archivo-comprobante.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateArchivoComprobanteDto
{
    public int? IdProduccion { get; set; }

    public int? IdArchivo { get; set; }

    [MaxLength(30)]
    public string? TipoArchivo { get; set; }

    [MaxLength(100)]
    public string? Descripcion { get; set; }
}
