using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la gestion de ordenes de pago en el portal administrativo.
///
/// <author>ADG Antonio</author>
/// <created>2025-02-03</created>
/// <modified>ADG Antonio - 2026-02-07 - Implementacion completa con patron AJAX</modified>
/// </summary>
[Authorize]
public class OrdenPagoController : Controller
{
    private readonly ILogger<OrdenPagoController> _logger;
    private readonly IOrdenPagoService _ordenPagoService;
    private readonly IOrdenPagoLiquidacionService _ordenPagoLiquidacionService;
    private readonly IOrdenPagoAprobacionService _ordenPagoAprobacionService;
    private readonly IBancoService _bancoService;

    public OrdenPagoController(
       ILogger<OrdenPagoController> logger,
       IOrdenPagoService ordenPagoService,
       IOrdenPagoLiquidacionService ordenPagoLiquidacionService,
       IOrdenPagoAprobacionService ordenPagoAprobacionService,
       IBancoService bancoService)
    {
        _logger = logger;
        _ordenPagoService = ordenPagoService;
        _ordenPagoLiquidacionService = ordenPagoLiquidacionService;
        _ordenPagoAprobacionService = ordenPagoAprobacionService;
        _bancoService = bancoService;
    }

    /// <summary>
    /// Vista principal de la bandeja de ordenes de pago.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-07</created>
    /// </summary>
    [HttpGet]
    [Route("OrdenPago")]
    [Route("OrdenPago/Index")]
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
    /// Obtiene el listado paginado de ordenes de pago (AJAX).
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-07</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(int? idBanco, string? estado, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var allItems = await _ordenPagoService.GetAllActiveAsync();

            // Aplicar filtros
            if (idBanco.HasValue && idBanco.Value > 0)
            {
                allItems = allItems.Where(o => o.IdBanco == idBanco.Value);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                allItems = allItems.Where(o => o.Estado == estado);
            }

            var itemsList = allItems.ToList();
            var totalCount = itemsList.Count;

            // Paginacion
            var pagedItems = itemsList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new OrdenPagoListViewModel
            {
                Items = pagedItems.Select(o => new OrdenPagoItemViewModel
                {
                    GuidRegistro = o.GuidRegistro ?? "",
                    NumeroOrdenPago = o.NumeroOrdenPago,
                    FechaGeneracion = o.FechaGeneracion,
                    NombreBanco = o.NombreBanco,
                    CantLiquidaciones = o.CantLiquidaciones,
                    CantComprobantes = o.CantComprobantes,
                    Estado = o.Estado,
                    MtoSubtotalAcum = o.MtoSubtotalAcum,
                    MtoIgvAcum = o.MtoIgvAcum,
                    MtoRentaAcum = o.MtoRentaAcum,
                    MtoTotalAcum = o.MtoTotalAcum
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IdBanco = idBanco,
                Estado = estado
            };

            _logger.LogInformation("Listando ordenes de pago. Total: {Total}, Pagina: {Page}, Banco: {Banco}, Estado: {Estado}",
                totalCount, pageNumber, idBanco?.ToString() ?? "Todos", estado ?? "Todos");

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar ordenes de pago");
            return PartialView("_ListPartial", new OrdenPagoListViewModel());
        }
    }

    /// <summary>
    /// Ver detalle de una orden de pago.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-07</created>
    /// </summary>
    [HttpGet]
    [Route("OrdenPago/Detalle/{guid}")]
    public async Task<IActionResult> Detalle(string guid)
    {
        try
        {
            var ordenPago = await _ordenPagoService.GetByGuidAsync(guid);
            if (ordenPago == null)
            {
                TempData["APP_RESPONSE"] = "ERROR";
                TempData["APP_MESSAGE"] = "Orden de pago no encontrada.";
                return RedirectToAction("Index");
            }

            // Cargar liquidaciones asociadas a la orden de pago
            var liquidaciones = await _ordenPagoLiquidacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Liquidaciones = liquidaciones.ToList();

            // Cargar aprobaciones asociadas a la orden de pago
            var aprobaciones = await _ordenPagoAprobacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Aprobaciones = aprobaciones.OrderBy(a => a.Orden).ToList();

            // Cargar detalle de liquidaciones (producciones por liquidacion)
            var detalleLiquidaciones = await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.DetalleLiquidaciones = detalleLiquidaciones.ToList();

            return View(ordenPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de orden de pago {Guid}", guid);
            TempData["APP_RESPONSE"] = "ERROR";
            TempData["APP_MESSAGE"] = "Error al obtener la orden de pago.";
            return RedirectToAction("Index");
        }
    }
}
