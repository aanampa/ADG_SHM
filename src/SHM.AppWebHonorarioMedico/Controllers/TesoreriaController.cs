using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.Constants;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para el modulo de Tesoreria: bandeja de ordenes de pago aprobadas y pagadas.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-10</created>
/// </summary>
[Authorize]
public class TesoreriaController : Controller
{
    private readonly ILogger<TesoreriaController> _logger;
    private readonly IOrdenPagoService _ordenPagoService;
    private readonly IOrdenPagoLiquidacionService _ordenPagoLiquidacionService;
    private readonly IBancoService _bancoService;
    private readonly ITablaDetalleService _tablaDetalleService;
    private readonly IProduccionService _produccionService;

    public TesoreriaController(
        ILogger<TesoreriaController> logger,
        IOrdenPagoService ordenPagoService,
        IOrdenPagoLiquidacionService ordenPagoLiquidacionService,
        IBancoService bancoService,
        ITablaDetalleService tablaDetalleService,
        IProduccionService produccionService)
    {
        _logger = logger;
        _ordenPagoService = ordenPagoService;
        _ordenPagoLiquidacionService = ordenPagoLiquidacionService;
        _bancoService = bancoService;
        _tablaDetalleService = tablaDetalleService;
        _produccionService = produccionService;
    }

    /// <summary>
    /// Vista principal de la bandeja de tesoreria (ordenes aprobadas y pagadas).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-10</created>
    /// </summary>
    [HttpGet]
    [Route("Tesoreria")]
    [Route("Tesoreria/Index")]
    public async Task<IActionResult> Index()
    {
        var bancos = await _bancoService.GetBancosConCuentasAsync();
        ViewBag.Bancos = bancos
            .OrderBy(b => b.NombreBanco)
            .Select(b => new { id = b.IdBanco, text = b.NombreBanco })
            .ToList();

        // Solo mostrar estados APROBADO y PAGADO
        var todosEstados = await _tablaDetalleService.ListarPorCodigoTablaAsync("ESTADO_ORDEN_PAGO");
        var estadosTesoreria = new[] { EstadoDescripcion.OrdenPago.Aprobado, EstadoDescripcion.OrdenPago.Pagado };
        ViewBag.Estados = todosEstados
            .Where(e => estadosTesoreria.Contains(e.Codigo))
            .Select(e => new SelectListItem { Value = e.Codigo, Text = e.Descripcion })
            .ToList();

        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de ordenes para tesoreria (AJAX).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-10</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(int? idBanco, string? estado, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var idSede = GetCurrentUserIdSede();

            // Si no se selecciona estado, mostrar APROBADO por defecto (bandeja principal de tesoreria)
            var estadoFiltro = string.IsNullOrEmpty(estado) ? EstadoDescripcion.OrdenPago.Aprobado : estado;

            var (pagedItems, totalCount) = await _ordenPagoService.GetPaginatedListAsync(idBanco, estadoFiltro, idSede, pageNumber, pageSize);

            var model = new TesoreriaListViewModel
            {
                Items = pagedItems.Select(o => new TesoreriaItemViewModel
                {
                    GuidRegistro    = o.GuidRegistro ?? "",
                    NumeroOrdenPago = o.NumeroOrdenPago,
                    FechaGeneracion = o.FechaGeneracion,
                    NombreSede      = o.NombreSede,
                    NombreBanco     = o.NombreBanco,
                    CantLiquidaciones = o.CantLiquidaciones,
                    CantComprobantes  = o.CantComprobantes,
                    Estado          = o.Estado,
                    MtoTotalAcum    = o.MtoTotalAcum
                }).ToList(),
                TotalCount  = totalCount,
                PageNumber  = pageNumber,
                PageSize    = pageSize,
                IdBanco     = idBanco,
                Estado      = estado
            };

            _logger.LogInformation("Tesoreria - Listando ordenes. Total: {Total}, Pagina: {Page}, Banco: {Banco}, Estado: {Estado}",
                totalCount, pageNumber, idBanco?.ToString() ?? "Todos", estado ?? "Todos");

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la bandeja de tesoreria");
            return StatusCode(500, "Error al cargar la lista");
        }
    }

    /// <summary>
    /// Ver detalle de una orden de pago desde la bandeja de tesoreria.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-10</created>
    /// </summary>
    [HttpGet]
    [Route("Tesoreria/Detalle/{guid}")]
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

            var liquidaciones = await _ordenPagoLiquidacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.Liquidaciones = liquidaciones.ToList();

            var detalleLiquidaciones = await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            ViewBag.DetalleLiquidaciones = detalleLiquidaciones.ToList();

            return View(ordenPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de orden de pago en tesoreria {Guid}", guid);
            TempData["APP_RESPONSE"] = "ERROR";
            TempData["APP_MESSAGE"] = "Error al obtener la orden de pago.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Registra el pago de las facturas seleccionadas cambiando su estado de
    /// FACTURA_ORDEN_PAGO a FACTURA_PAGADA. Si todas las producciones de la orden
    /// quedan en FACTURA_PAGADA, actualiza la orden a estado PAGADO.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-11</created>
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoRequest request)
    {
        try
        {
            if (request == null || request.Guids == null || !request.Guids.Any())
                return BadRequest(new { success = false, message = "Debe seleccionar al menos una factura." });

            var idUsuario = GetCurrentUserId();
            if (idUsuario == null)
                return Unauthorized();

            var errores = new List<string>();
            var actualizados = 0;

            foreach (var guid in request.Guids)
            {
                // Obtener produccion y validar estado
                var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
                if (produccion == null)
                {
                    errores.Add($"Factura {guid}: no encontrada.");
                    continue;
                }
                if (produccion.Estado != EstadoDescripcion.Produccion.FacturaOrdenPago)
                {
                    errores.Add($"Factura {produccion.Serie}-{produccion.Numero}: estado inválido ({produccion.Estado}).");
                    continue;
                }

                var ok = await _produccionService.UpdateEstadoAsync(guid, EstadoDescripcion.Produccion.FacturaPagada, idUsuario.Value);
                if (ok) actualizados++;
                else errores.Add($"Factura {produccion.Serie}-{produccion.Numero}: error al actualizar.");
            }

            if (actualizados == 0)
                return Ok(new { success = false, message = "No se pudo actualizar ninguna factura.", errores });

            // Verificar si todas las producciones de la orden ya están FACTURA_PAGADA
            var ordenPago = await _ordenPagoService.GetByGuidAsync(request.GuidOrdenPago);
            if (ordenPago != null)
            {
                var pendientes = await _ordenPagoService.GetCountProduccionesNotPagadasAsync(ordenPago.IdOrdenPago);
                if (pendientes == 0)
                {
                    await _ordenPagoService.UpdateEstadoAsync(ordenPago.IdOrdenPago, EstadoDescripcion.OrdenPago.Pagado, idUsuario.Value);
                    _logger.LogInformation("Orden de pago {Guid} actualizada a PAGADO.", request.GuidOrdenPago);
                }
            }

            _logger.LogInformation("RegistrarPago: {Actualizados} factura(s) pagadas. Errores: {Errores}", actualizados, errores.Count);

            return Ok(new { success = true, actualizados, errores });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en RegistrarPago");
            return StatusCode(500, new { success = false, message = "Error interno al registrar el pago." });
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private int? GetCurrentUserIdSede()
    {
        var idSedeStr = User.FindFirstValue("IdSede");
        return int.TryParse(idSedeStr, out var idSede) && idSede > 0 ? idSede : null;
    }
}
