namespace SHM.AppDomain.Entities;

/// <summary>
/// Entidad que representa una entidad medica (proveedor de servicios medicos).
/// Mapea a la tabla SHM_ENTIDAD_MEDICA en la base de datos.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadMedica
{
    public int IdEntidadMedica { get; set; }
    public string CodigoEntidad { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? CodigoAcreedor { get; set; }
    public string? CodigoCorrientista { get; set; }
    public string? Direccion { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int IdCreador { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int? IdModificador { get; set; }
}
