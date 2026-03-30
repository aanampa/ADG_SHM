using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.DTOs.SanPabloApi;
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
    private readonly IEntidadCuentaBancariaService _entidadCuentaBancariaService;
    private readonly IBancoService _bancoService;
    private readonly IBitacoraService _bitacoraService;
    private readonly IConfiguration _configuration;
    private readonly ISanPabloApiService _sanPabloApiService;

    public ProduccionController(
        ILogger<ProduccionController> logger,
        IProduccionService produccionService,
        ITablaDetalleService tablaDetalleService,
        IArchivoService archivoService,
        IArchivoComprobanteService archivoComprobanteService,
        IEntidadMedicaService entidadMedicaService,
        IEntidadCuentaBancariaService entidadCuentaBancariaService,
        IBancoService bancoService,
        IBitacoraService bitacoraService,
        IConfiguration configuration,
        ISanPabloApiService sanPabloApiService)
    {
        _logger = logger;
        _produccionService = produccionService;
        _tablaDetalleService = tablaDetalleService;
        _archivoService = archivoService;
        _archivoComprobanteService = archivoComprobanteService;
        _entidadMedicaService = entidadMedicaService;
        _entidadCuentaBancariaService = entidadCuentaBancariaService;
        _bancoService = bancoService;
        _bitacoraService = bitacoraService;
        _configuration = configuration;
        _sanPabloApiService = sanPabloApiService;
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
    /// <modified>ADG Vladimir D - 2026-02-02 - Agregado filtro interno por IdSede del usuario logueado</modified>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(string? produccion, string? estado, int? idEntidadMedica, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            // Obtener IdSede del usuario logueado desde los claims
            var idSede = GetCurrentUserIdSede();

            var (items, totalCount) = await _produccionService.GetPaginatedListAsync(produccion, estado, idEntidadMedica, idSede, pageNumber, pageSize);

            var model = new ProduccionListViewModel
            {
                Items = items.Select(p => new ProduccionItemViewModel
                {
                    GuidRegistro = p.GuidRegistro ?? "",
                    CodigoProduccion = p.CodigoProduccion,
                    NumeroProduccion = p.NumeroProduccion,
                    TipoProduccion = p.TipoProduccion,
                    DesTipoProduccion = p.DesTipoProduccion,
                    DesTipoMedico = p.DesTipoMedico,
                    DesTipoRubro = p.DesTipoRubro,
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
            if (produccion.Estado != EstadoDescripcion.Produccion.FacturaPendiente && produccion.Estado != EstadoDescripcion.Produccion.FacturaSolicitada)
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

            // Cargar datos bancarios de la Cia Medica
            string? nombreBanco = null;
            string? cuentaCorriente = null;
            string? cuentaCci = null;
            string? moneda = null;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var cuentasBancarias = await _entidadCuentaBancariaService
                    .GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
                var cuentaBancaria = cuentasBancarias.FirstOrDefault(c => c.Activo == 1);

                if (cuentaBancaria != null)
                {
                    cuentaCorriente = cuentaBancaria.CuentaCorriente;
                    cuentaCci = cuentaBancaria.CuentaCci;
                    moneda = cuentaBancaria.Moneda;

                    if (cuentaBancaria.IdBanco.HasValue)
                    {
                        var banco = await _bancoService.GetBancoByIdAsync(cuentaBancaria.IdBanco.Value);
                        nombreBanco = banco?.NombreBanco;
                    }
                }
            }

            ViewBag.NombreBanco = nombreBanco;
            ViewBag.CuentaCorriente = cuentaCorriente;
            ViewBag.CuentaCci = cuentaCci;
            ViewBag.Moneda = moneda;

            // Cargar bitacora de la produccion
            var bitacoras = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_PRODUCCION", produccion.IdProduccion);
            ViewBag.Bitacoras = bitacoras.ToList();

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
                // Registrar en Bitacora
                var produccion = await _produccionService.GetProduccionByGuidAsync(solicitud.GuidRegistro);
                if (produccion != null)
                {
                    // Formatear fecha limite para la descripcion (DD/MM/YYYY HH:mm)
                    var fechaFormateada = solicitud.Fecha;
                    if (DateTime.TryParse($"{solicitud.Fecha} {solicitud.Hora}", out DateTime fechaLimite))
                    {
                        fechaFormateada = fechaLimite.ToString("dd/MM/yyyy");
                    }

                    var bitacoraDto = new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                    {
                        Entidad = "SHM_PRODUCCION",
                        IdEntidad = produccion.IdProduccion,
                        Accion = EstadoDescripcion.Produccion.FacturaSolicitada,
                        Descripcion = $"Factura Solicitada con fecha limite el {fechaFormateada} a las {solicitud.Hora} horas",
                        FechaAccion = DateTime.Now
                    };
                    await _bitacoraService.CreateBitacoraAsync(bitacoraDto, idUsuario);
                }

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
                // Registrar en Bitacora
                var produccion = await _produccionService.GetProduccionByGuidAsync(request.GuidRegistro);
                if (produccion != null)
                {
                    var comprobante = produccion.ComprobanteFactura ?? $"{produccion.Serie}-{produccion.Numero}";
                    var bitacoraDto = new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                    {
                        Entidad = "SHM_PRODUCCION",
                        IdEntidad = produccion.IdProduccion,
                        Accion = EstadoDescripcion.Produccion.FacturaDevuelta,
                        Descripcion = $"Se devolvio el comprobante de pago electrónico: {comprobante} para su subsanacion",
                        FechaAccion = DateTime.Now
                    };
                    await _bitacoraService.CreateBitacoraAsync(bitacoraDto, idUsuario);
                }

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
                // Registrar en Bitacora
                var produccion = await _produccionService.GetProduccionByGuidAsync(request.GuidRegistro);
                if (produccion != null)
                {
                    var comprobante = produccion.ComprobanteFactura ?? $"{produccion.Serie}-{produccion.Numero}";
                    var bitacoraDto = new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                    {
                        Entidad = "SHM_PRODUCCION",
                        IdEntidad = produccion.IdProduccion,
                        Accion = EstadoDescripcion.Produccion.FacturaAceptada,
                        Descripcion = $"Se acepto el comprobante de pago electrónico: {comprobante}",
                        FechaAccion = DateTime.Now
                    };
                    await _bitacoraService.CreateBitacoraAsync(bitacoraDto, idUsuario);
                }

                // Invocar San Pablo API y transicionar a FACTURA_ENVIADA_HHMM
                if (produccion != null)
                {
                    var errorHhmm = await RegistrarComprobanteEnSanPabloAsync(produccion, idUsuario);
                    if (errorHhmm != null)
                    {
                        _logger.LogWarning("Factura aceptada pero error al enviar a HHMM. GUID: {Guid}, Error: {Error}", request.GuidRegistro, errorHhmm);
                        return Json(new { success = false, message = $"Error al enviar el comprobante a HHMM: {errorHhmm}" });
                    }
                }

                _logger.LogInformation("Factura aceptada y enviada a HHMM. GUID: {Guid}, Usuario: {Usuario}",
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

    // /// <summary>
    // /// Redirige la descarga de archivos al controlador centralizado ArchivoController.
    // /// Las vistas ahora invocan directamente a Archivo/Descargar.
    // /// </summary>
    // [HttpGet]
    // public IActionResult DescargarArchivo(string guid)
    // {
    //     return RedirectToAction("Descargar", "Archivo", new { guid });
    // }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("IdUsuario");
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int idUsuario))
        {
            return idUsuario;
        }
        return 0;
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

    /// <summary>
    /// Registra el comprobante en el API de San Pablo y transiciona a FACTURA_ENVIADA_HHMM.
    /// Retorna null si fue exitoso, o el mensaje de error si fallo.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-03-29</created>
    /// </summary>
    private async Task<string?> RegistrarComprobanteEnSanPabloAsync(
        ProduccionListaResponseDto produccion,
        int userId)
    {
        try
        {
            // Obtener codigo de entidad medica
            string? codigoEntidad = null;
            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var entidadMedica = await _entidadMedicaService.GetEntidadMedicaByIdAsync(produccion.IdEntidadMedica.Value);
                codigoEntidad = entidadMedica?.CodigoEntidad;
            }

            // Derivar tipo comprobante si es nulo
            var tipoComprobante = produccion.TipoComprobante;
            if (string.IsNullOrEmpty(tipoComprobante))
                tipoComprobante = produccion.TipoEntidadMedica == "1" ? "1" : "22";

            // Derivar glosa/concepto si es nulo
            var glosa = produccion.Concepto;
            if (string.IsNullOrEmpty(glosa))
            {
                var detalleTipoProd = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_PRODUCCION", produccion.TipoProduccion ?? "");
                var descripcionTipoProd = detalleTipoProd?.Descripcion?.ToUpper() ?? produccion.TipoProduccion ?? "";
                glosa = $"PRODUCCION {produccion.CodigoProduccion} - {descripcionTipoProd}";
            }

            // Obtener descripcion del tipo de comprobante
            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_COMPROBANTE", tipoComprobante);
            var descripcionTipo = tablaDetalle?.Descripcion ?? tipoComprobante;

            // FLG_CIAMEDICA: 1=CIA MEDICA, 0=MEDICO
            var flgCiaMedica = produccion.TipoEntidadMedica == "1" ? "1" : "0";

            // Formatear numero a 7 digitos
            var numero = produccion.Numero ?? "";
            if (int.TryParse(numero, out var numInt))
                numero = numInt.ToString("D7");

            var request = new SanPabloComprobanteRequestDto
            {
                COD_SEDE = produccion.CodigoSede,
                FLG_CIAMEDICA = flgCiaMedica,
                COD_ENTIDAD = codigoEntidad,
                COD_PROD = produccion.CodigoProduccion,
                FLG_PORTAL = "FA",
                CPM_TIPO = tipoComprobante,
                CPM_SERIE = produccion.Serie,
                CPM_NUMERO = numero,
                CPM_FECEMI = produccion.FechaEmision?.ToString("dd/MM/yyyy"),
                CPM_GLOSA = glosa,
                CPM_MTOTAL = produccion.MtoTotal?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                CPM_FECREG = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            var response = await _sanPabloApiService.RegistrarComprobanteAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Comprobante registrado en San Pablo. CodigoProduccion: {Cod}, Tipo: {Tipo}, Serie: {Serie}, Numero: {Numero}",
                    produccion.CodigoProduccion, descripcionTipo, produccion.Serie, produccion.Numero);

                // Cambiar estado a FACTURA_ENVIADA_HHMM
                await _produccionService.EnviarAHhmmAsync(produccion.GuidRegistro!, userId);

                // Registrar en bitacora
                await _bitacoraService.CreateBitacoraAsync(new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                {
                    Entidad = "SHM_PRODUCCION",
                    IdEntidad = produccion.IdProduccion,
                    Accion = EstadoDescripcion.Produccion.FacturaEnviadaHhmm,
                    Descripcion = $"{descripcionTipo} enviada a HHMM: {produccion.Serie}-{produccion.Numero}",
                    FechaAccion = DateTime.Now
                }, userId);

                return null; // Exito
            }
            else
            {
                _logger.LogWarning("Error al registrar comprobante en San Pablo. CodigoProduccion: {Cod}, Mensaje: {Msg}",
                    produccion.CodigoProduccion, response.Message);

                // Revertir estado a FACTURA_ENVIADA (sin limpiar datos del comprobante)
                await _produccionService.UpdateEstadoAsync(
                    produccion.GuidRegistro!,
                    EstadoDescripcion.Produccion.FacturaEnviada,
                    userId);

                // Registrar error en bitacora
                await _bitacoraService.CreateBitacoraAsync(new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                {
                    Entidad = "SHM_PRODUCCION",
                    IdEntidad = produccion.IdProduccion,
                    Accion = "ERROR_ENVIO_HHMM",
                    Descripcion = $"Error al enviar {descripcionTipo} a HHMM: {response.Message}. Estado revertido a FACTURA_ENVIADA.",
                    FechaAccion = DateTime.Now
                }, userId);

                return !string.IsNullOrWhiteSpace(response.Message)
                    ? response.Message
                    : "No se pudo enviar el comprobante";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comunicar comprobante a San Pablo. CodigoProduccion: {Cod}", produccion.CodigoProduccion);
            return ex.Message;
        }
    }
}
