using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

/// <summary>
/// ViewModel para el listado paginado de producciones.
///
/// <author>ADG Vladimir D</author>
/// <created>2025-01-20</created>
/// </summary>
public class ProduccionListViewModel
{
    public List<ProduccionItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Estado { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public List<SelectListItem> Estados { get; set; } = new();
}

/// <summary>
/// ViewModel para un item de produccion en el listado.
///
/// <author>ADG Vladimir D</author>
/// <created>2025-01-20</created>
/// </summary>
public class ProduccionItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? CodigoProduccion { get; set; }
    public string? TipoProduccion { get; set; }
    public string? DesTipoProduccion { get; set; }
    public string? Descripcion { get; set; }
    public string? Periodo { get; set; }
    public string? Estado { get; set; }
    public string? DesEstado { get; set; }
    public string? Ruc { get; set; }
    public string? RazonSocial { get; set; }
    public string? NombreSede { get; set; }
    public decimal? MtoSubtotal { get; set; }
    public decimal? MtoIgv { get; set; }
    public decimal? MtoRenta { get; set; }
    public decimal? MtoTotal { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public string? ComprobanteFactura => !string.IsNullOrEmpty(Serie) && !string.IsNullOrEmpty(Numero)
        ? $"{Serie}-{Numero}"
        : null;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaLimite { get; set; }
    public int Activo { get; set; }
}

/// <summary>
/// ViewModel para archivos adjuntos de produccion.
///
/// <author>ADG Vladimir D</author>
/// <created>2025-01-22</created>
/// </summary>
public class ArchivoAdjuntoViewModel
{
    public string? GuidRegistro { get; set; }
    public string? TipoArchivo { get; set; }
    public string? NombreArchivo { get; set; }
    public string? NombreOriginal { get; set; }
    public string? Extension { get; set; }
    public int? Tamano { get; set; }
    public DateTime FechaCreacion { get; set; }
}
