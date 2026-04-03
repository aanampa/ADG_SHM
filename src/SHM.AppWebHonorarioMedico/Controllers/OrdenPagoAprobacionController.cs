using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la bandeja de aprobacion de ordenes de pago.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
[Authorize]
public class OrdenPagoAprobacionController : Controller
{
    private readonly ILogger<OrdenPagoAprobacionController> _logger;
    private readonly IOrdenPagoService _ordenPagoService;
    private readonly IOrdenPagoLiquidacionService _ordenPagoLiquidacionService;
    private readonly IOrdenPagoAprobacionService _ordenPagoAprobacionService;
    private readonly IBitacoraService _bitacoraService;
    private readonly IPerfilAprobacionUsuarioService _perfilAprobacionUsuarioService;

    public OrdenPagoAprobacionController(
        ILogger<OrdenPagoAprobacionController> logger,
        IOrdenPagoService ordenPagoService,
        IOrdenPagoLiquidacionService ordenPagoLiquidacionService,
        IOrdenPagoAprobacionService ordenPagoAprobacionService,
        IBitacoraService bitacoraService,
        IPerfilAprobacionUsuarioService perfilAprobacionUsuarioService)
    {
        _logger = logger;
        _ordenPagoService = ordenPagoService;
        _ordenPagoLiquidacionService = ordenPagoLiquidacionService;
        _ordenPagoAprobacionService = ordenPagoAprobacionService;
        _bitacoraService = bitacoraService;
        _perfilAprobacionUsuarioService = perfilAprobacionUsuarioService;
    }

    /// <summary>
    /// Vista principal de la bandeja de aprobacion de ordenes de pago.
    /// </summary>
    [HttpGet]
    [Route("OrdenPagoAprobacion")]
    [Route("OrdenPagoAprobacion/Index")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de ordenes pendientes de aprobacion (AJAX).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return PartialView("_ListPartial", new OrdenPagoAprobacionListViewModel());
            }

            var allItems = await _ordenPagoService.GetPendingForApprovalByUserAsync(userId.Value);
            var itemsList = allItems.ToList();
            var totalCount = itemsList.Count;

