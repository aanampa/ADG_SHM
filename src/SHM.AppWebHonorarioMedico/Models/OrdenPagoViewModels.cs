namespace SHM.AppWebHonorarioMedico.Models;

/// <summary>
/// ViewModel para el listado paginado de ordenes de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public class OrdenPagoListViewModel
{
    public List<OrdenPagoItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? IdBanco { get; set; }
    public string? Estado { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// ViewModel para un item de orden de pago en el listado.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public class OrdenPagoItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NumeroOrdenPago { get; set; }
    public DateTime? FechaGeneracion { get; set; }
    public string? NombreBanco { get; set; }
    public int? CantLiquidaciones { get; set; }
    public int? CantComprobantes { get; set; }
    public string? Estado { get; set; }
    public decimal? MtoSubtotalAcum { get; set; }
    public decimal? MtoIgvAcum { get; set; }
    public decimal? MtoRentaAcum { get; set; }
    public decimal? MtoTotalAcum { get; set; }
}

/// <summary>
/// ViewModel para el listado paginado de ordenes de pago en bandeja de aprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class OrdenPagoAprobacionListViewModel
{
    public List<OrdenPagoItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Request para aprobar una orden de pago.
/// </summary>
public class AprobacionRequest
{
    public string? GuidOrdenPago { get; set; }
}

/// <summary>
/// Request para rechazar una orden de pago.
/// </summary>
public class RechazoRequest
{
    public string? GuidOrdenPago { get; set; }
    public string? Comentario { get; set; }
}
