using System.Security.Claims;
using System.Text.Json;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.Archivo;
using SHM.AppDomain.DTOs.ArchivoComprobante;
using SHM.AppDomain.DTOs.Bitacora;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.DTOs.SanPabloApi;
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
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly IBancoService _bancoService;
    private readonly IParametroService _parametroService;
    private readonly ILogger<FacturasController> _logger;
    private readonly IConfiguration _configuration;
    private readonly FacturaXmlParserService _facturaXmlParserService;
    private readonly RheXmlParserService _rheXmlParserService;
    private readonly ISanPabloApiService _sanPabloApiService;
    private readonly ITablaDetalleService _tablaDetalleService;

    public FacturasController(
        IProduccionService produccionService,
        ISedeService sedeService,
        IArchivoService archivoService,
        IArchivoComprobanteService archivoComprobanteService,
        IBitacoraService bitacoraService,
        IEntidadCuentaBancariaService entidadCuentaBancariaService,
        IEntidadMedicaService entidadMedicaService,
        IBancoService bancoService,
        IParametroService parametroService,
        ILogger<FacturasController> logger,
        IConfiguration configuration,
        FacturaXmlParserService facturaXmlParserService,
        RheXmlParserService rheXmlParserService,
        ISanPabloApiService sanPabloApiService,
        ITablaDetalleService tablaDetalleService)
    {
        _produccionService = produccionService;
        _sedeService = sedeService;
        _archivoService = archivoService;
        _archivoComprobanteService = archivoComprobanteService;
        _bitacoraService = bitacoraService;
        _entidadCuentaBancariaService = entidadCuentaBancariaService;
        _entidadMedicaService = entidadMedicaService;
        _bancoService = bancoService;
        _parametroService = parametroService;
        _logger = logger;
        _configuration = configuration;
        _facturaXmlParserService = facturaXmlParserService;
        _rheXmlParserService = rheXmlParserService;
        _sanPabloApiService = sanPabloApiService;
        _tablaDetalleService = tablaDetalleService;
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
                            (p.Estado == EstadoDescripcion.Produccion.FacturaSolicitada  ||
                             p.Estado == EstadoDescripcion.Produccion.FacturaDevuelta
                            ))
                .ToList();

            // Obtener todas las sedes para el mapeo
            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            // Obtener tipos de produccion para calcular concepto cuando esta vacio
            var tiposProduccion = await _tablaDetalleService.ListarPorCodigoTablaAsync("TIPO_PRODUCCION");
            var tiposProdDict = tiposProduccion.ToDictionary(t => t.Codigo ?? "", t => t.Descripcion ?? "");

            // Mapear a ViewModel
            var facturas = pendientes.Select(p => new FacturaPendienteViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = !string.IsNullOrEmpty(p.Concepto) ? p.Concepto
                    : $"PRODUCCION {p.CodigoProduccion} - {(tiposProdDict.TryGetValue(p.TipoProduccion ?? "", out var desTipoProd) ? desTipoProd.ToUpper() : p.TipoProduccion ?? "")}",
                MtoTotal = p.MtoTotal,
                FechaLimite = p.FechaLimite,
                Estado = p.Estado ?? EstadoDescripcion.Produccion.FacturaSolicitada,
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
            var idEntidadMedicaClaim = User.FindFirstValue("IdEntidadMedica");
            if (!int.TryParse(idEntidadMedicaClaim, out var idEntidadMedica))
            {
                idEntidadMedica = 0;
            }

            var producciones = await _produccionService.GetProduccionesByEntidadMedicaAsync(idEntidadMedica);

            var pendientes = producciones
                .Where(p => p.Activo == 1 &&
                           (string.IsNullOrEmpty(p.EstadoComprobante) ||
                            p.Estado == "PENDIENTE" ||
                            p.EstadoComprobante == "PENDIENTE"))
                .ToList();

            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            // Obtener tipos de produccion para calcular concepto cuando esta vacio
            var tiposProduccion = await _tablaDetalleService.ListarPorCodigoTablaAsync("TIPO_PRODUCCION");
            var tiposProdDict = tiposProduccion.ToDictionary(t => t.Codigo ?? "", t => t.Descripcion ?? "");

            var facturas = pendientes.Select(p => new FacturaPendienteViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = !string.IsNullOrEmpty(p.Concepto) ? p.Concepto
                    : $"PRODUCCION {p.CodigoProduccion} - {(tiposProdDict.TryGetValue(p.TipoProduccion ?? "", out var desTipoProd) ? desTipoProd.ToUpper() : p.TipoProduccion ?? "")}",
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

    // GET: Facturas/GetDatosProduccion (AJAX para modal)
    [HttpGet]
    public async Task<IActionResult> GetDatosProduccion(string guid)
    {
        try
        {
            if (string.IsNullOrEmpty(guid))
            {
                return Json(new { success = false, message = "GUID no proporcionado" });
            }

            var produccion = await _produccionService.GetProduccionByGuidAsync(guid);
            if (produccion == null)
            {
                return Json(new { success = false, message = "Producción no encontrada" });
            }

            // Obtener datos del emisor (entidad medica)
            string emisorRuc = "";
            string emisorRazonSocial = "";
            if (produccion.IdEntidadMedica.HasValue)
            {
                var entidadMedica = await _entidadMedicaService.GetEntidadMedicaByIdAsync(produccion.IdEntidadMedica.Value);
                if (entidadMedica != null)
                {
                    emisorRuc = entidadMedica.Ruc ?? "";
                    emisorRazonSocial = entidadMedica.RazonSocial ?? "";
                }
            }

            // Obtener datos del receptor (sede)
            var sede = produccion.IdSede.HasValue && produccion.IdSede.Value > 0
                ? await _sedeService.GetSedeByIdAsync(produccion.IdSede.Value)
                : null;

            // Si TipoComprobante es nulo o vacio, derivar de TipoEntidadMedica
            var tipoComprobanteCodigo = produccion.TipoComprobante;
            if (string.IsNullOrEmpty(tipoComprobanteCodigo))
            {
                tipoComprobanteCodigo = produccion.TipoEntidadMedica == "1" ? "1" : "22";
            }

            // Convertir tipo de comprobante
            string tipoComprobanteTexto = tipoComprobanteCodigo switch
            {
                "1" => "Factura",
                "22" => "Recibo por Honorarios",
                _ => tipoComprobanteCodigo ?? "-"
            };

            // Si Concepto es nulo o vacio, construir desde tipo de produccion
            var concepto = produccion.Concepto;
            if (string.IsNullOrEmpty(concepto))
            {
                var detalleTipoProd = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_PRODUCCION", produccion.TipoProduccion ?? "");
                var descripcionTipoProd = detalleTipoProd?.Descripcion?.ToUpper() ?? produccion.TipoProduccion ?? "";
                concepto = $"PRODUCCION {produccion.CodigoProduccion} - {descripcionTipoProd}";
            }

            var data = new
            {
                success = true,
                codigoProduccion = produccion.CodigoProduccion,
                concepto = concepto,
                descripcion = produccion.Descripcion ?? "-",
                tipoComprobante = tipoComprobanteTexto,
                tipoComprobanteCodigo = tipoComprobanteCodigo,
                mtoSubtotal = produccion.MtoSubtotal?.ToString("N2") ?? "0.00",
                mtoIgv = produccion.MtoIgv?.ToString("N2") ?? "0.00",
                mtoRenta = produccion.MtoRenta?.ToString("N2") ?? "0.00",
                mtoTotal = produccion.MtoTotal?.ToString("N2") ?? "0.00",
                emisorRuc = emisorRuc,
                emisorRazonSocial = emisorRazonSocial,
                receptorRuc = sede?.Ruc ?? "-",
                receptorNombre = sede?.Nombre ?? "-",
                fechaLimite = produccion.FechaLimite?.ToString("dd/MM/yyyy HH:mm") ?? ""
            };

            return Json(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos de produccion con GUID: {Guid}", guid);
            return Json(new { success = false, message = "Error al obtener los datos" });
        }
    }

    // GET: Facturas/Enviadas
    public async Task<IActionResult> Enviadas(string? busqueda, int pageNumber = 1, int pageSize = 10)
    {
        ViewData["Title"] = "Facturas Enviadas";

        try
        {
            var idEntidadMedicaClaim = User.FindFirstValue("IdEntidadMedica");
            if (!int.TryParse(idEntidadMedicaClaim, out var idEntidadMedica))
            {
                idEntidadMedica = 0;
            }

            var producciones = await _produccionService.GetProduccionesByEntidadMedicaAsync(idEntidadMedica);

            // Filtrar solo las que tienen comprobante enviado
            var enviadas = producciones
                .Where(p => p.Activo == 1 &&
                           !string.IsNullOrEmpty(p.EstadoComprobante) &&
                           p.EstadoComprobante != EstadoDescripcion.Produccion.FacturaSolicitada &&
                           p.EstadoComprobante != "PENDIENTE"
                           )
                .ToList();

            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            // Obtener tipos de produccion para calcular concepto cuando esta vacio
            var tiposProduccion = await _tablaDetalleService.ListarPorCodigoTablaAsync("TIPO_PRODUCCION");
            var tiposProdDict = tiposProduccion.ToDictionary(t => t.Codigo ?? "", t => t.Descripcion ?? "");

            var facturas = enviadas.Select(p => new FacturaEnviadaViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = !string.IsNullOrEmpty(p.Concepto) ? p.Concepto
                    : $"PRODUCCION {p.CodigoProduccion} - {(tiposProdDict.TryGetValue(p.TipoProduccion ?? "", out var desTipoProd) ? desTipoProd.ToUpper() : p.TipoProduccion ?? "")}",
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
            var idEntidadMedicaClaim = User.FindFirstValue("IdEntidadMedica");
            if (!int.TryParse(idEntidadMedicaClaim, out var idEntidadMedica))
            {
                idEntidadMedica = 0;
            }

            var producciones = await _produccionService.GetProduccionesByEntidadMedicaAsync(idEntidadMedica);

            // Filtrar solo las que tienen comprobante enviado
            var enviadas = producciones
                .Where(p => p.Activo == 1 &&
                           !string.IsNullOrEmpty(p.EstadoComprobante) &&
                           p.EstadoComprobante != "PENDIENTE" &&
                           p.EstadoComprobante != EstadoDescripcion.Produccion.FacturaPendiente &&
                           p.EstadoComprobante != EstadoDescripcion.Produccion.FacturaSolicitada
                           )
                .ToList();

            var sedes = await _sedeService.GetAllSedesAsync();
            var sedesDict = sedes.ToDictionary(s => s.IdSede, s => s.Nombre);

            // Obtener tipos de produccion para calcular concepto cuando esta vacio
            var tiposProduccion = await _tablaDetalleService.ListarPorCodigoTablaAsync("TIPO_PRODUCCION");
            var tiposProdDict = tiposProduccion.ToDictionary(t => t.Codigo ?? "", t => t.Descripcion ?? "");

            var facturas = enviadas.Select(p => new FacturaEnviadaViewModel
            {
                IdProduccion = p.IdProduccion,
                CodigoProduccion = p.CodigoProduccion,
                NombreSede = sedesDict.TryGetValue(p.IdSede, out var nombreSede) ? nombreSede : $"Sede {p.IdSede}",
                Concepto = !string.IsNullOrEmpty(p.Concepto) ? p.Concepto
                    : $"PRODUCCION {p.CodigoProduccion} - {(tiposProdDict.TryGetValue(p.TipoProduccion ?? "", out var desTipoProd) ? desTipoProd.ToUpper() : p.TipoProduccion ?? "")}",
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
    /// <modified>ADG Antonio - 2026-01-29 - Agregada seccion de bitacora</modified>
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

            // Obtener parametro de requerimiento de archivo CDR
            var requiereArchivoCdr = await _parametroService.GetValorByCodigoAsync("SHM_REQUIERE_ARCHIVO_CDR");
            var requiereCdr = requiereArchivoCdr?.ToUpper() != "N"; // Por defecto es requerido (S o vacio = true)

            // Obtener datos del Emisor (Entidad Medica)
            string? emisorRuc = null;
            string? emisorRazonSocial = null;

            // Obtener cuenta bancaria de la entidad medica
            string? nombreBanco = null;
            string? cuentaCorriente = null;
            string? cuentaCci = null;
            string? moneda = null;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var entidadMedica = await _entidadMedicaService.GetEntidadMedicaByIdAsync(produccion.IdEntidadMedica.Value);
                if (entidadMedica != null)
                {
                    emisorRuc = entidadMedica.Ruc;
                    emisorRazonSocial = entidadMedica.RazonSocial;
                }

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

            // Obtener bitacora de la produccion
            var bitacoraItems = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_PRODUCCION", produccion.IdProduccion);
            var bitacora = bitacoraItems.Select(b => new BitacoraItemViewModel
            {
                Descripcion = b.Descripcion,
                FechaAccion = b.FechaAccion,
                NombreUsuario = b.NombreCompletoUsuario,
                Accion = b.Accion
            }).ToList();

            // Si TipoComprobante es nulo o vacio, derivar de TipoEntidadMedica
            var tipoComprobante = produccion.TipoComprobante;
            if (string.IsNullOrEmpty(tipoComprobante))
            {
                tipoComprobante = produccion.TipoEntidadMedica == "1" ? "1" : "22";
            }

            // Si Concepto es nulo o vacio, construir desde tipo de produccion
            var concepto = produccion.Concepto;
            if (string.IsNullOrEmpty(concepto))
            {
                var detalleTipoProd = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_PRODUCCION", produccion.TipoProduccion ?? "");
                var descripcionTipoProd = detalleTipoProd?.Descripcion?.ToUpper() ?? produccion.TipoProduccion ?? "";
                concepto = $"PRODUCCION {produccion.CodigoProduccion} - {descripcionTipoProd}";
            }

            var model = new SubirFacturaViewModel
            {
                IdProduccion = produccion.IdProduccion,
                GuidRegistro = produccion.GuidRegistro,
                CodigoProduccion = produccion.CodigoProduccion,
                NombreSede = sede?.Nombre ?? $"Sede {produccion.IdSede}",
                Concepto = concepto,
                Descripcion = produccion.Descripcion,
                MtoSubtotal = produccion.MtoSubtotal,
                MtoIgv = produccion.MtoIgv,
                MtoRenta = produccion.MtoRenta,
                MtoTotal = produccion.MtoTotal,
                FechaLimite = produccion.FechaLimite,
                TipoComprobante = tipoComprobante,
                // Datos del Emisor (Compañia Medica)
                EmisorRuc = emisorRuc,
                EmisorRazonSocial = emisorRazonSocial,
                // Datos del Receptor (Sede)
                ReceptorRuc = sede?.Ruc,
                ReceptorNombre = sede?.Nombre,
                // Datos bancarios
                NombreBanco = nombreBanco,
                CuentaCorriente = cuentaCorriente,
                CuentaCci = cuentaCci,
                Moneda = moneda,
                RequiereValidacionCuenta = requiereValidacionCuenta,
                RequiereCdr = requiereCdr,
                Bitacora = bitacora
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
    /// <modified>ADG Antonio - 2026-01-29 - Agregada seccion de bitacora</modified>
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

            // Validar que la produccion pertenezca a la entidad medica del usuario
            var idEntidadMedicaClaim = User.FindFirstValue("IdEntidadMedica");
            if (int.TryParse(idEntidadMedicaClaim, out var idEntidadMedica) && idEntidadMedica > 0)
            {
                if (produccion.IdEntidadMedica != idEntidadMedica)
                {
                    _logger.LogWarning("Acceso denegado: usuario con IdEntidadMedica {IdEntidadMedica} intento acceder a produccion {Guid} de otra entidad", idEntidadMedica, guid);
                    return RedirectToAction(nameof(Enviadas));
                }
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

            // Obtener bitacora de la produccion
            var bitacoraItems = await _bitacoraService.GetBitacorasByEntidadYIdAsync("SHM_PRODUCCION", produccion.IdProduccion);
            var bitacora = bitacoraItems.Select(b => new BitacoraItemViewModel
            {
                Descripcion = b.Descripcion,
                FechaAccion = b.FechaAccion,
                NombreUsuario = b.NombreCompletoUsuario,
                Accion = b.Accion
            }).ToList();

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
                Moneda = moneda,
                Bitacora = bitacora
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

    /// <summary>
    /// Parsea un archivo XML de factura y devuelve los datos para vista previa.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-25</created>
    /// </summary>
    [HttpPost]
    public IActionResult ParsearXmlVistaPrevia([FromForm] IFormFile? archivoXml, [FromForm] string? tipoComprobante)
    {
        try
        {
            if (archivoXml == null)
            {
                return Json(new { success = false, message = "No se recibio el archivo XML" });
            }

            FacturaXmlData facturaData;

            if (tipoComprobante == "02")
            {
                // Recibo por Honorarios Electronico
                RheXmlValidationResult rheValidationResult;
                using (var xmlStreamValidation = archivoXml.OpenReadStream())
                {
                    rheValidationResult = _rheXmlParserService.ValidateRheXml(xmlStreamValidation);
                }

                if (!rheValidationResult.IsValid)
                {
                    return Json(new { success = false, message = $"El XML no es valido: {rheValidationResult.ErrorMessage}" });
                }

                using (var xmlStream = archivoXml.OpenReadStream())
                {
                    facturaData = _rheXmlParserService.ParseRheXml(xmlStream);
                }
            }
            else
            {
                // Factura o Boleta
                FacturaXmlValidationResult validationResult;
                using (var xmlStreamValidation = archivoXml.OpenReadStream())
                {
                    validationResult = _facturaXmlParserService.ValidateFacturaXml(xmlStreamValidation);
                }

                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = $"El XML no es valido: {validationResult.ErrorMessage}" });
                }

                using (var xmlStream = archivoXml.OpenReadStream())
                {
                    facturaData = _facturaXmlParserService.ParseFacturaXml(xmlStream);
                }
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    numeroFactura = facturaData.DatosGenerales.NumeroFactura,
                    fechaEmision = facturaData.DatosGenerales.FechaEmision,
                    horaEmision = facturaData.DatosGenerales.HoraEmision,
                    tipoDocumento = facturaData.DatosGenerales.TipoDocumento,
                    moneda = facturaData.DatosGenerales.Moneda,
                    totalEnLetras = facturaData.DatosGenerales.TotalEnLetras,
                    emisor = new
                    {
                        ruc = facturaData.Emisor.Ruc,
                        razonSocial = facturaData.Emisor.RazonSocial,
                        direccion = facturaData.Emisor.Direccion,
                        ubicacion = facturaData.Emisor.Ubicacion
                    },
                    cliente = new
                    {
                        tipoDocumento = facturaData.Cliente.TipoDocumento,
                        numeroDocumento = facturaData.Cliente.NumeroDocumento,
                        razonSocial = facturaData.Cliente.RazonSocial,
                        direccion = facturaData.Cliente.Direccion
                    },
                    totales = new
                    {
                        valorVenta = facturaData.DesgloseTotales.ValorVenta,
                        igv = facturaData.DesgloseTotales.Igv,
                        retencion = facturaData.DesgloseTotales.Retencion,
                        importeTotal = facturaData.DesgloseTotales.ImporteTotal,
                        descuentos = facturaData.DesgloseTotales.Descuentos
                    },
                    items = facturaData.DetalleItems.Select(i => new
                    {
                        numeroItem = i.NumeroItem,
                        descripcion = i.Descripcion,
                        cantidad = i.Cantidad,
                        unidadMedida = i.UnidadMedida,
                        precioUnitario = i.PrecioUnitario,
                        valorVenta = i.ValorVenta
                    }).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al parsear XML para vista previa");
            return Json(new { success = false, message = "Error al procesar el archivo XML" });
        }
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

            // Obtener primera cuenta bancaria activa de la entidad medica
            int? idCuentaBanco = null;
            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var cuentasBancarias = await _entidadCuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
                var cuentaBancaria = cuentasBancarias.FirstOrDefault(c => c.Activo == 1);

                // Validar cuenta bancaria si el parametro lo requiere
                var validaCuentaBancaria = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_CUENTA_BANCARIA");
                if (validaCuentaBancaria?.ToUpper() == "S")
                {
                    if (cuentaBancaria == null || (string.IsNullOrEmpty(cuentaBancaria.CuentaCorriente) && string.IsNullOrEmpty(cuentaBancaria.CuentaCci)))
                    {
                        return Json(new { success = false, message = "No puede enviar facturas sin tener una cuenta bancaria registrada. Por favor contacte al administrador." });
                    }
                }

                if (cuentaBancaria != null)
                {
                    idCuentaBanco = cuentaBancaria.IdCuentaBancaria;
                }
            }
            else
            {
                var validaCuentaBancaria = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_CUENTA_BANCARIA");
                if (validaCuentaBancaria?.ToUpper() == "S")
                {
                    return Json(new { success = false, message = "No puede enviar facturas sin tener una cuenta bancaria registrada. Por favor contacte al administrador." });
                }
            }

            // Obtener parametro de requerimiento de archivo CDR
            var requiereArchivoCdrParam = await _parametroService.GetValorByCodigoAsync("SHM_REQUIERE_ARCHIVO_CDR");
            var requiereCdrSubir = requiereArchivoCdrParam?.ToUpper() != "N";

            // Validar archivos requeridos
            if (archivoPdf == null || archivoXml == null || (requiereCdrSubir && archivoCdr == null))
            {
                return Json(new { success = false, message = requiereCdrSubir
                    ? "Todos los archivos son requeridos (PDF, XML, CDR)"
                    : "Los archivos PDF y XML son requeridos" });
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
            var serieUsuario = partes[0];
            var numeroFormateado = partes[1];

            // Formatear numero a 8 digitos con ceros a la izquierda
            var numeroFormateadoOriginal = numeroFormateado;
            if (int.TryParse(numeroFormateado, out var numeroInt))
            {
                numeroFormateado = numeroInt.ToString("D8");
            }

            // // Validar que los datos del formulario coincidan con los del XML
            // var erroresCoincidencia = new List<string>();

            // // Validar tipo de comprobante
            // var tipoComprobanteXml = facturaData.DatosGenerales.CodigoTipoDocumento;
            // if (!string.IsNullOrEmpty(tipoComprobante) && !string.IsNullOrEmpty(tipoComprobanteXml))
            // {
            //     if (tipoComprobante != tipoComprobanteXml)
            //     {
            //         erroresCoincidencia.Add($"Tipo de comprobante: formulario='{tipoComprobante}', XML='{tipoComprobanteXml}'");
            //     }
            // }

            // // Validar fecha de emision
            // var fechaEmisionXmlStr = facturaData.DatosGenerales.FechaEmision;
            // if (DateTime.TryParse(fechaEmisionXmlStr, out var fechaEmisionXml))
            // {
            //     if (fechaEmision.Date != fechaEmisionXml.Date)
            //     {
            //         erroresCoincidencia.Add($"Fecha de emision: formulario='{fechaEmision:yyyy-MM-dd}', XML='{fechaEmisionXml:yyyy-MM-dd}'");
            //     }
            // }

            // // Validar serie
            // if (!string.Equals(serie?.Trim(), serieUsuario?.Trim(), StringComparison.OrdinalIgnoreCase))
            // {
            //     erroresCoincidencia.Add($"Serie: formulario='{serie}', XML='{serieUsuario}'");
            // }

            // // Validar numero (comparar valores numericos para evitar problemas con ceros)
            // if (int.TryParse(numero, out var numeroFormulario) && int.TryParse(numeroFormateadoOriginal, out var numeroFormateadoInt))
            // {
            //     if (numeroFormulario != numeroFormateadoInt)
            //     {
            //         erroresCoincidencia.Add($"Numero: formulario='{numero}', XML='{numeroFormateadoOriginal}'");
            //     }
            // }
            // else if (numero?.Trim() != numeroFormateadoOriginal?.Trim())
            // {
            //     erroresCoincidencia.Add($"Numero: formulario='{numero}', XML='{numeroFormateadoOriginal}'");
            // }

            // Validar concepto contra descripcion del primer item del XML (si el parametro lo requiere)
            var validaConceptoParam = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_FACTURA_CONCEPTO");
            if (validaConceptoParam?.ToUpper() == "S")
            {
                if (facturaData.DetalleItems.Count > 0)
                {
                    var descripcionPrimerItem = facturaData.DetalleItems[0].Descripcion?.Trim();
                    var conceptoProduccion = produccion.Concepto?.Trim();

                    if (!string.IsNullOrEmpty(conceptoProduccion) && !string.IsNullOrEmpty(descripcionPrimerItem))
                    {
                        if (!string.Equals(conceptoProduccion, descripcionPrimerItem, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("El concepto no coincide. Produccion: '{ConceptoProduccion}', XML: '{ConceptoXml}'",
                                conceptoProduccion, descripcionPrimerItem);
                            return Json(new
                            {
                                success = false,
                                message = $"El concepto del XML no coincide con la produccion. Esperado: '{conceptoProduccion}', XML: '{descripcionPrimerItem}'"
                            });
                        }
                    }
                }
            }

            // Determinar tipo de almacenamiento configurado (FILE o BLOB)
            var tipoAlmacenamientoParam = await _parametroService.GetValorByCodigoAsync("SHM_TIPO_ALMACENAMIENTO_ARCHIVO");
            var usarBlobStorage = tipoAlmacenamientoParam?.ToUpper() == "BLOB";

            // Preparar nombres de archivos usando serie y numero del XML
            var pdfFileName = $"{serieUsuario}-{numeroFormateado}.pdf";
            var xmlFileName = $"{serieUsuario}-{numeroFormateado}.xml";
            var cdrExtension = archivoCdr != null ? Path.GetExtension(archivoCdr.FileName) : "";
            var cdrFileName = archivoCdr != null ? $"{serieUsuario}-{numeroFormateado}-cdr{cdrExtension}" : null;

            // Variables para contenido BLOB (solo se usan si usarBlobStorage = true)
            byte[]? pdfContent = null;
            byte[]? xmlContent = null;
            byte[]? cdrContent = null;

            if (usarBlobStorage)
            {
                // Modo BLOB: Leer contenido de archivos en memoria
                _logger.LogInformation("Usando almacenamiento BLOB para archivos");

                using (var ms = new MemoryStream())
                {
                    await archivoPdf.CopyToAsync(ms);
                    pdfContent = ms.ToArray();
                }

                using (var ms = new MemoryStream())
                {
                    await archivoXml.CopyToAsync(ms);
                    xmlContent = ms.ToArray();
                }

                if (archivoCdr != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        await archivoCdr.CopyToAsync(ms);
                        cdrContent = ms.ToArray();
                    }
                }
            }
            else
            {
                // Modo FILE: Guardar archivos en sistema de archivos
                _logger.LogInformation("Usando almacenamiento FILE para archivos");

                var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                uploadPath = Path.Combine(uploadBasePath, "facturas", guidRegistro);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var pdfPath = Path.Combine(uploadPath, pdfFileName);
                var xmlPath = Path.Combine(uploadPath, xmlFileName);
                var cdrPath = cdrFileName != null ? Path.Combine(uploadPath, cdrFileName) : null;

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

                if (archivoCdr != null && cdrPath != null)
                {
                    using (var stream = new FileStream(cdrPath, FileMode.Create))
                    {
                        await archivoCdr.CopyToAsync(stream);
                    }
                    archivosGuardados.Add(cdrPath);
                }

                // Guardar datos del XML parseado en archivo JSON (solo en modo FILE)
                var jsonFileName = $"{serieUsuario}-{numeroFormateado}.json";
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
            }

            // Formatear numero del usuario a 8 digitos
            var numeroUsuarioFormateado = numero;
            if (int.TryParse(numero, out var numUsuarioInt))
            {
                numeroUsuarioFormateado = numUsuarioInt.ToString("D8");
            }

            // Iniciar transaccion para operaciones de base de datos
            using (var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {

            // Inactivar archivos anteriores de esta produccion (si existen)
            var archivosAnteriores = await _archivoComprobanteService.GetArchivoComprobantesByProduccionAsync(produccion.IdProduccion);
            foreach (var archivoAnterior in archivosAnteriores.Where(a => a.Activo == 1))
            {
                // Inactivar el registro ArchivoComprobante
                await _archivoComprobanteService.DeleteArchivoComprobanteAsync(archivoAnterior.IdArchivoComprobante, userId);

                // Inactivar el archivo asociado
                if (archivoAnterior.IdArchivo.HasValue)
                {
                    await _archivoService.DeleteArchivoAsync(archivoAnterior.IdArchivo.Value, userId);
                }

                _logger.LogInformation("Archivo anterior inactivado. IdArchivoComprobante: {IdAC}, IdArchivo: {IdA}",
                    archivoAnterior.IdArchivoComprobante, archivoAnterior.IdArchivo);
            }

            // Registrar archivo PDF en BD
            var archivoPdfDto = new CreateArchivoDto
            {
                TipoArchivo = "PDF",
                NombreOriginal = archivoPdf.FileName,
                NombreArchivo = pdfFileName,
                Extension = ".pdf",
                Tamano = (int)archivoPdf.Length,
                Ruta = usarBlobStorage ? null : Path.Combine("facturas", guidRegistro, pdfFileName),
                ContenidoArchivo = pdfContent
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
                Ruta = usarBlobStorage ? null : Path.Combine("facturas", guidRegistro, xmlFileName),
                ContenidoArchivo = xmlContent
            };
            var archivoXmlCreado = await _archivoService.CreateArchivoAsync(archivoXmlDto, userId);

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

            // Registrar archivo CDR en BD solo si existe
            if (archivoCdr != null && cdrFileName != null)
            {
                var archivoCdrDto = new CreateArchivoDto
                {
                    TipoArchivo = "CDR",
                    NombreOriginal = archivoCdr.FileName,
                    NombreArchivo = cdrFileName,
                    Extension = cdrExtension,
                    Tamano = (int)archivoCdr.Length,
                    Ruta = usarBlobStorage ? null : Path.Combine("facturas", guidRegistro, cdrFileName),
                    ContenidoArchivo = cdrContent
                };
                var archivoCdrCreado = await _archivoService.CreateArchivoAsync(archivoCdrDto, userId);

                await _archivoComprobanteService.CreateArchivoComprobanteAsync(new CreateArchivoComprobanteDto
                {
                    IdProduccion = produccion.IdProduccion,
                    IdArchivo = archivoCdrCreado.IdArchivo,
                    TipoArchivo = "CDR",
                    Descripcion = "Constancia CDR"
                }, userId);
            }

            // Actualizar produccion con datos del formulario (ingresados por el usuario)
            var updateDto = new UpdateProduccionDto
            {
                TipoComprobante = tipoComprobante,
                Serie = serie,
                Numero = numeroUsuarioFormateado,
                FechaEmision = fechaEmision,
                EstadoComprobante = "ENVIADO",
                Glosa = produccion.Concepto,
                Estado = EstadoDescripcion.Produccion.FacturaEnviada,
                FacturaFechaEnvio = DateTime.Now,
                IdCuentaBanco = idCuentaBanco
            };

            var result = await _produccionService.UpdateProduccionAsync(produccion.IdProduccion, updateDto, userId);

            if (!result)
            {
                throw new InvalidOperationException("Error al actualizar la producción");
            }

            // Registrar en bitacora
            await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
            {
                Entidad = "SHM_PRODUCCION",
                IdEntidad = produccion.IdProduccion,
                Accion = EstadoDescripcion.Produccion.FacturaEnviada,
                Descripcion = $"Envio de comprobante de pago electrónico: {serie}-{numero}",
                FechaAccion = DateTime.Now
            }, userId);

            // Confirmar transaccion
            transactionScope.Complete();
            } // Fin del TransactionScope

            // Informar al sistema externo (San Pablo) si el parametro lo indica
            var enviaHhmm = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_ENVIA_HHMM");
            if (enviaHhmm?.ToUpper() == "S")
            {
                var errorHhmm = await RegistrarComprobanteEnSanPabloAsync(produccion, serie, numeroUsuarioFormateado, tipoComprobante, fechaEmision, produccion.Concepto, userId);
                if (errorHhmm != null)
                {
                    return Json(new { success = false, message = $"El comprobante se registro localmente pero no se pudo enviar a San Pablo: {errorHhmm}" });
                }
            }

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

    /// <summary>
    /// Prepara la vista previa de la factura guardando los archivos temporalmente.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-25</created>
    [HttpPost]
    public async Task<IActionResult> PrepararVistaPrevia(
        [FromForm] string guidRegistro,
        [FromForm] string tipoComprobante,
        [FromForm] string serie,
        [FromForm] string numero,
        [FromForm] DateTime fechaEmision,
        [FromForm] IFormFile? archivoPdf,
        [FromForm] IFormFile? archivoXml,
        [FromForm] IFormFile? archivoCdr)
    {
        try
        {
            // Validar produccion
            var produccion = await _produccionService.GetProduccionByGuidAsync(guidRegistro);
            if (produccion == null)
            {
                TempData["Error"] = "Producción no encontrada";
                return RedirectToAction(nameof(Pendientes));
            }

            // Validar fecha de emision no futura
            if (fechaEmision.Date > DateTime.Now.Date)
            {
                TempData["Error"] = "La fecha de emisión no puede ser una fecha futura";
                return RedirectToAction(nameof(Subir), new { guid = guidRegistro });
            }

            // Obtener parametro de requerimiento de archivo CDR
            var requiereArchivoCdr = await _parametroService.GetValorByCodigoAsync("SHM_REQUIERE_ARCHIVO_CDR");
            var requiereCdr = requiereArchivoCdr?.ToUpper() != "N";

            // Validar archivos requeridos
            if (archivoPdf == null || archivoXml == null || (requiereCdr && archivoCdr == null))
            {
                TempData["Error"] = requiereCdr
                    ? "Todos los archivos son requeridos (PDF, XML, CDR)"
                    : "Los archivos PDF y XML son requeridos";
                return RedirectToAction(nameof(Subir), new { guid = guidRegistro });
            }

            // Validar y parsear XML segun tipo de comprobante
            FacturaXmlData facturaData;

            if (tipoComprobante == "02")
            {
                // Recibo por Honorarios Electronico
                RheXmlValidationResult rheValidationResult;
                using (var xmlStreamValidation = archivoXml.OpenReadStream())
                {
                    rheValidationResult = _rheXmlParserService.ValidateRheXml(xmlStreamValidation);
                }

                if (!rheValidationResult.IsValid)
                {
                    TempData["Error"] = $"El XML no es válido: {rheValidationResult.ErrorMessage}";
                    return RedirectToAction(nameof(Subir), new { guid = guidRegistro });
                }

                using (var xmlStream = archivoXml.OpenReadStream())
                {
                    facturaData = _rheXmlParserService.ParseRheXml(xmlStream);
                }
            }
            else
            {
                // Factura o Boleta
                FacturaXmlValidationResult validationResult;
                using (var xmlStreamValidation = archivoXml.OpenReadStream())
                {
                    validationResult = _facturaXmlParserService.ValidateFacturaXml(xmlStreamValidation);
                }

                if (!validationResult.IsValid)
                {
                    TempData["Error"] = $"El XML no es válido: {validationResult.ErrorMessage}";
                    return RedirectToAction(nameof(Subir), new { guid = guidRegistro });
                }

                using (var xmlStream = archivoXml.OpenReadStream())
                {
                    facturaData = _facturaXmlParserService.ParseFacturaXml(xmlStream);
                }
            }

            // Crear session ID unico
            var sessionId = Guid.NewGuid().ToString("N");

            // Obtener ruta temporal
            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var tempPath = Path.Combine(uploadBasePath, "temp", sessionId);
            Directory.CreateDirectory(tempPath);

            // Guardar archivos temporales
            var pdfPath = Path.Combine(tempPath, $"factura.pdf");
            var xmlPath = Path.Combine(tempPath, $"factura.xml");

            using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                await archivoPdf.CopyToAsync(stream);
            }
            using (var stream = new FileStream(xmlPath, FileMode.Create))
            {
                await archivoXml.CopyToAsync(stream);
            }

            // Guardar CDR solo si es requerido y se proporciono
            if (archivoCdr != null)
            {
                var cdrPath = Path.Combine(tempPath, $"cdr{Path.GetExtension(archivoCdr.FileName)}");
                using (var stream = new FileStream(cdrPath, FileMode.Create))
                {
                    await archivoCdr.CopyToAsync(stream);
                }
            }

            // Guardar datos del XML en archivo JSON
            var jsonPath = Path.Combine(tempPath, "datos.json");
            var jsonContent = JsonSerializer.Serialize(facturaData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await System.IO.File.WriteAllTextAsync(jsonPath, jsonContent);

            // Guardar metadatos de la sesion
            var metadataPath = Path.Combine(tempPath, "metadata.json");
            var metadata = new
            {
                GuidRegistro = guidRegistro,
                TipoComprobante = tipoComprobante,
                Serie = serie,
                Numero = numero,
                FechaEmision = fechaEmision,
                CdrExtension = archivoCdr != null ? Path.GetExtension(archivoCdr.FileName) : "",
                TieneCdr = archivoCdr != null,
                CreatedAt = DateTime.Now
            };
            var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(metadataPath, metadataJson);

            _logger.LogInformation("Vista previa preparada. SessionId: {SessionId}, GuidRegistro: {GuidRegistro}", sessionId, guidRegistro);

            return RedirectToAction(nameof(VistaPrevia), new { sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar vista previa");
            TempData["Error"] = "Error al procesar los archivos. Intente nuevamente.";
            return RedirectToAction(nameof(Subir), new { guid = guidRegistro });
        }
    }

    /// <summary>
    /// Muestra la pagina de vista previa de la factura.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-25</created>
    public async Task<IActionResult> VistaPrevia(string sessionId)
    {
        ViewData["Title"] = "Vista Previa de Factura";

        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return RedirectToAction(nameof(Pendientes));
            }

            // Obtener ruta de archivos temporales
            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var tempPath = Path.Combine(uploadBasePath, "temp", sessionId);

            if (!Directory.Exists(tempPath))
            {
                TempData["Error"] = "Sesión de vista previa no encontrada o expirada";
                return RedirectToAction(nameof(Pendientes));
            }

            // Leer metadatos
            var metadataPath = Path.Combine(tempPath, "metadata.json");
            if (!System.IO.File.Exists(metadataPath))
            {
                TempData["Error"] = "Datos de sesión no encontrados";
                return RedirectToAction(nameof(Pendientes));
            }

            var metadataJson = await System.IO.File.ReadAllTextAsync(metadataPath);
            using var metadataDoc = JsonDocument.Parse(metadataJson);
            var metadata = metadataDoc.RootElement;

            var guidRegistro = metadata.GetProperty("GuidRegistro").GetString();

            // Obtener produccion
            var produccion = await _produccionService.GetProduccionByGuidAsync(guidRegistro ?? "");
            if (produccion == null)
            {
                TempData["Error"] = "Producción no encontrada";
                return RedirectToAction(nameof(Pendientes));
            }

            // Obtener sede
            var sedes = await _sedeService.GetAllSedesAsync();
            var sede = sedes.FirstOrDefault(s => s.IdSede == produccion.IdSede);

            // Leer datos del XML
            var jsonPath = Path.Combine(tempPath, "datos.json");
            var datosJson = await System.IO.File.ReadAllTextAsync(jsonPath);
            var datosXml = JsonSerializer.Deserialize<FacturaXmlData>(datosJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Obtener datos del Emisor (Compania Medica)
            string? emisorRuc = null;
            string? emisorRazonSocial = null;

            // Obtener cuenta bancaria
            string? nombreBanco = null;
            string? cuentaCorriente = null;
            string? cuentaCci = null;
            string? moneda = null;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var entidadMedica = await _entidadMedicaService.GetEntidadMedicaByIdAsync(produccion.IdEntidadMedica.Value);
                if (entidadMedica != null)
                {
                    emisorRuc = entidadMedica.Ruc;
                    emisorRazonSocial = entidadMedica.RazonSocial;
                }

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

            DateTime? fechaEmision = null;
            if (metadata.TryGetProperty("FechaEmision", out var fechaEmisionElement))
            {
                fechaEmision = fechaEmisionElement.GetDateTime();
            }

            // Obtener parametros de validacion configurable
            var paramValidaTipo = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_TIPO");
            var paramValidaFechaEmision = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_FECHA_EMISION");
            var paramValidaSerie = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_SERIE");
            var paramValidaNumero = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_NUMERO");
            var paramValidaImporte = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_IMPORTE");
            var paramValidaConcepto = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_FACTURA_CONCEPTO");
            var paramValidaRucEmisor = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_RUC_EMISOR");
            var paramValidaRucReceptor = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_VALIDA_RUC_RECEPTOR");

            // Si Concepto es nulo o vacio, construir desde tipo de produccion
            var conceptoVistaPrevia = produccion.Concepto;
            if (string.IsNullOrEmpty(conceptoVistaPrevia))
            {
                var detalleTipoProd = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_PRODUCCION", produccion.TipoProduccion ?? "");
                var descripcionTipoProd = detalleTipoProd?.Descripcion?.ToUpper() ?? produccion.TipoProduccion ?? "";
                conceptoVistaPrevia = $"PRODUCCION {produccion.CodigoProduccion} - {descripcionTipoProd}";
            }

            var model = new VistaPreviaFacturaViewModel
            {
                SessionId = sessionId,
                GuidRegistro = guidRegistro,
                CodigoProduccion = produccion.CodigoProduccion,
                NombreSede = sede?.Nombre ?? $"Sede {produccion.IdSede}",
                Concepto = conceptoVistaPrevia,
                MtoTotal = produccion.MtoTotal,
                FechaLimite = produccion.FechaLimite,
                TipoComprobante = metadata.GetProperty("TipoComprobante").GetString(),
                Serie = metadata.GetProperty("Serie").GetString(),
                Numero = metadata.GetProperty("Numero").GetString(),
                FechaEmision = fechaEmision,
                EmisorRuc = emisorRuc,
                EmisorRazonSocial = emisorRazonSocial,
                ReceptorRuc = sede?.Ruc,
                ReceptorNombre = sede?.Nombre,
                NombreBanco = nombreBanco,
                CuentaCorriente = cuentaCorriente,
                CuentaCci = cuentaCci,
                Moneda = moneda,
                PdfTempPath = $"/Facturas/ObtenerPdfTemporal?sessionId={sessionId}",
                DatosXml = datosXml,
                ValidaTipo = paramValidaTipo?.ToUpper() != "N",
                ValidaFechaEmision = paramValidaFechaEmision?.ToUpper() != "N",
                ValidaSerie = paramValidaSerie?.ToUpper() != "N",
                ValidaNumero = paramValidaNumero?.ToUpper() != "N",
                ValidaImporte = paramValidaImporte?.ToUpper() != "N",
                ValidaConcepto = paramValidaConcepto?.ToUpper() == "S",
                ValidaRucEmisor = paramValidaRucEmisor?.ToUpper() != "N",
                ValidaRucReceptor = paramValidaRucReceptor?.ToUpper() != "N"
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar vista previa");
            TempData["Error"] = "Error al cargar la vista previa";
            return RedirectToAction(nameof(Pendientes));
        }
    }

    /// <summary>
    /// Sirve el archivo PDF temporal para la vista previa.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-25</created>
    [HttpGet]
    public IActionResult ObtenerPdfTemporal(string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return NotFound();
            }

            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var pdfPath = Path.Combine(uploadBasePath, "temp", sessionId, "factura.pdf");

            if (!System.IO.File.Exists(pdfPath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(pdfPath);
            Response.Headers.Append("Content-Disposition", "inline; filename=\"factura.pdf\"");
            return File(fileBytes, "application/pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener PDF temporal");
            return NotFound();
        }
    }

    /// <summary>
    /// Confirma el envio de la factura desde la vista previa.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-25</created>
    [HttpPost]
    public async Task<IActionResult> ConfirmarEnvio([FromForm] string sessionId)
    {
        var archivosGuardados = new List<string>();
        string? uploadPath = null;
        string? tempPath = null;

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "Usuario no identificado" });
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                return Json(new { success = false, message = "Sesión no válida" });
            }

            // Obtener ruta de archivos temporales
            var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            tempPath = Path.Combine(uploadBasePath, "temp", sessionId);

            if (!Directory.Exists(tempPath))
            {
                return Json(new { success = false, message = "Sesión expirada. Por favor, vuelva a subir los archivos." });
            }

            // Leer metadatos
            var metadataPath = Path.Combine(tempPath, "metadata.json");
            var metadataJson = await System.IO.File.ReadAllTextAsync(metadataPath);
            using var metadataDoc = JsonDocument.Parse(metadataJson);
            var metadata = metadataDoc.RootElement;

            var guidRegistro = metadata.GetProperty("GuidRegistro").GetString() ?? "";
            var tipoComprobanteUsuario = metadata.GetProperty("TipoComprobante").GetString() ?? "";
            var serieUsuario = metadata.GetProperty("Serie").GetString() ?? "";
            var numeroUsuario = metadata.GetProperty("Numero").GetString() ?? "";
            var cdrExtension = metadata.TryGetProperty("CdrExtension", out var cdrExtProp) ? cdrExtProp.GetString() ?? ".zip" : ".zip";
            var tieneCdr = metadata.TryGetProperty("TieneCdr", out var tieneCdrProp) && tieneCdrProp.GetBoolean();

            // Leer fecha de emision del formulario (datos del usuario)
            DateTime? fechaEmisionUsuario = null;
            if (metadata.TryGetProperty("FechaEmision", out var fechaEmisionMeta))
            {
                if (DateTime.TryParse(fechaEmisionMeta.GetString(), out var fechaParsedMeta))
                {
                    fechaEmisionUsuario = fechaParsedMeta;
                }
            }

            // Formatear numero a 8 digitos
            var numeroFormateado = numeroUsuario;
            if (int.TryParse(numeroUsuario, out var numInt))
            {
                numeroFormateado = numInt.ToString("D8");
            }

            // Obtener produccion
            var produccion = await _produccionService.GetProduccionByGuidAsync(guidRegistro);
            if (produccion == null)
            {
                return Json(new { success = false, message = "Producción no encontrada" });
            }

            // Obtener cuenta bancaria de la entidad medica
            var validaCuentaBancaria = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_CUENTA_BANCARIA");
            var esValidaCuentaObligatoria = validaCuentaBancaria?.ToUpper() == "S";
            int? idCuentaBanco = null;

            if (produccion.IdEntidadMedica.HasValue && produccion.IdEntidadMedica.Value > 0)
            {
                var cuentasBancarias = await _entidadCuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(produccion.IdEntidadMedica.Value);
                var cuentaBancaria = cuentasBancarias.FirstOrDefault(c => c.Activo == 1);

                if (esValidaCuentaObligatoria)
                {
                    // Parametro S: la cuenta es obligatoria
                    if (cuentaBancaria == null || (string.IsNullOrEmpty(cuentaBancaria.CuentaCorriente) && string.IsNullOrEmpty(cuentaBancaria.CuentaCci)))
                    {
                        return Json(new { success = false, message = "No puede enviar facturas sin tener una cuenta bancaria registrada." });
                    }
                }

                // Si tiene cuenta, asignarla (aplica para S y N)
                if (cuentaBancaria != null)
                {
                    idCuentaBanco = cuentaBancaria.IdCuentaBancaria;
                }
            }
            else if (esValidaCuentaObligatoria)
            {
                return Json(new { success = false, message = "No puede enviar facturas sin tener una cuenta bancaria registrada." });
            }

            // Leer datos del XML
            var jsonPath = Path.Combine(tempPath, "datos.json");
            var datosJson = await System.IO.File.ReadAllTextAsync(jsonPath);
            var facturaData = JsonSerializer.Deserialize<FacturaXmlData>(datosJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (facturaData == null)
            {
                return Json(new { success = false, message = "Error al leer los datos del XML" });
            }

            // Validar concepto contra descripcion del primer item del XML (si el parametro lo requiere)
            var validaConceptoParam = await _parametroService.GetValorByCodigoAsync("SHM_VALIDA_FACTURA_CONCEPTO");
            if (validaConceptoParam?.ToUpper() == "S")
            {
                if (facturaData.DetalleItems.Count > 0)
                {
                    var descripcionPrimerItem = facturaData.DetalleItems[0].Descripcion?.Trim();
                    var conceptoProduccion = produccion.Concepto?.Trim();

                    if (!string.IsNullOrEmpty(conceptoProduccion) && !string.IsNullOrEmpty(descripcionPrimerItem))
                    {
                        if (!string.Equals(conceptoProduccion, descripcionPrimerItem, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("El concepto no coincide. Produccion: '{ConceptoProduccion}', XML: '{ConceptoXml}'",
                                conceptoProduccion, descripcionPrimerItem);
                            return Json(new
                            {
                                success = false,
                                message = $"El concepto del XML no coincide con la produccion. Esperado: '{conceptoProduccion}', XML: '{descripcionPrimerItem}'"
                            });
                        }
                    }
                }
            }

            // Determinar tipo de almacenamiento configurado (FILE o BLOB)
            var tipoAlmacenamientoParam = await _parametroService.GetValorByCodigoAsync("SHM_TIPO_ALMACENAMIENTO_ARCHIVO");
            var usarBlobStorage = tipoAlmacenamientoParam?.ToUpper() == "BLOB";

            // Preparar nombres de archivos
            var pdfFileName = $"{serieUsuario}-{numeroFormateado}.pdf";
            var xmlFileName = $"{serieUsuario}-{numeroFormateado}.xml";
            var cdrFileName = tieneCdr ? $"{serieUsuario}-{numeroFormateado}-cdr{cdrExtension}" : null;

            // Rutas de archivos temporales
            var pdfTempPath = Path.Combine(tempPath, "factura.pdf");
            var xmlTempPath = Path.Combine(tempPath, "factura.xml");
            string? cdrTempPath = tieneCdr ? Directory.GetFiles(tempPath, "cdr*").FirstOrDefault() : null;

            // Obtener tamaños de archivos desde temporales
            var pdfSize = (int)new FileInfo(pdfTempPath).Length;
            var xmlSize = (int)new FileInfo(xmlTempPath).Length;
            var cdrSize = (tieneCdr && cdrTempPath != null && System.IO.File.Exists(cdrTempPath)) ? (int)new FileInfo(cdrTempPath).Length : 0;

            // Variables para contenido BLOB (solo se usan si usarBlobStorage = true)
            byte[]? pdfContent = null;
            byte[]? xmlContent = null;
            byte[]? cdrContent = null;

            if (usarBlobStorage)
            {
                // Modo BLOB: Leer contenido de archivos temporales en memoria
                _logger.LogInformation("ConfirmarEnvio: Usando almacenamiento BLOB para archivos");

                pdfContent = await System.IO.File.ReadAllBytesAsync(pdfTempPath);
                xmlContent = await System.IO.File.ReadAllBytesAsync(xmlTempPath);

                if (tieneCdr && cdrTempPath != null)
                {
                    cdrContent = await System.IO.File.ReadAllBytesAsync(cdrTempPath);
                }
            }
            else
            {
                // Modo FILE: Copiar archivos de temporal a directorio definitivo
                _logger.LogInformation("ConfirmarEnvio: Usando almacenamiento FILE para archivos");

                uploadPath = Path.Combine(uploadBasePath, "facturas", guidRegistro);
                Directory.CreateDirectory(uploadPath);

                var pdfPath = Path.Combine(uploadPath, pdfFileName);
                var xmlPath = Path.Combine(uploadPath, xmlFileName);
                var cdrPath = cdrFileName != null ? Path.Combine(uploadPath, cdrFileName) : null;

                System.IO.File.Copy(pdfTempPath, pdfPath, true);
                archivosGuardados.Add(pdfPath);

                System.IO.File.Copy(xmlTempPath, xmlPath, true);
                archivosGuardados.Add(xmlPath);

                if (tieneCdr && cdrTempPath != null && cdrPath != null)
                {
                    System.IO.File.Copy(cdrTempPath, cdrPath, true);
                    archivosGuardados.Add(cdrPath);
                }

                // Guardar JSON con datos del XML (solo en modo FILE)
                var jsonFinalPath = Path.Combine(uploadPath, $"{serieUsuario}-{numeroFormateado}.json");
                System.IO.File.Copy(jsonPath, jsonFinalPath, true);
                archivosGuardados.Add(jsonFinalPath);
            }

            // Iniciar transaccion
            using (var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {

                // Inactivar archivos anteriores de esta produccion (si existen)
                var archivosAnteriores = await _archivoComprobanteService.GetArchivoComprobantesByProduccionAsync(produccion.IdProduccion);
                foreach (var archivoAnterior in archivosAnteriores.Where(a => a.Activo == 1))
                {
                    // Inactivar el registro ArchivoComprobante
                    await _archivoComprobanteService.DeleteArchivoComprobanteAsync(archivoAnterior.IdArchivoComprobante, userId);

                    // Inactivar el archivo asociado
                    if (archivoAnterior.IdArchivo.HasValue)
                    {
                        await _archivoService.DeleteArchivoAsync(archivoAnterior.IdArchivo.Value, userId);
                    }

                    _logger.LogInformation("ConfirmarEnvio: Archivo anterior inactivado. IdArchivoComprobante: {IdAC}, IdArchivo: {IdA}",
                        archivoAnterior.IdArchivoComprobante, archivoAnterior.IdArchivo);
                }

                // Registrar archivo PDF en BD
                var archivoPdfDto = new CreateArchivoDto
                {
                    TipoArchivo = "PDF",
                    NombreOriginal = "factura.pdf",
                    NombreArchivo = pdfFileName,
                    Extension = ".pdf",
                    Tamano = pdfSize,
                    Ruta = usarBlobStorage ? null : Path.Combine("facturas", guidRegistro, pdfFileName),
                    ContenidoArchivo = pdfContent
                };
                var archivoPdfCreado = await _archivoService.CreateArchivoAsync(archivoPdfDto, userId);

                // Registrar archivo XML en BD
                var archivoXmlDto = new CreateArchivoDto
                {
                    TipoArchivo = "XML",
                    NombreOriginal = "factura.xml",
                    NombreArchivo = xmlFileName,
                    Extension = ".xml",
                    Tamano = xmlSize,
                    Ruta = usarBlobStorage ? null : Path.Combine("facturas", guidRegistro, xmlFileName),
                    ContenidoArchivo = xmlContent
                };
                var archivoXmlCreado = await _archivoService.CreateArchivoAsync(archivoXmlDto, userId);

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

                // Registrar archivo CDR en BD solo si existe
                if (tieneCdr && cdrFileName != null)
                {
                    var archivoCdrDto = new CreateArchivoDto
                    {
                        TipoArchivo = "CDR",
                        NombreOriginal = $"cdr{cdrExtension}",
                        NombreArchivo = cdrFileName,
                        Extension = cdrExtension,
                        Tamano = cdrSize,
                        Ruta = usarBlobStorage ? null : Path.Combine("facturas", guidRegistro, cdrFileName),
                        ContenidoArchivo = cdrContent
                    };
                    var archivoCdrCreado = await _archivoService.CreateArchivoAsync(archivoCdrDto, userId);

                    await _archivoComprobanteService.CreateArchivoComprobanteAsync(new CreateArchivoComprobanteDto
                    {
                        IdProduccion = produccion.IdProduccion,
                        IdArchivo = archivoCdrCreado.IdArchivo,
                        TipoArchivo = "CDR",
                        Descripcion = "Constancia CDR"
                    }, userId);
                }

                // Actualizar produccion con datos del formulario (ingresados por el usuario)
                var updateDto = new UpdateProduccionDto
                {
                    TipoComprobante = tipoComprobanteUsuario,
                    Serie = serieUsuario,
                    Numero = numeroFormateado,
                    FechaEmision = fechaEmisionUsuario,
                    EstadoComprobante = "ENVIADO",
                    Estado = EstadoDescripcion.Produccion.FacturaEnviada,
                    Glosa = produccion.Concepto,
                    FacturaFechaEnvio = DateTime.Now,
                    IdCuentaBanco = idCuentaBanco
                };

                var result = await _produccionService.UpdateProduccionAsync(produccion.IdProduccion, updateDto, userId);

                if (!result)
                {
                    throw new InvalidOperationException("Error al actualizar la producción");
                }

                // Registrar en bitacora
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
                {
                    Entidad = "SHM_PRODUCCION",
                    IdEntidad = produccion.IdProduccion,
                    Accion = EstadoDescripcion.Produccion.FacturaEnviada,
                    Descripcion = $"Envio de comprobante de pago electrónico: {serieUsuario}-{numeroFormateado}", 
                    FechaAccion = DateTime.Now
                }, userId);

                transactionScope.Complete();

            } // Fin del TransactionScope

            // Limpiar archivos temporales
            try
            {
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
            catch (Exception cleanEx)
            {
                _logger.LogWarning(cleanEx, "No se pudo eliminar directorio temporal: {Path}", tempPath);
            }

            // Informar al sistema externo (San Pablo) si el parametro lo indica
            var enviaHhmm = await _parametroService.GetValorByCodigoAsync("SHM_COMPROBANTE_ENVIA_HHMM");

            if (enviaHhmm?.ToUpper() == "S")
            {
                var errorHhmm = await RegistrarComprobanteEnSanPabloAsync(produccion, serieUsuario, numeroFormateado, tipoComprobanteUsuario, fechaEmisionUsuario, produccion.Concepto, userId);
                if (errorHhmm != null)
                {
                    return Json(new { success = false, message = $"El comprobante se registro localmente pero no se pudo enviar a San Pablo: {errorHhmm}" });
                }
            }

            _logger.LogInformation("Factura enviada exitosamente desde vista previa. SessionId: {SessionId}", sessionId);

            return Json(new { success = true, message = "Factura enviada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar envío de factura");

            // Rollback de archivos guardados
            foreach (var archivo in archivosGuardados)
            {
                try
                {
                    if (System.IO.File.Exists(archivo))
                    {
                        System.IO.File.Delete(archivo);
                    }
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "No se pudo eliminar archivo durante rollback: {Archivo}", archivo);
                }
            }

            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Cancela la vista previa y elimina los archivos temporales.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-25</created>
    [HttpPost]
    public IActionResult CancelarVistaPrevia([FromForm] string sessionId, [FromForm] string guidRegistro)
    {
        try
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var tempPath = Path.Combine(uploadBasePath, "temp", sessionId);

                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                    _logger.LogInformation("Archivos temporales eliminados. SessionId: {SessionId}", sessionId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al eliminar archivos temporales");
        }

        return RedirectToAction(nameof(Subir), new { guid = guidRegistro });
    }

    /// <summary>
    /// Descarga un archivo por su GUID.
    /// Soporta almacenamiento FILE y BLOB automaticamente.
    ///
    /// <modified>ADG Vladimir - 2026-01-29 - Soporte para almacenamiento dual FILE/BLOB</modified>
    /// </summary>
    // GET: Facturas/DescargarArchivo
    public async Task<IActionResult> DescargarArchivo(string guid)
    {
        try
        {
            if (string.IsNullOrEmpty(guid))
            {
                return NotFound("Archivo no encontrado");
            }

            // Usar el nuevo metodo que maneja ambos tipos de almacenamiento (FILE y BLOB)
            var archivoContenido = await _archivoService.GetArchivoContenidoByGuidAsync(guid);
            if (archivoContenido == null)
            {
                _logger.LogWarning("Archivo no encontrado o sin contenido: {Guid}", guid);
                return NotFound("Archivo no encontrado");
            }

            // Para PDFs, mostrar inline en el navegador (visor embebido)
            // Para otros archivos, forzar descarga
            if (archivoContenido.Extension?.ToLower() == ".pdf")
            {
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{archivoContenido.NombreArchivo}\"");
                return File(archivoContenido.Contenido, archivoContenido.ContentType ?? "application/pdf");
            }

            return File(
                archivoContenido.Contenido,
                archivoContenido.ContentType ?? "application/octet-stream",
                archivoContenido.NombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo con GUID: {Guid}", guid);
            return StatusCode(500, "Error al descargar el archivo");
        }
    }

    /// <summary>
    /// Registra el comprobante en el sistema externo de San Pablo.
    /// Se ejecuta despues de confirmar la transaccion local.
    /// Retorna null si fue exitoso, o el mensaje de error si fallo.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-02-26</created>
    private async Task<string?> RegistrarComprobanteEnSanPabloAsync(
        ProduccionListaResponseDto produccion,
        string serie,
        string numero,
        string tipoComprobante,
        DateTime? fechaEmision,
        string? glosa,
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

            // Si TipoComprobante es nulo o vacio, derivar de TipoEntidadMedica
            var tipoComprobanteCalculado = false;
            if (string.IsNullOrEmpty(tipoComprobante))
            {
                tipoComprobante = produccion.TipoEntidadMedica == "1" ? "1" : "22";
                tipoComprobanteCalculado = true;
            }

            // Si concepto (glosa) es nulo o vacio, construir desde tipo de produccion
            var conceptoCalculado = false;
            if (string.IsNullOrEmpty(glosa))
            {
                var detalleTipoProd = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_PRODUCCION", produccion.TipoProduccion ?? "");
                var descripcionTipoProd = detalleTipoProd?.Descripcion?.ToUpper() ?? produccion.TipoProduccion ?? "";
                glosa = $"PRODUCCION {produccion.CodigoProduccion} - {descripcionTipoProd}";
                conceptoCalculado = true;
            }

            // Persistir valores calculados en la produccion
            if (tipoComprobanteCalculado || conceptoCalculado)
            {
                var updateCalculados = new UpdateProduccionDto();
                if (tipoComprobanteCalculado) updateCalculados.TipoComprobante = tipoComprobante;
                if (conceptoCalculado) updateCalculados.Concepto = glosa;
                await _produccionService.UpdateProduccionAsync(produccion.IdProduccion, updateCalculados, userId);
            }

            // Obtener descripcion del tipo de comprobante desde tabla maestra (01=Factura, 22=RHE)
            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByCodigoAsync("TIPO_COMPROBANTE", tipoComprobante);
            var descripcionTipo = tablaDetalle?.Descripcion ?? tipoComprobante;

            // FLG_CIAMEDICA: 0=MEDICO, 1=CIA MEDICA
            var flgCiaMedica = produccion.TipoEntidadMedica == "1" ? "1" : "0";

            // Formatear numero a 7 digitos
            var numeroFormateado7 = numero;
            if (int.TryParse(numero, out var numInt))
            {
                numeroFormateado7 = numInt.ToString("D7");
            }

            var request = new SanPabloComprobanteRequestDto
            {
                COD_SEDE = produccion.CodigoSede,
                FLG_CIAMEDICA = flgCiaMedica,
                COD_ENTIDAD = codigoEntidad,
                COD_PROD = produccion.CodigoProduccion,
                FLG_PORTAL = "FA",
                CPM_TIPO = tipoComprobante,
                CPM_SERIE = serie,
                CPM_NUMERO = numeroFormateado7,
                CPM_FECEMI = fechaEmision?.ToString("dd/MM/yyyy"),
                CPM_GLOSA = glosa,
                CPM_MTOTAL = produccion.MtoTotal?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                CPM_FECREG = DateTime.Now.ToString("dd/MM/yyyy")
            };

            var response = await _sanPabloApiService.RegistrarComprobanteAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Comprobante registrado en San Pablo exitosamente. CodigoProduccion: {CodigoProd}, Tipo: {Tipo}, Serie: {Serie}, Numero: {Numero}",
                    produccion.CodigoProduccion, descripcionTipo, serie, numero);

                // Actualizar estado de produccion a FACTURA_ENVIADA_HHMM
                await _produccionService.UpdateProduccionAsync(produccion.IdProduccion, new UpdateProduccionDto
                {
                    Estado = EstadoDescripcion.Produccion.FacturaEnviadaHhmm
                }, userId);

                // Registrar en bitacora
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
                {
                    Entidad = "SHM_PRODUCCION",
                    IdEntidad = produccion.IdProduccion,
                    Accion = EstadoDescripcion.Produccion.FacturaEnviadaHhmm,
                    Descripcion = $"{descripcionTipo} enviada a HHMM: {serie}-{numero}",
                    FechaAccion = DateTime.Now
                }, userId);

                return null; // Exito
            }
            else
            {
                _logger.LogWarning("Error al registrar comprobante en San Pablo. CodigoProduccion: {CodigoProd}, Mensaje: {Mensaje}",
                    produccion.CodigoProduccion, response.Message);

                // Revertir estado y datos del comprobante
                await RevertirProduccionAsync(produccion, userId);

                // Registrar error en bitacora
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
                {
                    Entidad = "SHM_PRODUCCION",
                    IdEntidad = produccion.IdProduccion,
                    Accion = "ERROR_ENVIO_HHMM",
                    Descripcion = $"Error al enviar {descripcionTipo} a HHMM: {response.Message}",
                    FechaAccion = DateTime.Now
                }, userId);

                return response.Message;
            }
        }
        catch (Exception ex)
        {
            // No lanzar excepcion para no afectar el flujo principal
            _logger.LogError(ex, "Error al comunicar comprobante a San Pablo. CodigoProduccion: {CodigoProd}",
                produccion.CodigoProduccion);

            try
            {
                // Revertir estado y datos del comprobante
                await RevertirProduccionAsync(produccion, userId);

                // Registrar error en bitacora
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraDto
                {
                    Entidad = "SHM_PRODUCCION",
                    IdEntidad = produccion.IdProduccion,
                    Accion = "ERROR_ENVIO_HHMM",
                    Descripcion = $"Error al enviar comprobante a HHMM: {ex.Message}",
                    FechaAccion = DateTime.Now
                }, userId);
            }
            catch (Exception revertEx)
            {
                _logger.LogError(revertEx, "Error al revertir produccion. CodigoProduccion: {CodigoProd}",
                    produccion.CodigoProduccion);
            }

            return ex.Message;
        }
    }

    /// <summary>
    /// Revierte el estado y datos del comprobante de una produccion al estado FacturaSolicitada.
    /// Se ejecuta cuando falla el envio al sistema externo de San Pablo.
    /// </summary>
    /// <author>ADG Antonio</author>
    /// <created>2026-03-01</created>
    private async Task RevertirProduccionAsync(ProduccionListaResponseDto produccion, int userId)
    {
        // Limpia: Serie, Numero, FechaEmision, Glosa, EstadoComprobante, FacturaFechaEnvio
        // y regresa al estado FacturaSolicitada
        await _produccionService.RevertComprobanteAsync(
            produccion.IdProduccion,
            EstadoDescripcion.Produccion.FacturaSolicitada,
            userId);
    }
}
