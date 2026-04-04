using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.Bitacora;
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
    private readonly IBitacoraService _bitacoraService;
    private readonly ITablaDetalleService _tablaDetalleService;

    public OrdenPagoController(
       ILogger<OrdenPagoController> logger,
       IOrdenPagoService ordenPagoService,
       IOrdenPagoLiquidacionService ordenPagoLiquidacionService,
       IOrdenPagoAprobacionService ordenPagoAprobacionService,
       IBancoService bancoService,
       IBitacoraService bitacoraService,
       ITablaDetalleService tablaDetalleService)
    {
        _logger = logger;
        _ordenPagoService = ordenPagoService;
        _ordenPagoLiquidacionService = ordenPagoLiquidacionService;
        _ordenPagoAprobacionService = ordenPagoAprobacionService;
        _bancoService = bancoService;
        _bitacoraService = bitacoraService;
        _tablaDetalleService = tablaDetalleService;
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
        // Cargar bancos para el filtro Select2 (solo bancos con cuentas registradas)
        var bancos = await _bancoService.GetBancosConCuentasAsync();
        ViewBag.Bancos = bancos
            .OrderBy(b => b.NombreBanco)
            .Select(b => new { id = b.IdBanco, text = b.NombreBanco })
            .ToList();

        // Cargar estados de orden de pago para el filtro
        var estados = await _tablaDetalleService.ListarPorCodigoTablaAsync("ESTADO_ORDEN_PAGO");
        ViewBag.Estados = estados.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = e.Codigo,
            Text = e.Descripcion
        }).ToList();

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
            var idSede = GetCurrentUserIdSede();
            var (pagedItems, totalCount) = await _ordenPagoService.GetPaginatedListAsync(idBanco, estado, idSede, pageNumber, pageSize);

            var model = new OrdenPagoListViewModel
            {
                Items = pagedItems.Select(o => new OrdenPagoItemViewModel
                {
                    GuidRegistro = o.GuidRegistro ?? "",
                    NumeroOrdenPago = o.NumeroOrdenPago,
                    FechaGeneracion = o.FechaGeneracion,
                    NombreSede = o.NombreSede,
                    NombreBanco = o.NombreBanco,
                    CantLiquidaciones = o.CantLiquidaciones,
                    CantComprobantes = o.CantComprobantes,
                    Estado = o.Estado,
                    MtoSubtotalAcum = o.MtoSubtotalAcum,
                    MtoIgvAcum = o.MtoIgvAcum,
                    MtoRentaAcum = o.MtoRentaAcum,
                    MtoTotalAcum = o.MtoTotalAcum,
                    EstadoAprobJefeSede = o.EstadoAprobJefeSede,
                    EstadoAprobJefeCorp = o.EstadoAprobJefeCorp
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IdBanco = idBanco,
                Estado = estado
            };

            _logger.LogInformation("Listando ordenes de pago. Total: {Total}, Pagina: {Page}, Sede: {Sede}, Banco: {Banco}, Estado: {Estado}",
                totalCount, pageNumber, idSede?.ToString() ?? "Todas", idBanco?.ToString() ?? "Todos", estado ?? "Todos");

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

            // Cargar bitacora de la orden de pago
            var bitacoras = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_ORDEN_PAGO", ordenPago.IdOrdenPago);
            ViewBag.Bitacoras = bitacoras.ToList();

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

    /// <summary>
    /// Anula una orden de pago en estado DEVUELTO y revierte las producciones a FACTURA_LIQUIDADA.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-04-02</created>
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular([FromForm] string guid)
    {
        try
        {
            var idUsuario = GetCurrentUserId();
            if (!idUsuario.HasValue)
                return Json(new { success = false, message = "Usuario no autenticado." });

            var ordenPago = await _ordenPagoService.GetByGuidAsync(guid);
            if (ordenPago == null)
                return Json(new { success = false, message = "Orden de pago no encontrada." });

            var estadosAnulables = new[] { EstadoDescripcion.OrdenPago.Devuelto, EstadoDescripcion.OrdenPago.AprobacionPendiente };
            if (!estadosAnulables.Contains(ordenPago.Estado))
                return Json(new { success = false, message = "Solo se pueden anular órdenes en estado Aprobación Pendiente o Devuelto." });

            var resultado = await _ordenPagoService.AnularAsync(guid, idUsuario.Value);
            if (!resultado)
                return Json(new { success = false, message = "No se pudo anular la orden de pago." });

            await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
            {
                Entidad = "SHM_ORDEN_PAGO",
                IdEntidad = ordenPago.IdOrdenPago,
                Accion = EstadoDescripcion.OrdenPago.Anulado,
                Descripcion = $"Orden de Pago anulada: {ordenPago.NumeroOrdenPago}. Las producciones asociadas fueron revertidas a Factura Liquidada.",
                FechaAccion = DateTime.Now
            }, idUsuario.Value);

            _logger.LogInformation("Orden de pago {NumeroOrden} anulada por usuario {IdUsuario}", ordenPago.NumeroOrdenPago, idUsuario);

            return Json(new { success = true, message = "Orden de pago anulada correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular orden de pago {Guid}", guid);
            return Json(new { success = false, message = "Error al anular la orden de pago." });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            return userId;
        return null;
    }

    private int? GetCurrentUserIdSede()
    {
        var sedeIdClaim = User.FindFirstValue("IdSede");
        if (!string.IsNullOrEmpty(sedeIdClaim) && int.TryParse(sedeIdClaim, out int idSede) && idSede > 0)
            return idSede;
        return null;
    }
}