            var pagedItems = itemsList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new OrdenPagoAprobacionListViewModel
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
                    MtoTotalAcum = o.MtoTotalAcum,
                    EstadoAprobJefeSede = o.EstadoAprobJefeSede,
                    EstadoAprobJefeCorp = o.EstadoAprobJefeCorp
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Bandeja de aprobacion. Usuario: {UserId}, Total: {Total}, Pagina: {Page}",
                userId, totalCount, pageNumber);

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar ordenes pendientes de aprobacion");
            return PartialView("_ListPartial", new OrdenPagoAprobacionListViewModel());
        }
    }

    /// <summary>
    /// Ver detalle de una orden de pago para aprobacion.
    /// </summary>
    [HttpGet]
    [Route("OrdenPagoAprobacion/Detalle/{guid}")]
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

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["APP_RESPONSE"] = "ERROR";
                TempData["APP_MESSAGE"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Index");
            }

            // Cargar liquidaciones asociadas
            var liquidaciones = await _ordenPagoLiquidacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Liquidaciones = liquidaciones.ToList();

            // Cargar aprobaciones asociadas
            var aprobaciones = await _ordenPagoAprobacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Aprobaciones = aprobaciones.OrderBy(a => a.Orden).ToList();

            // Cargar detalle de liquidaciones
            var detalleLiquidaciones = await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.DetalleLiquidaciones = detalleLiquidaciones.ToList();

            // Cargar bitacora de la orden de pago
            var bitacoras = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_ORDEN_PAGO", ordenPago.IdOrdenPago);
            ViewBag.Bitacoras = bitacoras.ToList();

            // Cargar perfiles de aprobacion del usuario actual
            var perfilesUsuario = await _perfilAprobacionUsuarioService.GetByUsuarioIdAsync(userId.Value);
            ViewBag.PerfilesUsuario = perfilesUsuario
                .Select(p => p.NombrePerfil)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToList();

            return View(ordenPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de orden de pago para aprobacion {Guid}", guid);
            TempData["APP_RESPONSE"] = "ERROR";
            TempData["APP_MESSAGE"] = "Error al obtener la orden de pago.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Aprueba una orden de pago.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar([FromBody] AprobacionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "No se pudo identificar al usuario." });

            if (string.IsNullOrEmpty(request?.GuidOrdenPago))
                return Json(new { success = false, message = "Orden de pago no especificada." });

            var ordenPago = await _ordenPagoService.GetByGuidAsync(request.GuidOrdenPago);
            if (ordenPago == null)
                return Json(new { success = false, message = "Orden de pago no encontrada." });

            var (success, message) = await _ordenPagoAprobacionService.AprobarAsync(ordenPago.IdOrdenPago, userId.Value);

            _logger.LogInformation("Aprobacion de orden de pago {NumeroOP}: {Resultado} por usuario {UserId}",
                ordenPago.NumeroOrdenPago, success ? "APROBADO" : "FALLIDO", userId);

            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar orden de pago");
            return Json(new { success = false, message = "Error interno al procesar la aprobaci\u00f3n." });
        }
    }

    /// <summary>
    /// Rechaza una orden de pago.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar([FromBody] RechazoRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "No se pudo identificar al usuario." });

            if (string.IsNullOrEmpty(request?.GuidOrdenPago))
                return Json(new { success = false, message = "Orden de pago no especificada." });

            if (string.IsNullOrEmpty(request.Comentario))
                return Json(new { success = false, message = "Debe ingresar un comentario para rechazar." });

            var ordenPago = await _ordenPagoService.GetByGuidAsync(request.GuidOrdenPago);
            if (ordenPago == null)
                return Json(new { success = false, message = "Orden de pago no encontrada." });

            var (success, message) = await _ordenPagoAprobacionService.RechazarAsync(
                ordenPago.IdOrdenPago, userId.Value, request.Comentario);

            _logger.LogInformation("Rechazo de orden de pago {NumeroOP}: {Resultado} por usuario {UserId}",
                ordenPago.NumeroOrdenPago, success ? "RECHAZADO" : "FALLIDO", userId);

            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar orden de pago");
            return Json(new { success = false, message = "Error interno al procesar el rechazo." });
        }
    }

    /// <summary>
    /// Vista con el historial de ordenes de pago aprobadas por el usuario.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-03-30</created>
    /// </summary>
    [HttpGet]
    [Route("OrdenPagoAprobacion/Aprobadas")]
    public IActionResult Aprobadas()
    {
        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de ordenes ya aprobadas por el usuario (AJAX).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-03-30</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetApprovedList(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return PartialView("_ApprovedListPartial", new OrdenPagoAprobacionListViewModel());

            var allItems = await _ordenPagoService.GetApprovedByUserAsync(userId.Value);
            var itemsList = allItems.ToList();
            var totalCount = itemsList.Count;

            var pagedItems = itemsList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new OrdenPagoAprobacionListViewModel
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
                    MtoTotalAcum = o.MtoTotalAcum,
                    EstadoAprobJefeSede = o.EstadoAprobJefeSede,
                    EstadoAprobJefeCorp = o.EstadoAprobJefeCorp
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Historial de aprobadas. Usuario: {UserId}, Total: {Total}, Pagina: {Page}",
                userId, totalCount, pageNumber);

            return PartialView("_ApprovedListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar ordenes aprobadas por el usuario");
            return PartialView("_ApprovedListPartial", new OrdenPagoAprobacionListViewModel());
        }
    }

    /// <summary>
    /// Ver detalle de una orden de pago aprobada (solo consulta, sin acciones).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-02</created>
    /// </summary>
    [HttpGet]
    [Route("OrdenPagoAprobacion/DetalleAprobada/{guid}")]
    public async Task<IActionResult> DetalleAprobada(string guid)
    {
        try
        {
            var ordenPago = await _ordenPagoService.GetByGuidAsync(guid);
            if (ordenPago == null)
            {
                TempData["APP_RESPONSE"] = "ERROR";
                TempData["APP_MESSAGE"] = "Orden de pago no encontrada.";
                return RedirectToAction("Aprobadas");
            }

            var liquidaciones = await _ordenPagoLiquidacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Liquidaciones = liquidaciones.ToList();

            var aprobaciones = await _ordenPagoAprobacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Aprobaciones = aprobaciones.OrderBy(a => a.Orden).ToList();

            var detalleLiquidaciones = await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.DetalleLiquidaciones = detalleLiquidaciones.ToList();

            var bitacoras = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_ORDEN_PAGO", ordenPago.IdOrdenPago);
            ViewBag.Bitacoras = bitacoras.ToList();

            return View(ordenPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de orden de pago aprobada {Guid}", guid);
            TempData["APP_RESPONSE"] = "ERROR";
            TempData["APP_MESSAGE"] = "Error al obtener la orden de pago.";
            return RedirectToAction("Aprobadas");
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return null;
    }
}
