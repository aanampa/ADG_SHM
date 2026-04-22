using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using SHM.AppDomain.Constants;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;
using SHM.AppWebHonorarioMedico.Reports;
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
    private readonly IOrdenPagoAprobacionService _ordenPagoAprobacionService;
    private readonly IWebHostEnvironment _env;

    public TesoreriaController(
        ILogger<TesoreriaController> logger,
        IOrdenPagoService ordenPagoService,
        IOrdenPagoLiquidacionService ordenPagoLiquidacionService,
        IBancoService bancoService,
        ITablaDetalleService tablaDetalleService,
        IProduccionService produccionService,
        IOrdenPagoAprobacionService ordenPagoAprobacionService,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _ordenPagoService = ordenPagoService;
        _ordenPagoLiquidacionService = ordenPagoLiquidacionService;
        _bancoService = bancoService;
        _tablaDetalleService = tablaDetalleService;
        _produccionService = produccionService;
        _ordenPagoAprobacionService = ordenPagoAprobacionService;
        _env = env;
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

    /// <summary>
    /// Genera y descarga el PDF de una Orden de Pago directamente en memoria.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-04-21</created>
    /// </summary>
    [HttpGet]
    [Route("Tesoreria/DescargarOrdenPago/{guid}")]
    public async Task<IActionResult> DescargarOrdenPago(string guid)
    {
        try
        {
            var ordenPago = await _ordenPagoService.GetByGuidAsync(guid);
            if (ordenPago == null)
                return NotFound("Orden de pago no encontrada.");

            var liquidaciones = await _ordenPagoLiquidacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            var detalle       = await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            var aprobaciones  = await _ordenPagoAprobacionService.GetByOrdenPagoIdAsync(ordenPago.IdOrdenPago);
            var logoPath      = Path.Combine(_env.WebRootPath, "images", "logo_login.jpg");

            var documento = new OrdenPagoDocument(
                ordenPago,
                [.. liquidaciones],
                [.. detalle],
                [.. aprobaciones],
                logoPath);

            var pdfBytes      = documento.GeneratePdf();
            var nombreArchivo = $"{ordenPago.NumeroOrdenPago ?? $"OP_{guid}"}.pdf";

            _logger.LogInformation("PDF Orden de Pago generado en memoria: {Numero}", ordenPago.NumeroOrdenPago);
            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF de orden de pago {Guid}", guid);
            return StatusCode(500, "Error al generar el reporte PDF.");
        }
    }

    /// <summary>
    /// Genera y descarga el Excel de comprobantes de una Orden de Pago directamente en memoria.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-04-21</created>
    /// </summary>
    [HttpGet]
    [Route("Tesoreria/DescargarExcelOrdenPago/{guid}")]
    public async Task<IActionResult> DescargarExcelOrdenPago(string guid)
    {
        try
        {
            var ordenPago = await _ordenPagoService.GetByGuidAsync(guid);
            if (ordenPago == null)
                return NotFound("Orden de pago no encontrada.");

            var detalle   = (await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(ordenPago.IdOrdenPago)).ToList();
            var logoPath  = Path.Combine(_env.WebRootPath, "images", "logo_login.jpg");

            var colorHeader     = XLColor.FromHtml("#6c757d");
            var colorHeaderFont = XLColor.White;
            var colorAlt        = XLColor.FromHtml("#f8f9fa");
            var colorAcento     = XLColor.FromHtml("#f26522");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Comprobantes");

            // --- Logo ---
            ws.Row(1).Height = 36;
            ws.Range(1, 1, 1, 13).Merge();
            if (System.IO.File.Exists(logoPath))
            {
                using var imgStream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
                ws.AddPicture(imgStream).MoveTo(ws.Cell("A1")).WithSize(120, 32);
            }

            // --- Título ---
            ws.Range(2, 1, 2, 13).Merge();
            ws.Cell(2, 1).Value = $"ORDEN DE PAGO  N° {ordenPago.NumeroOrdenPago ?? "-"}  —  {ordenPago.NombreSede ?? "-"}";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 13;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 1).Style.Fill.BackgroundColor = colorAcento;
            ws.Cell(2, 1).Style.Font.FontColor = colorHeaderFont;
            ws.Row(2).Height = 22;

            // --- Resumen ---
            int rRes = 3;
            var resumenLabels = new[]
            {
                ("Banco:",          ordenPago.NombreBanco ?? "-"),
                ("Fecha:",          ordenPago.FechaGeneracion?.ToString("dd/MM/yyyy") ?? "-"),
                ("Estado:",         EstadoDescripcion.OrdenPago.GetDescripcion(ordenPago.Estado)),
                ("Liquidaciones:",  ordenPago.CantLiquidaciones?.ToString() ?? "-"),
                ("Comprobantes:",   ordenPago.CantComprobantes?.ToString() ?? "-"),
                ("Sub Total S/:",   ordenPago.MtoSubtotalAcum?.ToString("N2") ?? "--"),
                ("IGV S/:",         ordenPago.MtoIgvAcum?.ToString("N2") ?? "--"),
                ("Imp. Renta S/:",  ordenPago.MtoRentaAcum?.ToString("N2") ?? "--"),
                ("TOTAL S/:",       ordenPago.MtoTotalAcum?.ToString("N2") ?? "--"),
            };
            int col = 1;
            foreach (var (label, valor) in resumenLabels)
            {
                ws.Cell(rRes, col).Value = label;
                ws.Cell(rRes, col).Style.Font.Bold = true;
                ws.Cell(rRes, col).Style.Font.FontSize = 8;
                ws.Cell(rRes, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#e9ecef");
                ws.Cell(rRes, col + 1).Value = valor;
                ws.Cell(rRes, col + 1).Style.Font.FontSize = 8;
                if (label == "TOTAL S/:")
                {
                    ws.Cell(rRes, col + 1).Style.Font.Bold = true;
                    ws.Cell(rRes, col + 1).Style.Font.FontColor = colorAcento;
                }
                rRes++;
                if (rRes > 5) { rRes = 3; col += 2; }
            }

            // --- Encabezado tabla ---
            int rH = 9;
            var hdrs = new[] { "#", "Liquidación", "RUC", "Tipo Entidad", "Cía Médica", "Banco",
                               "Comprobante", "Estado", "Sub Total S/.", "IGV S/.", "Imp. Renta S/.", "Detracción S/.", "Total S/." };
            ws.Row(rH).Height = 28;
            for (int i = 0; i < hdrs.Length; i++)
            {
                var c = ws.Cell(rH, i + 1);
                c.Value = hdrs[i];
                c.Style.Font.Bold = true;
                c.Style.Font.FontSize = 9;
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
                c.Style.Alignment.WrapText   = true;
                c.Style.Fill.BackgroundColor = colorHeader;
                c.Style.Font.FontColor       = colorHeaderFont;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // --- Filas ---
            int row = rH + 1;
            int cnt = 1;
            foreach (var det in detalle)
            {
                var bg          = cnt % 2 == 0 ? colorAlt : XLColor.White;
                var comprobante = !string.IsNullOrEmpty(det.Serie) && !string.IsNullOrEmpty(det.Numero)
                    ? $"{det.Serie}-{det.Numero}" : "-";

                ws.Cell(row, 1).Value  = cnt;
                ws.Cell(row, 2).Value  = det.NumeroLiquidacion ?? "-";
                ws.Cell(row, 3).Value  = det.Ruc ?? "-";
                ws.Cell(row, 4).Value  = det.DesTipoEntidadMedica ?? det.TipoEntidadMedica ?? "-";
                ws.Cell(row, 5).Value  = det.RazonSocial ?? "-";
                ws.Cell(row, 6).Value  = det.NombreBanco ?? "-";
                ws.Cell(row, 7).Value  = comprobante;
                ws.Cell(row, 8).Value  = EstadoDescripcion.Produccion.GetDescripcion(det.Estado);
                ws.Cell(row, 9).Value  = det.MtoSubtotal ?? 0;
                ws.Cell(row, 10).Value = det.MtoIgv ?? 0;
                ws.Cell(row, 11).Value = det.MtoRenta ?? 0;
                ws.Cell(row, 12).Value = 0;  // MtoDetraccion — campo futuro
                ws.Cell(row, 13).Value = det.MtoTotal ?? 0;

                for (int c = 9; c <= 13; c++)
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00";

                ws.Row(row).Height = 15;
                for (int c = 1; c <= hdrs.Length; c++)
                {
                    ws.Cell(row, c).Style.Font.FontSize = 9;
                    ws.Cell(row, c).Style.Fill.BackgroundColor = bg;
                    ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(row, c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#dee2e6");
                    ws.Cell(row, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
                cnt++; row++;
            }

            // --- Fila de totales ---
            int rTot = row;
            ws.Cell(rTot, 8).Value = "TOTAL:";
            ws.Cell(rTot, 8).Style.Font.Bold = true;
            ws.Cell(rTot, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var totalCols = new[] { 9, 10, 11, 12, 13 };
            foreach (var tc in totalCols)
            {
                ws.Cell(rTot, tc).FormulaA1 = $"SUM({ws.Cell(rH + 1, tc).Address}:{ws.Cell(row - 1, tc).Address})";
                ws.Cell(rTot, tc).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(rTot, tc).Style.Font.Bold = true;
                ws.Cell(rTot, tc).Style.Fill.BackgroundColor = XLColor.FromHtml("#e9ecef");
                ws.Cell(rTot, tc).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Range(rTot, 1, rTot, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#e9ecef");
            ws.Columns().AdjustToContents(5, 60);

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var nombreArchivo = $"{ordenPago.NumeroOrdenPago ?? $"OP_{guid}"}.xlsx";
            _logger.LogInformation("Excel Orden de Pago generado en memoria: {Numero}", ordenPago.NumeroOrdenPago);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar Excel de orden de pago {Guid}", guid);
            return StatusCode(500, "Error al generar el reporte Excel.");
        }
    }

    /// <summary>
    /// Exporta el listado de ordenes de pago y el detalle de liquidaciones a Excel.
    /// Respeta los mismos filtros que la lista paginada.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-13</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportarExcel(int? idBanco, string? estado)
    {
        try
        {
            var idSede = GetCurrentUserIdSede();
            var estadoFiltro = string.IsNullOrEmpty(estado) ? EstadoDescripcion.OrdenPago.Aprobado : estado;

            var (ordenes, _) = await _ordenPagoService.GetPaginatedListAsync(idBanco, estadoFiltro, idSede, 1, 10000);
            var listaOrdenes = ordenes.ToList();

            using var workbook = new XLWorkbook();

            var colorHeader    = XLColor.FromHtml("#6c757d");
            var colorHeaderFont = XLColor.White;
            var colorAlt       = XLColor.FromHtml("#f8f9fa");

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo_login.jpg");

            // ----------------------------------------------------------------
            // HOJA 1 — Órdenes de Pago
            // ----------------------------------------------------------------
            var ws1 = workbook.Worksheets.Add("Ordenes de Pago");

            ws1.Row(1).Height = 36;
            ws1.Range(1, 1, 1, 12).Merge();
            if (System.IO.File.Exists(imagePath))
            {
                using var imgStream1 = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                ws1.AddPicture(imgStream1).MoveTo(ws1.Cell("A1")).WithSize(120, 32);
            }

            ws1.Range(2, 1, 2, 12).Merge();
            ws1.Cell(2, 1).Value = "Reporte de Tesorería - Órdenes de Pago";
            ws1.Cell(2, 1).Style.Font.Bold = true;
            ws1.Cell(2, 1).Style.Font.FontSize = 14;
            ws1.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws1.Cell(2, 1).Style.Fill.BackgroundColor = colorHeader;
            ws1.Cell(2, 1).Style.Font.FontColor = colorHeaderFont;
            ws1.Row(2).Height = 22;

            ws1.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws1.Cell(3, 1).Style.Font.Italic = true;
            ws1.Cell(3, 1).Style.Font.FontSize = 9;
            ws1.Range(3, 1, 3, 12).Merge();

            int rH1 = 5;
            var hdrs1 = new[] { "#", "N° Orden de Pago", "Sede", "Banco", "Fecha Generación", "Estado",
                                "N° Liquid.", "N° Facturas", "Sub Total S/.", "IGV S/.", "Imp. Renta S/.", "Total S/." };
            ws1.Row(rH1).Height = 28;
            for (int i = 0; i < hdrs1.Length; i++)
            {
                var c = ws1.Cell(rH1, i + 1);
                c.Value = hdrs1[i];
                c.Style.Font.Bold = true;
                c.Style.Font.FontSize = 9;
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
                c.Style.Alignment.WrapText   = true;
                c.Style.Fill.BackgroundColor = colorHeader;
                c.Style.Font.FontColor       = colorHeaderFont;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            int row1 = rH1 + 1;
            int cnt1 = 1;
            foreach (var o in listaOrdenes)
            {
                var bg = cnt1 % 2 == 0 ? colorAlt : XLColor.White;
                ws1.Cell(row1, 1).Value  = cnt1;
                ws1.Cell(row1, 2).Value  = o.NumeroOrdenPago ?? "-";
                ws1.Cell(row1, 3).Value  = o.NombreSede ?? "-";
                ws1.Cell(row1, 4).Value  = o.NombreBanco ?? "-";
                ws1.Cell(row1, 5).Value  = o.FechaGeneracion.HasValue ? o.FechaGeneracion.Value.ToString("dd/MM/yyyy") : "-";
                ws1.Cell(row1, 6).Value  = EstadoDescripcion.OrdenPago.GetDescripcion(o.Estado);
                ws1.Cell(row1, 7).Value  = o.CantLiquidaciones ?? 0;
                ws1.Cell(row1, 8).Value  = o.CantComprobantes ?? 0;
                ws1.Cell(row1, 9).Value  = o.MtoSubtotalAcum ?? 0;
                ws1.Cell(row1, 10).Value = o.MtoIgvAcum ?? 0;
                ws1.Cell(row1, 11).Value = o.MtoRentaAcum ?? 0;
                ws1.Cell(row1, 12).Value = o.MtoTotalAcum ?? 0;
                for (int c = 9; c <= 12; c++)
                    ws1.Cell(row1, c).Style.NumberFormat.Format = "#,##0.00";
                ws1.Row(row1).Height = 15;
                for (int c = 1; c <= hdrs1.Length; c++)
                {
                    ws1.Cell(row1, c).Style.Font.FontSize = 9;
                    ws1.Cell(row1, c).Style.Fill.BackgroundColor = bg;
                    ws1.Cell(row1, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws1.Cell(row1, c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#dee2e6");
                    ws1.Cell(row1, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
                cnt1++; row1++;
            }
            ws1.Columns().AdjustToContents(5, 60);

            // ----------------------------------------------------------------
            // HOJA 2 — Detalle de Liquidaciones
            // ----------------------------------------------------------------
            var ws2 = workbook.Worksheets.Add("Detalle de Liquidaciones");

            ws2.Row(1).Height = 36;
            ws2.Range(1, 1, 1, 13).Merge();
            if (System.IO.File.Exists(imagePath))
            {
                using var imgStream2 = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                ws2.AddPicture(imgStream2).MoveTo(ws2.Cell("A1")).WithSize(120, 32);
            }

            ws2.Range(2, 1, 2, 13).Merge();
            ws2.Cell(2, 1).Value = "Reporte de Tesorería - Detalle de Liquidaciones";
            ws2.Cell(2, 1).Style.Font.Bold = true;
            ws2.Cell(2, 1).Style.Font.FontSize = 14;
            ws2.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws2.Cell(2, 1).Style.Fill.BackgroundColor = colorHeader;
            ws2.Cell(2, 1).Style.Font.FontColor = colorHeaderFont;
            ws2.Row(2).Height = 22;

            ws2.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws2.Cell(3, 1).Style.Font.Italic = true;
            ws2.Cell(3, 1).Style.Font.FontSize = 9;
            ws2.Range(3, 1, 3, 13).Merge();

            int rH2 = 5;
            var hdrs2 = new[] { "#", "N° Orden de Pago", "Liquidación", "Tipo Liquidación", "Período",
                                "RUC", "Tipo Entidad", "Cía Médica", "Banco",
                                "Comprobante", "Estado", "Sub Total S/.", "IGV S/.", "Imp. Renta S/.", "Total S/." };
            ws2.Row(rH2).Height = 28;
            for (int i = 0; i < hdrs2.Length; i++)
            {
                var c = ws2.Cell(rH2, i + 1);
                c.Value = hdrs2[i];
                c.Style.Font.Bold = true;
                c.Style.Font.FontSize = 9;
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
                c.Style.Alignment.WrapText   = true;
                c.Style.Fill.BackgroundColor = colorHeader;
                c.Style.Font.FontColor       = colorHeaderFont;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            int row2 = rH2 + 1;
            int cnt2 = 1;
            foreach (var o in listaOrdenes)
            {
                var detalles = await _ordenPagoLiquidacionService.GetDetalleLiquidacionesByOrdenPagoIdAsync(o.IdOrdenPago);
                foreach (var det in detalles)
                {
                    var bg = cnt2 % 2 == 0 ? colorAlt : XLColor.White;
                    var comprobante = !string.IsNullOrEmpty(det.Serie) && !string.IsNullOrEmpty(det.Numero)
                        ? $"{det.Serie}-{det.Numero}" : "-";

                    ws2.Cell(row2, 1).Value  = cnt2;
                    ws2.Cell(row2, 2).Value  = o.NumeroOrdenPago ?? "-";
                    ws2.Cell(row2, 3).Value  = det.NumeroLiquidacion ?? "-";
                    ws2.Cell(row2, 4).Value  = det.DesTipoLiquidacion ?? det.TipoLiquidacion ?? "-";
                    ws2.Cell(row2, 5).Value  = det.PeriodoLiquidacion ?? "-";
                    ws2.Cell(row2, 6).Value  = det.Ruc ?? "-";
                    ws2.Cell(row2, 7).Value  = det.DesTipoEntidadMedica ?? det.TipoEntidadMedica ?? "-";
                    ws2.Cell(row2, 8).Value  = det.RazonSocial ?? "-";
                    ws2.Cell(row2, 9).Value  = det.NombreBanco ?? "-";
                    ws2.Cell(row2, 10).Value = comprobante;
                    ws2.Cell(row2, 11).Value = EstadoDescripcion.Produccion.GetDescripcion(det.Estado);
                    ws2.Cell(row2, 12).Value = det.MtoSubtotal ?? 0;
                    ws2.Cell(row2, 13).Value = det.MtoIgv ?? 0;
                    ws2.Cell(row2, 14).Value = det.MtoRenta ?? 0;
                    ws2.Cell(row2, 15).Value = det.MtoTotal ?? 0;
                    for (int c = 12; c <= 15; c++)
                        ws2.Cell(row2, c).Style.NumberFormat.Format = "#,##0.00";
                    ws2.Row(row2).Height = 15;
                    for (int c = 1; c <= hdrs2.Length; c++)
                    {
                        ws2.Cell(row2, c).Style.Font.FontSize = 9;
                        ws2.Cell(row2, c).Style.Fill.BackgroundColor = bg;
                        ws2.Cell(row2, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws2.Cell(row2, c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#dee2e6");
                        ws2.Cell(row2, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    cnt2++; row2++;
                }
            }
            ws2.Columns().AdjustToContents(5, 60);

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var fileName = $"Tesoreria_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar tesoreria a Excel");
            return BadRequest("Error al generar el reporte");
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
