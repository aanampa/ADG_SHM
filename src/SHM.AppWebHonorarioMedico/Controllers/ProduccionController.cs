using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la gestion de producciones en el portal administrativo.
///
/// <author>ADG Vladimir D</author>
/// <created>2025-01-20</created>
/// </summary>
[Authorize]
public class ProduccionController : Controller
{
    private readonly ILogger<ProduccionController> _logger;
    private readonly IProduccionService _produccionService;
    private readonly ITablaDetalleService _tablaDetalleService;

    public ProduccionController(
        ILogger<ProduccionController> logger,
        IProduccionService produccionService,
        ITablaDetalleService tablaDetalleService)
    {
        _logger = logger;
        _produccionService = produccionService;
        _tablaDetalleService = tablaDetalleService;
    }

    /// <summary>
    /// Vista principal de la bandeja de producciones.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// </summary>
    [HttpGet]
    [Route("Produccion")]
    [Route("Produccion/Index")]
    public async Task<IActionResult> Index()
    {
        // Cargar estados para el filtro
        var estados = await _tablaDetalleService.ListarPorCodigoTablaAsync("ESTADO_PROCESO");
        ViewBag.Estados = estados.Select(e => new SelectListItem
        {
            Value = e.Codigo,
            Text = e.Descripcion
        }).ToList();

        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de producciones (AJAX).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(string? estado, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _produccionService.GetPaginatedListAsync(estado, pageNumber, pageSize);

            var model = new ProduccionListViewModel
            {
                Items = items.Select(p => new ProduccionItemViewModel
                {
                    GuidRegistro = p.GuidRegistro ?? "",
                    CodigoProduccion = p.CodigoProduccion,
                    TipoProduccion = p.TipoProduccion,
                    DesTipoProduccion = p.DesTipoProduccion,
                    Descripcion = p.Descripcion,
                    Periodo = p.Periodo,
                    Estado = p.Estado,
                    DesEstado = p.DesEstado,
                    Ruc = p.Ruc,
                    RazonSocial = p.RazonSocial,
                    NombreSede = p.NombreSede,
                    MtoSubtotal = p.MtoSubtotal,
                    MtoIgv = p.MtoIgv,
                    MtoRenta = p.MtoRenta,
                    MtoTotal = p.MtoTotal,
                    Serie = p.Serie,
                    Numero = p.Numero,
                    FechaEmision = p.FechaEmision,
                    Activo = p.Activo
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Estado = estado
            };

            _logger.LogInformation("Listando producciones. Total: {Total}, Pagina: {Page}, Estado: {Estado}",
                totalCount, pageNumber, estado ?? "Todos");

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar producciones");
            return PartialView("_ListPartial", new ProduccionListViewModel());
        }
    }

    /// <summary>
    /// Ver detalle de una produccion.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// </summary>
    [HttpGet]
    [Route("Produccion/Detalle/{guid}")]
    public async Task<IActionResult> Detalle(string guid)
    {
        try
        {
            var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
            if (produccion == null)
            {
                return NotFound();
            }

            return View(produccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de produccion: {Guid}", guid);
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Solicita factura actualizando fecha limite y estado.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-21</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SolicitarFactura([FromBody] SolicitarFacturaDto solicitud)
    {
        try
        {
            if (solicitud == null || string.IsNullOrEmpty(solicitud.GuidRegistro))
            {
                return Json(new { success = false, message = "Datos de solicitud invalidos" });
            }

            if (string.IsNullOrEmpty(solicitud.Fecha) || string.IsNullOrEmpty(solicitud.Hora))
            {
                return Json(new { success = false, message = "La fecha y hora limite son requeridas" });
            }

            var idUsuario = GetCurrentUserId();
            var resultado = await _produccionService.SolicitarFacturaAsync(solicitud, idUsuario);

            if (resultado)
            {
                _logger.LogInformation("Solicitud de factura enviada. GUID: {Guid}, Usuario: {Usuario}",
                    solicitud.GuidRegistro, idUsuario);
                return Json(new { success = true, message = "Solicitud de factura enviada correctamente" });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo procesar la solicitud" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar factura: {Guid}", solicitud?.GuidRegistro);
            return Json(new { success = false, message = "Error al procesar la solicitud" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("IdUsuario");
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int idUsuario))
        {
            return idUsuario;
        }
        return 0;
    }
}
