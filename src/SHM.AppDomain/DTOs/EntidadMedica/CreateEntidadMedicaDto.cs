namespace SHM.AppDomain.DTOs.EntidadMedica;

/// <summary>
/// DTO para la creacion de una nueva entidad medica.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class CreateEntidadMedicaDto
{
    public string CodigoEntidad { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? CodigoAcreedor { get; set; }
    public string? CodigoCorrientista { get; set; }
    public string? Direccion { get; set; }
}
