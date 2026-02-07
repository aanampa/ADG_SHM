using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la bandeja de liquidaciones en el portal administrativo.
/// Muestra producciones con estado FACTURA_LIQUIDADA.
/// Permite generar ordenes de pago agrupando liquidaciones por banco.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// <modified>ADG Vladimir D - 2026-02-06 - Agregado soporte para generar ordenes de pago</modified>
/// </summary>
[Authorize]
public class LiquidacionController : Controller
{
    private readonly ILogger<LiquidacionController> _logger;
    private readonly ILiquidacionService _liquidacionService;
    private readonly IBancoService _bancoService;
    private readonly IOrdenPagoRepository _ordenPagoRepository;
    private readonly IOrdenPagoProduccionRepository _ordenPagoLiquidacionRepository;

    public LiquidacionController(
        ILogger<LiquidacionController> logger,
        ILiquidacionService liquidacionService,
        IBancoService bancoService,
        IOrdenPagoRepository ordenPagoRepository,
        IOrdenPagoProduccionRepository ordenPagoLiquidacionRepository)
    {
        _logger = logger;
        _liquidacionService = liquidacionService;
        _bancoService = bancoService;
        _ordenPagoRepository = ordenPagoRepository;
        _ordenPagoLiquidacionRepository = ordenPagoLiquidacionRepository;
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

    /// <summary>
    /// Obtiene el listado agrupado de liquidaciones por codigo y banco (AJAX).
    /// Para seleccion multiple y generacion de ordenes de pago.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetListGrupos(int? idBanco)
    {
        try
        {
            var idSede = GetCurrentUserIdSede();
            if (!idSede.HasValue || idSede.Value <= 0)
            {
                return PartialView("_ListGruposPartial", new LiquidacionGrupoListViewModel());
            }

            var grupos = await _liquidacionService.GetGruposAsync(idBanco, idSede.Value);

            var model = new LiquidacionGrupoListViewModel
            {
                Items = grupos.Select(g => new LiquidacionGrupoItemViewModel
                {
                    CodigoLiquidacion = g.CodigoLiquidacion ?? "",
                    NumeroLiquidacion = g.NumeroLiquidacion,
                    IdBanco = g.IdBanco,
                    CodigoBanco = g.CodigoBanco,
                    NombreBanco = g.NombreBanco,
                    DesTipoProduccion = g.DesTipoProduccion,
                    Descripcion = g.Descripcion,
                    Periodo = g.Periodo,
                    MtoTotal = g.MtoTotal,
                    CantidadFacturas = g.CantidadFacturas
                }).ToList(),
                IdBanco = idBanco
            };

            _logger.LogInformation("Listando grupos de liquidaciones. Total: {Total}, Banco: {Banco}",
                model.Items.Count, idBanco?.ToString() ?? "Todos");

            return PartialView("_ListGruposPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar grupos de liquidaciones");
            return PartialView("_ListGruposPartial", new LiquidacionGrupoListViewModel());
        }
    }

    /// <summary>
    /// Obtiene las producciones asociadas a un codigo de liquidacion (AJAX).
    /// Para mostrar en modal de detalle.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// <modified>ADG Vladimir D - 2026-02-06 - Agregado filtro por ID_BANCO para consistencia con CantidadFacturas</modified>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProduccionesByLiquidacion(string codigoLiquidacion, int? idBanco)
    {
        try
        {
            var idSede = GetCurrentUserIdSede();
            if (!idSede.HasValue || idSede.Value <= 0 || string.IsNullOrEmpty(codigoLiquidacion))
            {
                return PartialView("_ProduccionesModalPartial", new List<LiquidacionItemViewModel>());
            }

            var producciones = await _liquidacionService.GetProduccionesByCodigoLiquidacionAsync(codigoLiquidacion, idSede.Value, idBanco);

            var items = producciones.Select(p => new LiquidacionItemViewModel
            {
                GuidRegistro = p.GuidRegistro ?? "",
                CodigoProduccion = p.CodigoProduccion,
                DesTipoProduccion = p.DesTipoProduccion,
                Descripcion = p.Descripcion,
                Periodo = p.Periodo,
                Ruc = p.Ruc,
                RazonSocial = p.RazonSocial,
                MtoTotal = p.MtoTotal,
                CodigoLiquidacion = p.CodigoLiquidacion,
                EstadoLiquidacion = p.EstadoLiquidacion,
                NombreBanco = p.NombreBanco,
                Serie = p.Serie,
                Numero = p.Numero,
                FechaEmision = p.FechaEmision
            }).ToList();

            return PartialView("_ProduccionesModalPartial", items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producciones de liquidacion {Codigo}", codigoLiquidacion);
            return PartialView("_ProduccionesModalPartial", new List<LiquidacionItemViewModel>());
        }
    }

    /// <summary>
    /// Genera una orden de pago a partir de las liquidaciones seleccionadas.
    /// Todas las liquidaciones deben corresponder al mismo banco.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GenerarOrdenPago([FromBody] GenerarOrdenPagoRequest request)
    {
        try
        {
            if (request.CodigosLiquidacion == null || !request.CodigosLiquidacion.Any())
            {
                return Json(new { success = false, message = "Debe seleccionar al menos una liquidacion." });
            }

            if (!request.IdBanco.HasValue || request.IdBanco <= 0)
            {
                return Json(new { success = false, message = "El banco es requerido." });
            }

            var idSede = GetCurrentUserIdSede();
            if (!idSede.HasValue || idSede.Value <= 0)
            {
                return Json(new { success = false, message = "No se pudo determinar la sede del usuario." });
            }

            var idUsuario = GetCurrentUserId();
            if (!idUsuario.HasValue)
            {
                return Json(new { success = false, message = "No se pudo determinar el usuario." });
            }

            // Obtener todas las producciones de los codigos seleccionados (filtrando por banco)
            var todasLasProducciones = new List<SHM.AppDomain.DTOs.Liquidacion.LiquidacionListaResponseDto>();
            foreach (var codigo in request.CodigosLiquidacion)
            {
                var producciones = await _liquidacionService.GetProduccionesByCodigoLiquidacionAsync(codigo, idSede.Value, request.IdBanco);
                todasLasProducciones.AddRange(producciones);
            }

            if (!todasLasProducciones.Any())
            {
                return Json(new { success = false, message = "No se encontraron producciones para las liquidaciones seleccionadas." });
            }

            // Calcular totales
            var mtoConsumoAcum = todasLasProducciones.Sum(p => p.MtoConsumo ?? 0);
            var mtoDescuentoAcum = todasLasProducciones.Sum(p => p.MtoDescuento ?? 0);
            var mtoSubtotalAcum = todasLasProducciones.Sum(p => p.MtoSubtotal ?? 0);
            var mtoRentaAcum = todasLasProducciones.Sum(p => p.MtoRenta ?? 0);
            var mtoIgvAcum = todasLasProducciones.Sum(p => p.MtoIgv ?? 0);
            var mtoTotalAcum = todasLasProducciones.Sum(p => p.MtoTotal ?? 0);

            // Generar numero de orden de pago (formato: OP-YYYYMMDD-XXXX)
            var numeroOrdenPago = $"OP-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";

            // Crear la orden de pago
            var ordenPago = new OrdenPago
            {
                IdSede = idSede.Value,
                IdBanco = request.IdBanco.Value,
                NumeroOrdenPago = numeroOrdenPago,
                FechaGeneracion = DateTime.Now,
                Estado = "PENDIENTE",
                MtoConsumoAcum = mtoConsumoAcum,
                MtoDescuentoAcum = mtoDescuentoAcum,
                MtoSubtotalAcum = mtoSubtotalAcum,
                MtoRentaAcum = mtoRentaAcum,
                MtoIgvAcum = mtoIgvAcum,
                MtoTotalAcum = mtoTotalAcum,
                CantComprobantes = todasLasProducciones.Count,
                CantLiquidaciones = request.CodigosLiquidacion.Count(),
                Comentarios = $"Orden generada para {request.CodigosLiquidacion.Count()} liquidaciones",
                IdCreador = idUsuario.Value
            };

            var idOrdenPago = await _ordenPagoRepository.CreateAsync(ordenPago);

            // Crear las relaciones con las producciones
            var ordenPagoLiquidaciones = todasLasProducciones.Select(p => new OrdenPagoProduccion
            {
                IdOrdenPago = idOrdenPago,
                IdProduccion = p.IdProduccion,
                IdCreador = idUsuario.Value,
                Activo = 1
            }).ToList();

            await _ordenPagoLiquidacionRepository.CreateBulkAsync(ordenPagoLiquidaciones);

            // Actualizar estado de las producciones a FACTURA_ORDEN_PAGO
            var idsProduccion = todasLasProducciones.Select(p => p.IdProduccion).ToList();
            await _liquidacionService.UpdateEstadoProduccionesAsync(idsProduccion, "FACTURA_ORDEN_PAGO", idUsuario.Value);

            _logger.LogInformation("Orden de pago {NumeroOrden} generada. ID: {Id}, Banco: {Banco}, Total: {Total}, Producciones actualizadas: {Count}",
                numeroOrdenPago, idOrdenPago, request.IdBanco, mtoTotalAcum, idsProduccion.Count);

            return Json(new
            {
                success = true,
                message = $"Orden de pago {numeroOrdenPago} generada correctamente.",
                data = new
                {
                    idOrdenPago,
                    numeroOrdenPago,
                    cantidadProducciones = todasLasProducciones.Count,
                    cantidadLiquidaciones = request.CodigosLiquidacion.Count(),
                    montoTotal = mtoTotalAcum
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar orden de pago");
            return Json(new { success = false, message = "Error al generar la orden de pago. " + ex.Message });
        }
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
