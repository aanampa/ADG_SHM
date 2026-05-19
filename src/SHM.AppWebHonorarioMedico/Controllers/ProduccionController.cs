using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.DTOs.SanPabloApi;
using SHM.AppDomain.Interfaces.Repositories;
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
    private readonly IUsuarioService _usuarioService;
    private readonly IEmailLogRepository _emailLogRepository;

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
        ISanPabloApiService sanPabloApiService,
        IUsuarioService usuarioService,
        IEmailLogRepository emailLogRepository)
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
        _usuarioService = usuarioService;
        _emailLogRepository = emailLogRepository;
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
                    FacturaFechaVencimiento = p.FacturaFechaVencimiento,
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
            int cantCuentasActivas = 0;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var cuentasBancarias = await _entidadCuentaBancariaService
                    .GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
                var cuentasActivas = cuentasBancarias.Where(c => c.Activo == 1).ToList();
                cantCuentasActivas = cuentasActivas.Count;

                if (cantCuentasActivas == 1)
                {
                    var cuentaBancaria = cuentasActivas[0];
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
            ViewBag.CantCuentasActivas = cantCuentasActivas;

            // Cargar bitacora de la produccion
            var bitacoras = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_PRODUCCION", produccion.IdProduccion);
            ViewBag.Bitacoras = bitacoras.ToList();

            // Verificar si la entidad medica tiene usuarios externos activos (para habilitar Solicitar Factura)
            var tieneUsuariosExternos = false;
            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var usuariosEntidad = await _usuarioService.GetUsuariosByEntidadMedicaAsync(produccion.IdEntidadMedica.Value);
                tieneUsuariosExternos = usuariosEntidad.Any(u => u.TipoUsuario == "E");
            }
            ViewBag.TieneUsuariosExternos = tieneUsuariosExternos;

            return View(produccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de produccion: {Guid}", guid);
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Retorna el modal de Solicitud de Factura (FACTURA_PENDIENTE).
    /// Con guidRegistro: filtra a un solo registro (modo individual desde el listado).
    /// Sin guidRegistro: carga todos los FACTURA_PENDIENTE de la sede.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-26</created>
    /// <modified>ADG Vladimir D - 2026-04-26 - Soporte modo individual por guidRegistro</modified>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetModalSolicitudFactura(string? guidRegistro = null)
    {
        try
        {
            IEnumerable<ProduccionListaResponseDto> allItems;

            if (!string.IsNullOrEmpty(guidRegistro))
            {
                var item = await _produccionService.GetSolicitudMasivaByGuidAsync(guidRegistro);
                allItems = item != null ? new[] { item } : Array.Empty<ProduccionListaResponseDto>();
            }
            else
            {
                var idSede = GetCurrentUserIdSede();
                var lista  = await _produccionService.GetListSolicitudMasivaAsync(idSede);
                allItems   = lista.Where(p => p.Estado == AppDomain.Constants.EstadoDescripcion.Produccion.FacturaPendiente);
            }

            // Solo FACTURA_PENDIENTE
            var viewItems = allItems
                .Where(p => p.Estado == AppDomain.Constants.EstadoDescripcion.Produccion.FacturaPendiente)
                .Select(item =>
                {
                    bool habilitado = true;
                    string? motivo = null;
                    if (!item.IdEntidadMedica.HasValue)
                    { habilitado = false; motivo = "Sin compañía médica asignada"; }
                    else if (item.NroUsuariosExternos == 0)
                    { habilitado = false; motivo = "Sin usuarios externos"; }
                    else if (item.NroCuentasBancarias == 0)
                    { habilitado = false; motivo = "Sin cuenta bancaria activa"; }
                    else if (item.NroCuentasBancarias > 1)
                    { habilitado = false; motivo = $"Tiene {item.NroCuentasBancarias} cuentas bancarias activas"; }

                    return new Models.SolicitudMasivaItemViewModel
                    {
                        GuidRegistro        = item.GuidRegistro ?? string.Empty,
                        NumeroProduccion    = item.NumeroProduccion,
                        DesTipoProduccion   = item.DesTipoProduccion,
                        DesTipoMedico       = item.DesTipoMedico,
                        RazonSocial         = item.RazonSocial,
                        Periodo             = item.Periodo,
                        MtoTotal            = item.MtoTotal,
                        Estado              = item.Estado,
                        DesEstado           = item.DesEstado,
                        Habilitado          = habilitado,
                        MotivoDeshabilitado = motivo,
                        FechaLimite         = item.FechaLimite,
                        FechaVencimiento    = item.FacturaFechaVencimiento
                    };
                }).ToList();

            ViewBag.GuidPreseleccionado = guidRegistro;
            return PartialView("_SolicitudFacturaModal", viewItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar modal de solicitud de factura");
            return StatusCode(500, "Error al cargar el modal");
        }
    }

    /// <summary>
    /// Graba fechas límite y vencimiento en producciones FACTURA_PENDIENTE
    /// sin cambiar el estado. Corresponde al Paso 1 del flujo de solicitud.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-26</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GrabarFechasProduccion([FromBody] GrabarFechasProduccionDto dto)
    {
        try
        {
            if (dto == null || dto.Guids == null || !dto.Guids.Any())
                return Json(new { success = false, message = "Debe seleccionar al menos un registro" });

            if (string.IsNullOrEmpty(dto.Fecha) || string.IsNullOrEmpty(dto.Hora))
                return Json(new { success = false, message = "La fecha y hora límite son requeridas" });

            if (string.IsNullOrEmpty(dto.FechaVencimiento))
                return Json(new { success = false, message = "La fecha de vencimiento es requerida" });

            if (!DateTime.TryParse($"{dto.Fecha}T{dto.Hora}", out var fechaLimite) || fechaLimite <= DateTime.Now)
                return Json(new { success = false, message = "La fecha y hora límite debe ser mayor a la actual" });

            if (!DateTime.TryParse(dto.FechaVencimiento, out var fechaVencimiento))
                return Json(new { success = false, message = "La fecha de vencimiento no tiene formato válido" });

            var idUsuario = GetCurrentUserId();
            int actualizados = 0;
            int errores = 0;

            foreach (var guid in dto.Guids)
            {
                var ok = await _produccionService.GrabarFechasProduccionAsync(guid, fechaLimite, fechaVencimiento, idUsuario);
                if (ok) actualizados++; else errores++;
            }

            return Json(new
            {
                success = errores == 0,
                message = errores == 0
                    ? $"Fechas grabadas en {actualizados} registro(s) correctamente"
                    : $"Se grabaron {actualizados} registro(s). {errores} no pudieron actualizarse.",
                actualizados,
                errores
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al grabar fechas de producción");
            return Json(new { success = false, message = "Error al procesar la solicitud" });
        }
    }

    /// <summary>
    /// Procesa la solicitud masiva de facturas para una lista de GUIDs.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-03</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SolicitarFacturaMasivo([FromBody] SolicitudMasivaRequestDto solicitud)
    {
        try
        {
            if (solicitud == null || solicitud.Guids == null || !solicitud.Guids.Any())
                return Json(new { success = false, message = "Debe seleccionar al menos un registro" });

            if (string.IsNullOrEmpty(solicitud.Fecha) || string.IsNullOrEmpty(solicitud.Hora))
                return Json(new { success = false, message = "La fecha y hora límite son requeridas" });

            if (string.IsNullOrEmpty(solicitud.FechaVencimiento))
                return Json(new { success = false, message = "La fecha de vencimiento de factura es requerida" });

            var idUsuario = GetCurrentUserId();
            int enviados = 0;
            int errores = 0;
            var detalleErrores = new List<string>();

            foreach (var guid in solicitud.Guids)
            {
                try
                {
                    var dto = new AppDomain.DTOs.Produccion.SolicitarFacturaDto
                    {
                        GuidRegistro     = guid,
                        Fecha            = solicitud.Fecha,
                        Hora             = solicitud.Hora,
                        FechaVencimiento = solicitud.FechaVencimiento
                    };

                    var resultado = await _produccionService.SolicitarFacturaAsync(dto, idUsuario);
                    if (resultado)
                    {
                        var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
                        if (produccion != null)
                        {
                            var bitacoraDto = new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                            {
                                Entidad = "SHM_PRODUCCION",
                                IdEntidad = produccion.IdProduccion,
                                Accion = EstadoDescripcion.Produccion.FacturaSolicitada,
                                Descripcion = $"Factura Solicitada (masivo) con fecha limite el {solicitud.Fecha} a las {solicitud.Hora} horas",
                                FechaAccion = DateTime.Now
                            };
                            await _bitacoraService.CreateBitacoraAsync(bitacoraDto, idUsuario);
                        }
                        enviados++;
                    }
                    else
                    {
                        errores++;
                        detalleErrores.Add(guid);
                    }
                }
                catch (Exception exItem)
                {
                    _logger.LogError(exItem, "Error al solicitar factura masiva para GUID: {Guid}", guid);
                    errores++;
                    detalleErrores.Add(guid);
                }
            }

            return Json(new
            {
                success = true,
                enviados,
                errores,
                message = errores == 0
                    ? $"Se procesaron {enviados} solicitud(es) correctamente."
                    : $"Se procesaron {enviados} correctamente y {errores} con error."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en solicitud masiva de facturas");
            return Json(new { success = false, message = "Error al procesar la solicitud masiva" });
        }
    }

    /// <summary>
    /// Retorna el modal con el historial de notificaciones de correo para una produccion.
    /// Muestra los registros de SHM_EMAIL_LOG tipo SOLICITUD_FACTURA asociados al ID de produccion.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-17</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetModalHistorialNotificaciones(string guidRegistro)
    {
        try
        {
            var produccion = await _produccionService.GetProduccionByGuidAsync(guidRegistro);
            if (produccion == null)
                return StatusCode(404, "Produccion no encontrada");

            var logs = await _emailLogRepository.GetByReferenciaAsync(
                "SHM_PRODUCCION", produccion.IdProduccion);

            ViewBag.GuidRegistro      = guidRegistro;
            ViewBag.NumeroProduccion  = produccion.NumeroProduccion;
            ViewBag.FechaLimite       = produccion.FechaLimite?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            ViewBag.PuedeRenotificar  = produccion.Estado == EstadoDescripcion.Produccion.FacturaSolicitada;

            return PartialView("_HistorialNotificacionesModal", logs.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar historial de notificaciones: {Guid}", guidRegistro);
            return StatusCode(500, "Error al cargar el historial");
        }
    }

    /// <summary>
    /// Renotifica la solicitud de factura reenviando el correo con la fecha limite ya establecida.
    /// Solo aplica a registros en estado FACTURA_SOLICITADA.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-17</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RenotificarFactura([FromBody] string guidRegistro)
    {
        try
        {
            if (string.IsNullOrEmpty(guidRegistro))
                return Json(new { success = false, message = "GUID invalido" });

            var idUsuario = GetCurrentUserId();
            var resultado = await _produccionService.RenotificarFacturaAsync(guidRegistro, idUsuario);

            if (resultado)
            {
                var produccion = await _produccionService.GetProduccionByGuidAsync(guidRegistro);
                if (produccion != null)
                {
                    var bitacoraDto = new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                    {
                        Entidad = "SHM_PRODUCCION",
                        IdEntidad = produccion.IdProduccion,
                        Accion = "RENOTIFICAR_FACTURA",
                        Descripcion = "Renotificacion de solicitud de factura enviada por correo",
                        FechaAccion = DateTime.Now
                    };
                    await _bitacoraService.CreateBitacoraAsync(bitacoraDto, idUsuario);
                }

                _logger.LogInformation("Renotificacion enviada. GUID: {Guid}, Usuario: {Usuario}", guidRegistro, idUsuario);
                return Json(new { success = true, message = "Notificacion reenviada correctamente" });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo enviar la notificacion. Verifique que el registro este en estado Factura Solicitada y tenga fecha limite asignada." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renotificar factura: {Guid}", guidRegistro);
            return Json(new { success = false, message = "Error al procesar la renotificacion" });
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

            // Leer comprobante ANTES de devolver porque DevolverFacturaAsync limpia Serie/Numero
            var produccionPrevia = await _produccionService.GetProduccionByGuidAsync(request.GuidRegistro);
            var comprobantePrevia = produccionPrevia?.ComprobanteFactura
                ?? (!string.IsNullOrEmpty(produccionPrevia?.Serie) && !string.IsNullOrEmpty(produccionPrevia?.Numero)
                    ? $"{produccionPrevia.Serie}-{produccionPrevia.Numero}"
                    : "-");

            var resultado = await _produccionService.DevolverFacturaAsync(request.GuidRegistro, idUsuario);

            if (resultado)
            {
                // Registrar en Bitacora
                var produccion = await _produccionService.GetProduccionByGuidAsync(request.GuidRegistro);
                if (produccion != null)
                {
                    var comprobante = comprobantePrevia;
                    var motivo = !string.IsNullOrWhiteSpace(request.MotivoDevolucion)
                        ? $" Motivo: {request.MotivoDevolucion.Trim()}"
                        : "";
                    var bitacoraDto = new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                    {
                        Entidad = "SHM_PRODUCCION",
                        IdEntidad = produccion.IdProduccion,
                        Accion = EstadoDescripcion.Produccion.FacturaDevuelta,
                        Descripcion = $"Se devolvio el comprobante de pago electrónico: {comprobante} para su subsanacion.{motivo}",
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

    /// <summary>
    /// Exporta el listado de producciones a un archivo Excel (.xlsx).
    /// Respeta los mismos filtros y filtro de sede que la lista paginada.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-04</created>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportarExcel(string? produccion, string? estado, int? idEntidadMedica)
    {
        try
        {
            var idSede = GetCurrentUserIdSede();
            var (items, _) = await _produccionService.GetPaginatedListAsync(
                produccion, estado, idEntidadMedica, idSede, 1, 10000);

            // Cargar catalogo TIPO_COMPROBANTE para lookup
            var tiposComprobante = await _tablaDetalleService.ListarPorCodigoTablaAsync("TIPO_COMPROBANTE");
            var dicTipoComprobante = tiposComprobante.ToDictionary(t => t.Codigo ?? "", t => t.Descripcion ?? "-");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Producciones");

            // --- Colores corporativos ---
            var colorHeader = XLColor.FromHtml("#6c757d");
            var colorHeaderFont = XLColor.White;
            var colorAlt = XLColor.FromHtml("#f8f9fa");

            // --- Logo (fila 1) ---
            ws.Row(1).Height = 36;
            ws.Range(1, 1, 1, 26).Merge();
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo_login.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                var picture = ws.AddPicture(imageStream).MoveTo(ws.Cell("A1"));
                picture.WithSize(120, 32);
            }

            // --- Título (fila 2) ---
            ws.Range(2, 1, 2, 26).Merge();
            ws.Cell(2, 1).Value = "Reporte de Producciones";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 1).Style.Fill.BackgroundColor = colorHeader;
            ws.Cell(2, 1).Style.Font.FontColor = colorHeaderFont;
            ws.Row(2).Height = 22;

            // --- Fecha de generación (fila 3) ---
            ws.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Cell(3, 1).Style.Font.Italic = true;
            ws.Cell(3, 1).Style.Font.FontSize = 9;
            ws.Range(3, 1, 3, 26).Merge();

            // --- Encabezados (fila 5) ---
            int rowHeader = 5;
            var headers = new[]
            {
                "#", "ID", "Cód. Producción", "Tipo Producción", "Tipo Médico", "Tipo Rubro",
                "Sede", "Periodo", "Descripción",
                "RUC", "Razón Social", "Tipo Entidad",
                "Consumo", "Descuento", "Sub Total", "IGV", "Renta", "Total",
                "Tipo Comprobante", "Serie-Número", "Fecha Emisión",
                "Estado", "Fecha Límite", "Concepto", "Glosa",
                "F. Solicitud"
            };
            ws.Row(rowHeader).Height = 28;
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(rowHeader, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 9;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Fill.BackgroundColor = colorHeader;
                cell.Style.Font.FontColor = colorHeaderFont;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // --- Datos ---
            int rowIndex = rowHeader + 1;
            int count = 1;
            foreach (var item in items)
            {
                bool isAlt = count % 2 == 0;
                var bgColor = isAlt ? colorAlt : XLColor.White;

                ws.Cell(rowIndex, 1).Value = count;
                ws.Cell(rowIndex, 2).Value = item.IdProduccion;
                ws.Cell(rowIndex, 3).Value = item.NumeroProduccion ?? "-";
                ws.Cell(rowIndex, 4).Value = item.DesTipoProduccion ?? item.TipoProduccion ?? "-";
                ws.Cell(rowIndex, 5).Value = item.DesTipoMedico ?? item.TipoMedico ?? "-";
                ws.Cell(rowIndex, 6).Value = item.DesTipoRubro ?? item.TipoRubro ?? "-";
                ws.Cell(rowIndex, 7).Value = item.NombreSede ?? "-";
                ws.Cell(rowIndex, 8).Value = item.Periodo ?? "-";
                ws.Cell(rowIndex, 9).Value = item.Descripcion ?? "-";
                ws.Cell(rowIndex, 10).Value = item.Ruc ?? "-";
                ws.Cell(rowIndex, 11).Value = item.RazonSocial ?? "-";
                ws.Cell(rowIndex, 12).Value = item.DesTipoEntidadMedica ?? item.TipoEntidadMedica ?? "-";

                // Montos — formato numérico
                ws.Cell(rowIndex, 13).Value = item.MtoConsumo ?? 0;
                ws.Cell(rowIndex, 14).Value = item.MtoDescuento ?? 0;
                ws.Cell(rowIndex, 15).Value = item.MtoSubtotal ?? 0;
                ws.Cell(rowIndex, 16).Value = item.MtoIgv ?? 0;
                ws.Cell(rowIndex, 17).Value = item.MtoRenta ?? 0;
                ws.Cell(rowIndex, 18).Value = item.MtoTotal ?? 0;
                for (int col = 13; col <= 18; col++)
                    ws.Cell(rowIndex, col).Style.NumberFormat.Format = "#,##0.00";

                // Tipo Comprobante: lookup en catalogo
                var codComp = item.TipoComprobante ?? "";
                ws.Cell(rowIndex, 19).Value = dicTipoComprobante.TryGetValue(codComp, out var desComp) ? desComp : (codComp == "" ? "-" : codComp);
                ws.Cell(rowIndex, 20).Value = item.ComprobanteFactura ?? "-";
                ws.Cell(rowIndex, 21).Value = item.FechaEmision.HasValue ? item.FechaEmision.Value.ToString("dd/MM/yyyy") : "-";
                ws.Cell(rowIndex, 22).Value = item.DesEstado ?? item.Estado ?? "-";
                ws.Cell(rowIndex, 23).Value = item.FechaLimite.HasValue ? item.FechaLimite.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                ws.Cell(rowIndex, 24).Value = item.Concepto ?? "-";
                ws.Cell(rowIndex, 25).Value = item.Glosa ?? "-";
                ws.Cell(rowIndex, 26).Value = item.FacturaFechaSolicitud.HasValue ? item.FacturaFechaSolicitud.Value.ToString("dd/MM/yyyy HH:mm") : "-";

                // Estilo de fila
                ws.Row(rowIndex).Height = 15;
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = ws.Cell(rowIndex, col);
                    cell.Style.Font.FontSize = 9;
                    cell.Style.Fill.BackgroundColor = bgColor;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#dee2e6");
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                count++;
                rowIndex++;
            }

            // --- Ajustar anchos ---
            ws.Columns().AdjustToContents(5, 60); // min 5, max 60 chars

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var fileName = $"Producciones_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar producciones a Excel");
            return BadRequest("Error al generar el reporte");
        }
    }

    /// <summary>
    /// Retorna el modal de ReNotificar Factura (FACTURA_SOLICITADA / FACTURA_DEVUELTA).
    /// Con guidRegistro: filtra a un solo registro (modo individual desde Detalle).
    /// Sin guidRegistro: carga todos los SOLICITADA/DEVUELTA de la sede (modo masivo desde Index).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-26</created>
    /// <modified>ADG Vladimir D - 2026-04-27 - Soporte modo individual por guidRegistro</modified>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetModalReNotificarFactura(string? guidRegistro = null)
    {
        try
        {
            IEnumerable<ProduccionListaResponseDto> allItems;

            if (!string.IsNullOrEmpty(guidRegistro))
            {
                var item = await _produccionService.GetSolicitudMasivaByGuidAsync(guidRegistro);
                allItems = item != null ? new[] { item } : Array.Empty<ProduccionListaResponseDto>();
            }
            else
            {
                var idSede = GetCurrentUserIdSede();
                var lista  = await _produccionService.GetListSolicitudMasivaAsync(idSede);
                allItems   = lista.Where(p =>
                    p.Estado == EstadoDescripcion.Produccion.FacturaSolicitada ||
                    p.Estado == EstadoDescripcion.Produccion.FacturaDevuelta);
            }

            allItems = allItems.Where(p =>
                p.Estado == EstadoDescripcion.Produccion.FacturaSolicitada ||
                p.Estado == EstadoDescripcion.Produccion.FacturaDevuelta);

            var viewItems = allItems.Select(item =>
            {
                bool habilitado = true;
                string? motivo  = null;

                if (!item.FechaLimite.HasValue)
                { habilitado = false; motivo = "Sin fecha límite asignada"; }
                else if (!item.IdEntidadMedica.HasValue)
                { habilitado = false; motivo = "Sin compañía médica asignada"; }
                else if (item.NroUsuariosExternos == 0)
                { habilitado = false; motivo = "Sin usuarios externos"; }
                else if (item.NroCuentasBancarias == 0)
                { habilitado = false; motivo = "Sin cuenta bancaria activa"; }
                else if (item.NroCuentasBancarias > 1)
                { habilitado = false; motivo = $"Tiene {item.NroCuentasBancarias} cuentas bancarias activas"; }

                return new Models.SolicitudMasivaItemViewModel
                {
                    GuidRegistro        = item.GuidRegistro ?? string.Empty,
                    NumeroProduccion    = item.NumeroProduccion,
                    DesTipoProduccion   = item.DesTipoProduccion,
                    RazonSocial         = item.RazonSocial,
                    Periodo             = item.Periodo,
                    MtoTotal            = item.MtoTotal,
                    Estado              = item.Estado,
                    DesEstado           = item.DesEstado,
                    Habilitado          = habilitado,
                    MotivoDeshabilitado = motivo,
                    FechaLimite         = item.FechaLimite,
                    FechaVencimiento    = item.FacturaFechaVencimiento
                };
            }).ToList();

            ViewBag.GuidPreseleccionado = guidRegistro;
            return PartialView("_ReNotificarFacturaModal", viewItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar modal de ReNotificar Factura");
            return StatusCode(500, "Error al cargar el modal");
        }
    }

    /// <summary>
    /// Ejecuta la acción de ReNotificar Factura para una lista de GUIDs.
    /// Accion "solo": reenvía el correo con las fechas actuales.
    /// Accion "cambiar": actualiza fecha límite y/o vencimiento, luego envía correo.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-26</created>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> EjecutarReNotificarFactura([FromBody] ReNotificarModalRequestDto dto)
    {
        try
        {
            if (dto == null || dto.Guids == null || !dto.Guids.Any())
                return Json(new { success = false, message = "Debe seleccionar al menos un registro" });

            var idUsuario    = GetCurrentUserId();
            bool tieneNuevaFL = !string.IsNullOrEmpty(dto.Fecha) && !string.IsNullOrEmpty(dto.Hora);
            bool tieneNuevaFV = !string.IsNullOrEmpty(dto.FechaVencimiento);

            // Validar fecha limite nueva si se proporcionó
            if (tieneNuevaFL)
            {
                if (!DateTime.TryParse($"{dto.Fecha}T{dto.Hora}", out var fl) || fl <= DateTime.Now)
                    return Json(new { success = false, message = "La nueva fecha y hora límite debe ser mayor a la fecha y hora actual." });
            }

            int enviados = 0;
            int errores  = 0;

            foreach (var guid in dto.Guids)
            {
                try
                {
                    bool ok;

                    if (dto.Accion == "solo")
                    {
                        ok = await _produccionService.RenotificarFacturaAsync(guid, idUsuario);
                    }
                    else
                    {
                        // Determinar fechas a usar (nueva o la que ya tiene en DB)
                        string useFecha = dto.Fecha ?? "";
                        string useHora  = dto.Hora  ?? "";
                        string useFV    = dto.FechaVencimiento ?? "";

                        if (!tieneNuevaFL || !tieneNuevaFV)
                        {
                            var prod = await _produccionService.GetProduccionByGuidAsync(guid);
                            if (!tieneNuevaFL && prod?.FechaLimite.HasValue == true)
                            {
                                useFecha = prod.FechaLimite.Value.ToString("yyyy-MM-dd");
                                useHora  = prod.FechaLimite.Value.ToString("HH:mm");
                            }
                            if (!tieneNuevaFV && prod?.FacturaFechaVencimiento.HasValue == true)
                                useFV = prod.FacturaFechaVencimiento.Value.ToString("yyyy-MM-dd");
                        }

                        // SolicitarFacturaAsync: actualiza fechas + estado(→SOLICITADA) + envía correo
                        ok = await _produccionService.SolicitarFacturaAsync(new SolicitarFacturaDto
                        {
                            GuidRegistro     = guid,
                            Fecha            = useFecha,
                            Hora             = useHora,
                            FechaVencimiento = useFV
                        }, idUsuario);
                    }

                    if (ok)
                    {
                        var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
                        if (produccion != null)
                        {
                            var descripcion = dto.Accion == "solo"
                                ? "Renotificación de solicitud de factura (sin cambio de fechas)"
                                : $"ReNotificación con{(tieneNuevaFL ? $" nueva fecha límite: {dto.Fecha} {dto.Hora}" : " fecha límite sin cambio")}" +
                                  $"{(tieneNuevaFV ? $", nueva F. Vencimiento: {dto.FechaVencimiento}" : ", F. Vencimiento sin cambio")}";
                            await _bitacoraService.CreateBitacoraAsync(new AppDomain.DTOs.Bitacora.CreateBitacoraDto
                            {
                                Entidad     = "SHM_PRODUCCION",
                                IdEntidad   = produccion.IdProduccion,
                                Accion      = "RENOTIFICAR_FACTURA",
                                Descripcion = descripcion,
                                FechaAccion = DateTime.Now
                            }, idUsuario);
                        }
                        enviados++;
                    }
                    else
                    {
                        errores++;
                    }
                }
                catch (Exception exItem)
                {
                    _logger.LogError(exItem, "Error en ReNotificar para GUID: {Guid}", guid);
                    errores++;
                }
            }

            return Json(new
            {
                success = true,
                enviados,
                errores,
                message = errores == 0
                    ? $"Se procesaron {enviados} registro(s) correctamente."
                    : $"Se procesaron {enviados} correctamente y {errores} con error."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en EjecutarReNotificarFactura");
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
