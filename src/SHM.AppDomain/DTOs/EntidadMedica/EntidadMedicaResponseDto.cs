namespace SHM.AppDomain.DTOs.EntidadMedica;

/// <summary>
/// DTO de respuesta con la informacion de una entidad medica.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadMedicaResponseDto
{
    public int IdEntidadMedica { get; set; }
    public string CodigoEntidad { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? CodigoAcreedor { get; set; }
    public string? Direccion { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
