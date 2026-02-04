using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la bandeja de liquidaciones en el portal administrativo.
/// Muestra producciones con estado FACTURA_LIQUIDADA.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
[Authorize]
public class LiquidacionController : Controller
{
    private readonly ILogger<LiquidacionController> _logger;
    private readonly ILiquidacionService _liquidacionService;
    private readonly IBancoService _bancoService;

    public LiquidacionController(
        ILogger<LiquidacionController> logger,
        ILiquidacionService liquidacionService,
        IBancoService bancoService)
    {
        _logger = logger;
        _liquidacionService = liquidacionService;
        _bancoService = bancoService;
    }

    /// <summary>
    /// Vista principal de la bandeja de liquidaciones.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    [HttpGet]
    [Route("Liquidacion")]
    [Route("Liquidacion/Index")]
    public async Task<IActionResult> Index()
    {
        // Cargar bancos para el filtro Select2
        var bancos = await _bancoService.GetAllBancosAsync();
        ViewBag.Bancos = bancos
            .Where(b => b.Activo == 1)
            .OrderBy(b => b.NombreBanco)
            .Select(b => new { id = b.IdBanco, text = b.NombreBanco })
            .ToList();

        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de liquidaciones (AJAX).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(int? idBanco, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            // Obtener IdSede del usuario logueado desde los claims
            var idSede = GetCurrentUserIdSede();

            var (items, totalCount) = await _liquidacionService.GetPaginatedListAsync(idBanco, idSede, pageNumber, pageSize);

            var model = new LiquidacionListViewModel
            {
                Items = items.Select(l => new LiquidacionItemViewModel
                {
                    GuidRegistro = l.GuidRegistro ?? "",
                    CodigoProduccion = l.CodigoProduccion,
                    TipoProduccion = l.TipoProduccion,
                    DesTipoProduccion = l.DesTipoProduccion,
                    Descripcion = l.Descripcion,
                    Periodo = l.Periodo,
                    Estado = l.Estado,
                    DesEstado = l.DesEstado,
                    Ruc = l.Ruc,
                    RazonSocial = l.RazonSocial,
                    NombreSede = l.NombreSede,
                    MtoTotal = l.MtoTotal,
                    Serie = l.Serie,
                    Numero = l.Numero,
                    FechaEmision = l.FechaEmision,
                    // Liquidacion
                    NumeroLiquidacion = l.NumeroLiquidacion,
                    CodigoLiquidacion = l.CodigoLiquidacion,
                    PeriodoLiquidacion = l.PeriodoLiquidacion,
                    EstadoLiquidacion = l.EstadoLiquidacion,
                    FechaLiquidacion = l.FechaLiquidacion,
                    DescripcionLiquidacion = l.DescripcionLiquidacion,
                    // Banco
                    NombreBanco = l.NombreBanco,
                    CuentaCorriente = l.CuentaCorriente,
                    CuentaCci = l.CuentaCci,
                    Moneda = l.Moneda,
                    Activo = l.Activo
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IdBanco = idBanco
            };

            _logger.LogInformation("Listando liquidaciones. Total: {Total}, Pagina: {Page}, Banco: {Banco}",
                totalCount, pageNumber, idBanco?.ToString() ?? "Todos");

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar liquidaciones");
            return PartialView("_ListPartial", new LiquidacionListViewModel());
        }
    }

    /// <summary>
    /// Ver detalle de una liquidacion.
    /// Redirige al detalle de Produccion ya que comparten la misma estructura.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    [HttpGet]
    [Route("Liquidacion/Detalle/{guid}")]
    public IActionResult Detalle(string guid)
    {
        // Redirigir a la vista de detalle de Produccion
        return RedirectToAction("Detalle", "Produccion", new { guid });
    }

    private int? GetCurrentUserIdSede()
    {
        var sedeIdClaim = User.FindFirstValue("IdSede");
        if (!string.IsNullOrEmpty(sedeIdClaim) && int.TryParse(sedeIdClaim, out int idSede) && idSede > 0)
        {
            return idSede;
        }
        return null;
    }
}
