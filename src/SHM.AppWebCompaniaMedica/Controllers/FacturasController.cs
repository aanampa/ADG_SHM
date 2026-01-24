using System.Security.Claims;
using System.Text.Json;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Archivo;
using SHM.AppDomain.DTOs.ArchivoComprobante;
using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebCompaniaMedica.Models;
using SHM.AppWebCompaniaMedica.Services;

namespace SHM.AppWebCompaniaMedica.Controllers;

public class FacturasController : BaseController
{
    private readonly IProduccionService _produccionService;
    private readonly ISedeService _sedeService;
    private readonly IArchivoService _archivoService;
    private readonly IArchivoComprobanteService _archivoComprobanteService;
    private readonly IBitacoraService _bitacoraService;
    private readonly IEntidadCuentaBancariaService _entidadCuentaBancariaService;
    private readonly IBancoService _bancoService;
    private readonly IParametroService _parametroService;
    private readonly ILogger<FacturasController> _logger;
    private readonly IConfiguration _configuration;
    private readonly FacturaXmlParserService _facturaXmlParserService;

    public FacturasController(
        IProduccionService produccionService,
        ISedeService sedeService,
        IArchivoService archivoService,
        IArchivoComprobanteService archivoComprobanteService,
        IBitacoraService bitacoraService,
        IEntidadCuentaBancariaService entidadCuentaBancariaService,
        IBancoService bancoService,
        IParametroService parametroService,
        ILogger<FacturasController> logger,
        IConfiguration configuration,
        FacturaXmlParserService facturaXmlParserService)
    {
        _produccionService = produccionService;
        _sedeService = sedeService;
        _archivoService = archivoService;
        _archivoComprobanteService = archivoComprobanteService;
        _bitacoraService = bitacoraService;
        _entidadCuentaBancariaService = entidadCuentaBancariaService;
        _bancoService = bancoService;
        _parametroService = parametroService;
        _logger = logger;
        _configuration = configuration;
        _facturaXmlParserService = facturaXmlParserService;
    }

    // GET: Facturas/Pendientes
    public async Task<IActionResult> Pendientes(string? busqueda)
    {
        ViewData["Title"] = "Facturas Pendientes";

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                userId = 0;
            }

            var idEntidadMedicaClaim = User.FindFirstValue("IdEntidadMedica");
            if (!int.TryParse(idEntidadMedicaClaim, out var idEntidadMedica))
            {
                idEntidadMedica = 0;
            }

            // Obtener todas las producciones (en produccion filtrar por IdEntidadMedica del usuario)
            var producciones = await _produccionService.GetProduccionesByEntidadMedicaAsync(idEntidadMedica);

            // Filtrar solo las pendientes (Estado = "PENDIENTE" o sin comprobante)
            var pendientes = producciones
                .Where(p => p.Activo == 1 &&
                            p.Estado == "FACTURA_SOLICITADA" 
                            )
                .ToList();

