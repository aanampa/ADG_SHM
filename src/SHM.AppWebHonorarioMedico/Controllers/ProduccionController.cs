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
    private readonly IArchivoService _archivoService;
    private readonly IArchivoComprobanteService _archivoComprobanteService;
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly IConfiguration _configuration;

    public ProduccionController(
        ILogger<ProduccionController> logger,
        IProduccionService produccionService,
        ITablaDetalleService tablaDetalleService,
        IArchivoService archivoService,
        IArchivoComprobanteService archivoComprobanteService,
        IEntidadMedicaService entidadMedicaService,
        IConfiguration configuration)
    {
        _logger = logger;
        _produccionService = produccionService;
        _tablaDetalleService = tablaDetalleService;
        _archivoService = archivoService;
        _archivoComprobanteService = archivoComprobanteService;
        _entidadMedicaService = entidadMedicaService;
        _configuration = configuration;
    }

    /// <summary>
    /// Vista principal de la bandeja de producciones.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// <modified>ADG Vladimir D - 2026-01-24 - Agregado carga de Cias Medicas para filtro</modified>
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

        // Cargar Cias Medicas para el filtro Select2
        var ciasMedicas = await _entidadMedicaService.GetAllEntidadesMedicasAsync();
        ViewBag.CiasMedicas = ciasMedicas
            .Where(c => c.Activo == 1)
            .OrderBy(c => c.RazonSocial)
            .Select(c => new { id = c.IdEntidadMedica, text = c.RazonSocial })
            .ToList();

        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de producciones (AJAX).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// <modified>ADG Vladimir D - 2026-01-24 - Agregado filtro por codigo de produccion</modified>
    /// <modified>ADG Vladimir D - 2026-01-24 - Agregado filtro por Cia Medica</modified>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(string? produccion, string? estado, int? idEntidadMedica, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _produccionService.GetPaginatedListAsync(produccion, estado, idEntidadMedica, pageNumber, pageSize);

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
                    FechaLimite = p.FechaLimite,
                    Activo = p.Activo
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Estado = estado
            };

            _logger.LogInformation("Listando producciones. Total: {Total}, Pagina: {Page}, Produccion: {Produccion}, Estado: {Estado}, CiaMedica: {CiaMedica}",
                totalCount, pageNumber, produccion ?? "Todos", estado ?? "Todos", idEntidadMedica?.ToString() ?? "Todas");

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
    /// <modified>ADG Vladimir D - 2025-01-22 - Agregado carga de archivos adjuntos</modified>
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

            // Cargar archivos adjuntos para estados diferentes a FACTURA_PENDIENTE y FACTURA_SOLICITADA
            var archivos = new List<ArchivoAdjuntoViewModel>();
            if (produccion.Estado != "FACTURA_PENDIENTE" && produccion.Estado != "FACTURA_SOLICITADA")
            {
                var archivosComprobante = await _archivoComprobanteService.GetArchivoComprobantesByProduccionAsync(produccion.IdProduccion);
                foreach (var ac in archivosComprobante.Where(a => a.Activo == 1))
                {
                    if (ac.IdArchivo.HasValue)
                    {
                        var archivo = await _archivoService.GetArchivoByIdAsync(ac.IdArchivo.Value);
                        if (archivo != null && archivo.Activo == 1)
                        {
                            archivos.Add(new ArchivoAdjuntoViewModel
                            {
                                GuidRegistro = archivo.GuidRegistro,
                                TipoArchivo = ac.TipoArchivo ?? archivo.TipoArchivo,
                                NombreArchivo = archivo.NombreArchivo,
                                NombreOriginal = archivo.NombreOriginal,
                                Extension = archivo.Extension,
                                Tamano = archivo.Tamano,
                                FechaCreacion = archivo.FechaCreacion
                            });
                        }
                    }
                }
            }

            ViewBag.Archivos = archivos;
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

    /// <summary>
    /// Devuelve una factura cambiando el estado a FACTURA_DEVUELTA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-22</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DevolverFactura([FromBody] CambioEstadoFacturaDto request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.GuidRegistro))
            {
                return Json(new { success = false, message = "Datos de solicitud invalidos" });
            }

            var idUsuario = GetCurrentUserId();
            var resultado = await _produccionService.DevolverFacturaAsync(request.GuidRegistro, idUsuario);

            if (resultado)
            {
                _logger.LogInformation("Factura devuelta. GUID: {Guid}, Usuario: {Usuario}",
                    request.GuidRegistro, idUsuario);
                return Json(new { success = true, message = "Factura devuelta correctamente" });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo devolver la factura" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al devolver factura: {Guid}", request?.GuidRegistro);
            return Json(new { success = false, message = "Error al procesar la solicitud" });
        }
    }

    /// <summary>
    /// Acepta una factura cambiando el estado a FACTURA_ACEPTADA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-22</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AceptarFactura([FromBody] CambioEstadoFacturaDto request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.GuidRegistro))
            {
                return Json(new { success = false, message = "Datos de solicitud invalidos" });
            }

            var idUsuario = GetCurrentUserId();
            var resultado = await _produccionService.AceptarFacturaAsync(request.GuidRegistro, idUsuario);

            if (resultado)
            {
                _logger.LogInformation("Factura aceptada. GUID: {Guid}, Usuario: {Usuario}",
                    request.GuidRegistro, idUsuario);
                return Json(new { success = true, message = "Factura aceptada correctamente" });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo aceptar la factura" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aceptar factura: {Guid}", request?.GuidRegistro);
            return Json(new { success = false, message = "Error al procesar la solicitud" });
        }
    }

    /// <summary>
    /// Descarga un archivo adjunto de comprobante.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-22</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DescargarArchivo(string guid)
    {
        try
        {
            if (string.IsNullOrEmpty(guid))
            {
                return NotFound("Archivo no encontrado");
            }

            var archivo = await _archivoService.GetArchivoByGuidAsync(guid);
            if (archivo == null || archivo.Activo != 1)
            {
                return NotFound("Archivo no encontrado");
            }

            // Obtener ruta base de archivos desde configuracion
            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadBasePath, archivo.Ruta ?? "");

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Archivo fisico no encontrado: {FilePath}", filePath);
                return NotFound("Archivo fisico no encontrado");
            }

            // Determinar content type
            var contentType = archivo.Extension?.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            // Para PDFs, mostrar inline en el navegador (visor embebido)
            // Para otros archivos, forzar descarga
            if (archivo.Extension?.ToLower() == ".pdf")
            {
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{archivo.NombreArchivo}\"");
                return File(fileBytes, contentType);
            }

            return File(fileBytes, contentType, archivo.NombreArchivo ?? $"archivo{archivo.Extension}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo con GUID: {Guid}", guid);
            return StatusCode(500, "Error al descargar el archivo");
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
