using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

/// <summary>
/// ViewModel para el listado paginado de liquidaciones.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
public class LiquidacionListViewModel
{
    public List<LiquidacionItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? IdBanco { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public List<SelectListItem> Bancos { get; set; } = new();
}

/// <summary>
/// ViewModel para un item de liquidacion en el listado.
/// Incluye campos de produccion, liquidacion y cuenta bancaria.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
public class LiquidacionItemViewModel
{
    // Produccion
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
    public decimal? MtoTotal { get; set; }

    // Comprobante
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public string? ComprobanteFactura => !string.IsNullOrEmpty(Serie) && !string.IsNullOrEmpty(Numero)
        ? $"{Serie}-{Numero}"
        : null;
    public DateTime? FechaEmision { get; set; }

    // Liquidacion
    public string? NumeroLiquidacion { get; set; }
    public string? CodigoLiquidacion { get; set; }
    public string? PeriodoLiquidacion { get; set; }
    public string? EstadoLiquidacion { get; set; }
    public DateTime? FechaLiquidacion { get; set; }
    public string? DescripcionLiquidacion { get; set; }

    // Banco y Cuenta
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }

    public int Activo { get; set; }
}

/// <summary>
/// ViewModel para el listado agrupado de liquidaciones.
/// Agrupa por CODIGO_LIQUIDACION e ID_BANCO.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-06</created>
/// </summary>
public class LiquidacionGrupoListViewModel
{
    public List<LiquidacionGrupoItemViewModel> Items { get; set; } = new();
    public int? IdBanco { get; set; }
}

/// <summary>
/// ViewModel para un item de liquidacion agrupado.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-06</created>
/// </summary>
public class LiquidacionGrupoItemViewModel
{
    public string CodigoLiquidacion { get; set; } = string.Empty;
    public string? NumeroLiquidacion { get; set; }
    public int? IdBanco { get; set; }
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? DesTipoProduccion { get; set; }
    public string? Descripcion { get; set; }
    public string? Periodo { get; set; }
    public decimal? MtoTotal { get; set; }
    public int CantidadFacturas { get; set; }
}

/// <summary>
/// Request para generar una orden de pago.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-06</created>
/// </summary>
public class GenerarOrdenPagoRequest
{
    public IEnumerable<string>? CodigosLiquidacion { get; set; }
    public int? IdBanco { get; set; }
}