            // Obtener todas las sedes para el mapeo
            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            // Mapear a ViewModel
            var facturas = pendientes.Select(p => new FacturaPendienteViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = p.Concepto ?? p.Descripcion,
                MtoTotal = p.MtoTotal,
                FechaLimite = p.FechaLimite,
                Estado = p.Estado ?? "FACTURA_SOLICITADA",
                GuidRegistro = p.GuidRegistro
            }).ToList();

            // Aplicar busqueda si existe
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                facturas = facturas.Where(f =>
                    (f.CodigoProduccion?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.NombreSede?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Concepto?.ToLower().Contains(busquedaLower) ?? false)
                ).ToList();
            }

            var model = new FacturasPendientesViewModel
            {
                Facturas = facturas,
                Busqueda = busqueda,
                TotalRegistros = facturas.Count
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener facturas pendientes");
            return View(new FacturasPendientesViewModel());
        }
    }

    // GET: Facturas/GetListPendientes (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetListPendientes(string? busqueda)
    {
        try
        {
            var producciones = await _produccionService.GetAllProduccionesAsync();

            var pendientes = producciones
                .Where(p => p.Activo == 1 &&
                           (string.IsNullOrEmpty(p.EstadoComprobante) ||
                            p.Estado == "PENDIENTE" ||
                            p.EstadoComprobante == "PENDIENTE"))
                .ToList();

            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            var facturas = pendientes.Select(p => new FacturaPendienteViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = p.Concepto ?? p.Descripcion,
                MtoTotal = p.MtoTotal,
                FechaLimite = p.FechaLimite,
                Estado = p.Estado ?? "PENDIENTE",
                GuidRegistro = p.GuidRegistro
            }).ToList();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                facturas = facturas.Where(f =>
                    (f.CodigoProduccion?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.NombreSede?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Concepto?.ToLower().Contains(busquedaLower) ?? false)
                ).ToList();
            }

            var model = new FacturasPendientesViewModel
            {
                Facturas = facturas,
                Busqueda = busqueda,
                TotalRegistros = facturas.Count
            };

            return PartialView("_ListPendientesPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de facturas pendientes");
            return PartialView("_ListPendientesPartial", new FacturasPendientesViewModel());
        }
    }

    // GET: Facturas/Enviadas
    public async Task<IActionResult> Enviadas(string? busqueda, int pageNumber = 1, int pageSize = 10)
    {
        ViewData["Title"] = "Facturas Enviadas";

        try
        {
            var producciones = await _produccionService.GetAllProduccionesAsync();

            // Filtrar solo las que tienen comprobante enviado
            var enviadas = producciones
                .Where(p => p.Activo == 1 &&
                           !string.IsNullOrEmpty(p.EstadoComprobante) &&
                           p.EstadoComprobante != "FACTURA_SOLICITADA" &&
                           p.EstadoComprobante != "PENDIENTE"
                           )
                .ToList();

            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            var facturas = enviadas.Select(p => new FacturaEnviadaViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = p.Concepto ?? p.Descripcion,
                MtoTotal = p.MtoTotal,
                FechaEmision = p.FechaEmision,
                Serie = p.Serie,
                Numero = p.Numero,
                EstadoComprobante = p.EstadoComprobante,
                GuidRegistro = p.GuidRegistro
            }).ToList();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                facturas = facturas.Where(f =>
                    (f.CodigoProduccion?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.NombreSede?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Concepto?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Serie?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Numero?.ToLower().Contains(busquedaLower) ?? false)
                ).ToList();
            }

            var totalRegistros = facturas.Count;
            var facturasPaginadas = facturas
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new FacturasEnviadasViewModel
            {
                Facturas = facturasPaginadas,
                Busqueda = busqueda,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRegistros = totalRegistros
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener facturas enviadas");
            return View(new FacturasEnviadasViewModel());
        }
    }

    // GET: Facturas/GetListEnviadas (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetListEnviadas(string? busqueda, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var producciones = await _produccionService.GetAllProduccionesAsync();

            // Filtrar solo las que tienen comprobante enviado
            var enviadas = producciones
                .Where(p => p.Activo == 1 &&
                           !string.IsNullOrEmpty(p.EstadoComprobante) &&
                           p.EstadoComprobante != "PENDIENTE" &
                           p.EstadoComprobante != "FACTURA_PENDIENTE" &&
                           p.EstadoComprobante != "FACTURA_SOLICITADA"
                           )
                .ToList();

            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            var facturas = enviadas.Select(p => new FacturaEnviadaViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = p.Concepto ?? p.Descripcion,
                MtoTotal = p.MtoTotal,
                FechaEmision = p.FechaEmision,
                Serie = p.Serie,
                Numero = p.Numero,
                EstadoComprobante = p.EstadoComprobante,
                GuidRegistro = p.GuidRegistro
            }).ToList();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                facturas = facturas.Where(f =>
                    (f.CodigoProduccion?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.NombreSede?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Concepto?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Serie?.ToLower().Contains(busquedaLower) ?? false) ||
                    (f.Numero?.ToLower().Contains(busquedaLower) ?? false)
                ).ToList();
            }

            var totalRegistros = facturas.Count;
            var facturasPaginadas = facturas
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new FacturasEnviadasViewModel
            {
                Facturas = facturasPaginadas,
                Busqueda = busqueda,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRegistros = totalRegistros
            };

            return PartialView("_ListEnviadasPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de facturas enviadas");
            return PartialView("_ListEnviadasPartial", new FacturasEnviadasViewModel());
        }
    }

    // GET: Facturas/Subir
    /// <author>ADG Antonio</author>
    /// <modified>ADG Antonio - 2026-01-23 - Agregado datos de cuenta bancaria y validacion por parametro</modified>
    public async Task<IActionResult> Subir(string guid)
    {
        ViewData["Title"] = "Subir Factura";

        try
        {
            if (string.IsNullOrEmpty(guid))
            {
                return RedirectToAction(nameof(Pendientes));
            }

            var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
            if (produccion == null)
            {
                return RedirectToAction(nameof(Pendientes));
            }

            var sedes = await _sedeService.GetAllSedesAsync();
            var sede = sedes.FirstOrDefault(s => s.IdSede == produccion.IdSede);

            // Obtener parametro de validacion de cuenta bancaria
            var validaCuentaBancaria = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_CUENTA_BANCARIA");
            var requiereValidacionCuenta = validaCuentaBancaria?.ToUpper() == "S";

            // Obtener cuenta bancaria de la entidad medica
            string? nombreBanco = null;
            string? cuentaCorriente = null;
            string? cuentaCci = null;
            string? moneda = null;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var cuentasBancarias = await _entidadCuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
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

            var model = new SubirFacturaViewModel
            {
                GuidRegistro = produccion.GuidRegistro,
                CodigoProduccion = produccion.CodigoProduccion,
                NombreSede = sede?.Nombre ?? $"Sede {produccion.IdSede}",
                Concepto = produccion.Concepto ?? produccion.Descripcion,
                MtoTotal = produccion.MtoTotal,
                FechaLimite = produccion.FechaLimite,
                NombreBanco = nombreBanco,
                CuentaCorriente = cuentaCorriente,
                CuentaCci = cuentaCci,
                Moneda = moneda,
                RequiereValidacionCuenta = requiereValidacionCuenta
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar formulario de subir factura");
            return RedirectToAction(nameof(Pendientes));
        }
    }

    // GET: Facturas/Detalle
    /// <author>ADG Antonio</author>
    /// <modified>ADG Antonio - 2026-01-23 - Agregado datos de cuenta bancaria</modified>
    public async Task<IActionResult> Detalle(string guid)
    {
        ViewData["Title"] = "Detalle de Factura";

        try
        {
            if (string.IsNullOrEmpty(guid))
            {
                return RedirectToAction(nameof(Enviadas));
            }

            var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
            if (produccion == null)
            {
                return RedirectToAction(nameof(Enviadas));
            }

            var sedes = await _sedeService.GetAllSedesAsync();
            var sede = sedes.FirstOrDefault(s => s.IdSede == produccion.IdSede);

            // Obtener archivos adjuntos
            var archivosComprobante = await _archivoComprobanteService.GetArchivoComprobantesByProduccionAsync(produccion.IdProduccion);
            var archivos = new List<ArchivoAdjuntoViewModel>();

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

            // Obtener cuenta bancaria de la entidad medica
            string? nombreBanco = null;
            string? cuentaCorriente = null;
            string? cuentaCci = null;
            string? moneda = null;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var cuentasBancarias = await _entidadCuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
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

            var model = new DetalleFacturaViewModel
            {
                IdProduccion = produccion.IdProduccion,
                GuidRegistro = produccion.GuidRegistro,
                CodigoProduccion = produccion.CodigoProduccion,
                NombreSede = sede?.Nombre ?? $"Sede {produccion.IdSede}",
                Concepto = produccion.Concepto,
                Descripcion = produccion.Descripcion,
                Periodo = produccion.Periodo,
                MtoConsumo = produccion.MtoConsumo,
                MtoDescuento = produccion.MtoDescuento,
                MtoSubtotal = produccion.MtoSubtotal,
                MtoIgv = produccion.MtoIgv,
                MtoTotal = produccion.MtoTotal,
                TipoComprobante = produccion.TipoComprobante,
                Serie = produccion.Serie,
                Numero = produccion.Numero,
                FechaEmision = produccion.FechaEmision,
                FechaLimite = produccion.FechaLimite,
                Estado = produccion.Estado,
                EstadoComprobante = produccion.EstadoComprobante,
                Archivos = archivos,
                NombreBanco = nombreBanco,
                CuentaCorriente = cuentaCorriente,
                CuentaCci = cuentaCci,
                Moneda = moneda
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de factura");
            return RedirectToAction(nameof(Enviadas));
        }
    }

    // GET: Facturas/Historial
    public IActionResult Historial()
    {
        ViewData["Title"] = "Historial de Facturas";
        return View();
    }

    // POST: Facturas/SubirFactura
    [HttpPost]
    public async Task<IActionResult> SubirFactura(
        [FromForm] string guidRegistro,
        [FromForm] string tipoComprobante,
        [FromForm] string serie,
        [FromForm] string numero,
        [FromForm] DateTime fechaEmision,
        [FromForm] IFormFile? archivoPdf,
        [FromForm] IFormFile? archivoXml,
        [FromForm] IFormFile? archivoCdr)
    {
        // Lista de archivos guardados para eliminar en caso de error
        var archivosGuardados = new List<string>();
        string? uploadPath = null;

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "Usuario no identificado" });
            }

            // Validar que existe la producción
            var produccion = await _produccionService.GetProduccionByGuidAsync(guidRegistro);
            if (produccion == null)
            {
                return Json(new { success = false, message = "Producción no encontrada" });
            }

            // Validar cuenta bancaria si el parametro lo requiere
            var validaCuentaBancaria = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_CUENTA_BANCARIA");
            if (validaCuentaBancaria?.ToUpper() == "S")
            {
                if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
                {
                    var cuentasBancarias = await _entidadCuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
                    var cuentaBancaria = cuentasBancarias.FirstOrDefault(c => c.Activo == 1);

                    if (cuentaBancaria == null || (string.IsNullOrEmpty(cuentaBancaria.CuentaCorriente) && string.IsNullOrEmpty(cuentaBancaria.CuentaCci)))
                    {
                        return Json(new { success = false, message = "No puede enviar facturas sin tener una cuenta bancaria registrada. Por favor contacte al administrador." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "No puede enviar facturas sin tener una cuenta bancaria registrada. Por favor contacte al administrador." });
                }
            }

            // Validar archivos requeridos
            if (archivoPdf == null || archivoXml == null || archivoCdr == null)
            {
                return Json(new { success = false, message = "Todos los archivos son requeridos (PDF, XML, CDR)" });
            }

            // Validar que el XML sea una factura electronica valida
            FacturaXmlValidationResult validationResult;
            using (var xmlStreamValidation = archivoXml.OpenReadStream())
            {
                validationResult = _facturaXmlParserService.ValidateFacturaXml(xmlStreamValidation);
            }

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("XML de factura invalido: {Errors}", validationResult.ErrorMessage);
                return Json(new { success = false, message = $"El XML de factura no es valido: {validationResult.ErrorMessage}" });
            }

            // Parsear XML para obtener serie y numero de factura
            FacturaXmlData facturaData;
            try
            {
                using var xmlStream = archivoXml.OpenReadStream();
                facturaData = _facturaXmlParserService.ParseFacturaXml(xmlStream);
            }
            catch (Exception xmlEx)
            {
                _logger.LogError(xmlEx, "Error al parsear el XML de factura");
                return Json(new { success = false, message = "Error al leer el archivo XML de factura" });
            }

            // Extraer serie y numero del XML (formato: "E001-17" -> serie="E001", numero="17")
            var numeroFactura = facturaData.DatosGenerales.NumeroFactura;
            if (string.IsNullOrEmpty(numeroFactura) || !numeroFactura.Contains('-'))
            {
                return Json(new { success = false, message = "El XML no contiene un numero de factura valido" });
            }

            var partes = numeroFactura.Split('-');
            var serieXml = partes[0];
            var numeroXml = partes[1];

            // Formatear numero a 8 digitos con ceros a la izquierda
            var numeroXmlOriginal = numeroXml;
            if (int.TryParse(numeroXml, out var numeroInt))
            {
                numeroXml = numeroInt.ToString("D8");
            }

            // Validar que los datos del formulario coincidan con los del XML
            var erroresCoincidencia = new List<string>();

            // Validar tipo de comprobante
            var tipoComprobanteXml = facturaData.DatosGenerales.CodigoTipoDocumento;
            if (!string.IsNullOrEmpty(tipoComprobante) && !string.IsNullOrEmpty(tipoComprobanteXml))
            {
                if (tipoComprobante != tipoComprobanteXml)
                {
                    erroresCoincidencia.Add($"Tipo de comprobante: formulario='{tipoComprobante}', XML='{tipoComprobanteXml}'");
                }
            }

            // Validar fecha de emision
            var fechaEmisionXmlStr = facturaData.DatosGenerales.FechaEmision;
            if (DateTime.TryParse(fechaEmisionXmlStr, out var fechaEmisionXml))
            {
                if (fechaEmision.Date != fechaEmisionXml.Date)
                {
                    erroresCoincidencia.Add($"Fecha de emision: formulario='{fechaEmision:yyyy-MM-dd}', XML='{fechaEmisionXml:yyyy-MM-dd}'");
                }
            }

            // Validar serie
            if (!string.Equals(serie?.Trim(), serieXml?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                erroresCoincidencia.Add($"Serie: formulario='{serie}', XML='{serieXml}'");
            }

            // Validar numero (comparar valores numericos para evitar problemas con ceros)
            if (int.TryParse(numero, out var numeroFormulario) && int.TryParse(numeroXmlOriginal, out var numeroXmlInt))
            {
                if (numeroFormulario != numeroXmlInt)
                {
                    erroresCoincidencia.Add($"Numero: formulario='{numero}', XML='{numeroXmlOriginal}'");
                }
            }
            else if (numero?.Trim() != numeroXmlOriginal?.Trim())
            {
                erroresCoincidencia.Add($"Numero: formulario='{numero}', XML='{numeroXmlOriginal}'");
            }

            // Validar importe total
            var importeTotalXml = facturaData.DesgloseTotales.ImporteTotal;
            if (produccion.MtoTotal.HasValue && importeTotalXml > 0)
            {
                // Comparar con tolerancia de 0.01 para evitar problemas de redondeo
                if (Math.Abs(produccion.MtoTotal.Value - importeTotalXml) > 0.01m)
                {
                    erroresCoincidencia.Add($"Importe total: produccion='S/ {produccion.MtoTotal:N2}', XML='S/ {importeTotalXml:N2}'");
                }
            }

            // Validar concepto contra descripcion del primer item del XML
            if (facturaData.DetalleItems.Count > 0)
            {
                var descripcionPrimerItem = facturaData.DetalleItems[0].Descripcion?.Trim();
                var conceptoProduccion = produccion.Concepto?.Trim();

                if (!string.IsNullOrEmpty(conceptoProduccion) && !string.IsNullOrEmpty(descripcionPrimerItem))
                {
                    if (!string.Equals(conceptoProduccion, descripcionPrimerItem, StringComparison.OrdinalIgnoreCase))
                    {
                        erroresCoincidencia.Add($"Concepto: produccion='{conceptoProduccion}', XML (primer item)='{descripcionPrimerItem}'");
                    }
                }
            }

            if (erroresCoincidencia.Count > 0)
            {
                _logger.LogWarning("Los datos del formulario no coinciden con el XML: {Errores}", string.Join("; ", erroresCoincidencia));
                return Json(new
                {
                    success = false,
                    message = "Los datos ingresados no coinciden con el XML de la factura: " + string.Join("; ", erroresCoincidencia)
                });
            }

            // Obtener ruta de archivos desde configuración
            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            uploadPath = Path.Combine(uploadBasePath, "facturas", guidRegistro);

            // Crear directorio si no existe
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Preparar nombres de archivos usando serie y numero del XML
            var pdfFileName = $"{serieXml}-{numeroXml}.pdf";
            var xmlFileName = $"{serieXml}-{numeroXml}.xml";
            var cdrExtension = Path.GetExtension(archivoCdr.FileName);
            var cdrFileName = $"{serieXml}-{numeroXml}-cdr{cdrExtension}";

            var pdfPath = Path.Combine(uploadPath, pdfFileName);
            var xmlPath = Path.Combine(uploadPath, xmlFileName);
            var cdrPath = Path.Combine(uploadPath, cdrFileName);

            // Guardar archivos físicos primero
            using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                await archivoPdf.CopyToAsync(stream);
            }
            archivosGuardados.Add(pdfPath);

            using (var stream = new FileStream(xmlPath, FileMode.Create))
            {
                await archivoXml.CopyToAsync(stream);
            }
            archivosGuardados.Add(xmlPath);

            using (var stream = new FileStream(cdrPath, FileMode.Create))
            {
                await archivoCdr.CopyToAsync(stream);
            }
            archivosGuardados.Add(cdrPath);

            // Guardar datos del XML parseado en archivo JSON
            var jsonFileName = $"{serieXml}-{numeroXml}.json";
            var jsonPath = Path.Combine(uploadPath, jsonFileName);
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var jsonContent = JsonSerializer.Serialize(facturaData, jsonOptions);
            await System.IO.File.WriteAllTextAsync(jsonPath, jsonContent);
            archivosGuardados.Add(jsonPath);
            _logger.LogInformation("Datos de factura XML extraidos y guardados en JSON: {JsonPath}", jsonPath);

            // Iniciar transacción para operaciones de base de datos
            using var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            // Registrar archivo PDF en BD
            var archivoPdfDto = new CreateArchivoDto
            {
                TipoArchivo = "PDF",
                NombreOriginal = archivoPdf.FileName,
                NombreArchivo = pdfFileName,
                Extension = ".pdf",
                Tamano = (int)archivoPdf.Length,
                Ruta = Path.Combine("facturas", guidRegistro, pdfFileName)
            };
            var archivoPdfCreado = await _archivoService.CreateArchivoAsync(archivoPdfDto, userId);

            // Registrar archivo XML en BD
            var archivoXmlDto = new CreateArchivoDto
            {
                TipoArchivo = "XML",
                NombreOriginal = archivoXml.FileName,
                NombreArchivo = xmlFileName,
                Extension = ".xml",
                Tamano = (int)archivoXml.Length,
                Ruta = Path.Combine("facturas", guidRegistro, xmlFileName)
            };
            var archivoXmlCreado = await _archivoService.CreateArchivoAsync(archivoXmlDto, userId);

            // Registrar archivo CDR en BD
            var archivoCdrDto = new CreateArchivoDto
            {
                TipoArchivo = "CDR",
                NombreOriginal = archivoCdr.FileName,
                NombreArchivo = cdrFileName,
                Extension = cdrExtension,
                Tamano = (int)archivoCdr.Length,
                Ruta = Path.Combine("facturas", guidRegistro, cdrFileName)
            };
            var archivoCdrCreado = await _archivoService.CreateArchivoAsync(archivoCdrDto, userId);

            // Crear registros en ArchivoComprobante vinculando archivos con producción
            await _archivoComprobanteService.CreateArchivoComprobanteAsync(new CreateArchivoComprobanteDto
            {
                IdProduccion = produccion.IdProduccion,
                IdArchivo = archivoPdfCreado.IdArchivo,
                TipoArchivo = "PDF",
                Descripcion = "Factura PDF"
            }, userId);

            await _archivoComprobanteService.CreateArchivoComprobanteAsync(new CreateArchivoComprobanteDto
            {
                IdProduccion = produccion.IdProduccion,
                IdArchivo = archivoXmlCreado.IdArchivo,
                TipoArchivo = "XML",
                Descripcion = "Factura XML"
            }, userId);

            await _archivoComprobanteService.CreateArchivoComprobanteAsync(new CreateArchivoComprobanteDto
            {
                IdProduccion = produccion.IdProduccion,
                IdArchivo = archivoCdrCreado.IdArchivo,
                TipoArchivo = "CDR",
                Descripcion = "Constancia CDR"
            }, userId);

            // Actualizar producción con datos del XML
            DateTime? fechaEmisionParaActualizar = null;
            if (DateTime.TryParse(facturaData.DatosGenerales.FechaEmision, out var fechaParsedUpdate))
            {
                fechaEmisionParaActualizar = fechaParsedUpdate;
            }

            var updateDto = new UpdateProduccionDto
            {
                TipoComprobante = facturaData.DatosGenerales.CodigoTipoDocumento,
                Serie = serieXml,
                Numero = numeroXml,
                FechaEmision = fechaEmisionParaActualizar ?? fechaEmision,
                EstadoComprobante = "ENVIADO",
                Estado = "FACTURA_ENVIADA"
            };

            var result = await _produccionService.UpdateProduccionAsync(produccion.IdProduccion, updateDto, userId);

            if (!result)
            {
                throw new InvalidOperationException("Error al actualizar la producción");
            }

            // Registrar en bitácora
            await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
            {
                Entidad = "SHM_ARCHIVO_COMPROBANTE",
                Accion = "Informar Factura",
                Descripcion = "Factura enviada"
            }, userId);

            // Confirmar transacción
            transactionScope.Complete();

            return Json(new { success = true, message = "Factura enviada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir factura. Realizando rollback.");

            // Eliminar archivos físicos guardados en caso de error
            foreach (var archivo in archivosGuardados)
            {
                try
                {
                    if (System.IO.File.Exists(archivo))
                    {
                        System.IO.File.Delete(archivo);
                        _logger.LogInformation("Archivo eliminado por rollback: {Archivo}", archivo);
                    }
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "No se pudo eliminar archivo durante rollback: {Archivo}", archivo);
                }
            }

            // Eliminar directorio si quedó vacío
            if (!string.IsNullOrEmpty(uploadPath) && Directory.Exists(uploadPath))
            {
                try
                {
                    var files = Directory.GetFiles(uploadPath);
                    if (files.Length == 0)
                    {
                        Directory.Delete(uploadPath);
                    }
                }
                catch (Exception dirEx)
                {
                    _logger.LogWarning(dirEx, "No se pudo eliminar directorio vacío: {Path}", uploadPath);
                }
            }

            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // GET: Facturas/DescargarArchivo
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

            // Obtener ruta base de archivos desde configuración
            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadBasePath, archivo.Ruta ?? "");

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Archivo físico no encontrado: {FilePath}", filePath);
                return NotFound("Archivo físico no encontrado");
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
}
