namespace SHM.AppWebHonorarioMedico.Models;

/// <summary>
/// ViewModel para el listado paginado de ordenes de pago en la bandeja de tesoreria.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-10</created>
/// </summary>
public class TesoreriaListViewModel
{
    public List<TesoreriaItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? IdBanco { get; set; }
    public string? Estado { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// ViewModel para un item de orden de pago en la bandeja de tesoreria.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-10</created>
/// </summary>
public class TesoreriaItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NumeroOrdenPago { get; set; }
    public DateTime? FechaGeneracion { get; set; }
    public string? NombreSede { get; set; }
    public string? NombreBanco { get; set; }
    public int? CantLiquidaciones { get; set; }
    public int? CantComprobantes { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public decimal? MtoTotalAcum { get; set; }
}

/// <summary>
/// Request para el endpoint RegistrarPago.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-11</created>
/// </summary>
public class RegistrarPagoRequest
{
    public string GuidOrdenPago { get; set; } = string.Empty;
    public List<string> Guids { get; set; } = new();
}

/// <summary>
/// Request para el endpoint SincronizarPagoFacturas.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-22</created>
/// </summary>
public class SincronizarPagoRequest
{
    public string GuidOrdenPago { get; set; } = string.Empty;
}
