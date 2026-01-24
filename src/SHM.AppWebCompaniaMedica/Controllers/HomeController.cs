using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebCompaniaMedica.Models;

namespace SHM.AppWebCompaniaMedica.Controllers;

/// <summary>
/// Controlador principal del portal de companias medicas.
/// Gestiona el Dashboard con estadisticas dinamicas.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-24</created>
/// </summary>
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProduccionService _produccionService;

    public HomeController(ILogger<HomeController> logger, IProduccionService produccionService)
    {
        _logger = logger;
        _produccionService = produccionService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Dashboard");
    }

    /// <summary>
    /// Muestra el Dashboard con estadisticas de producciones por entidad medica.
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "Dashboard";

        var model = new DashboardViewModel();

        // Obtener IdEntidadMedica del usuario logueado
        var idEntidadMedicaClaim = User.FindFirstValue("IdEntidadMedica");
        if (!int.TryParse(idEntidadMedicaClaim, out var idEntidadMedica) || idEntidadMedica == 0)
        {
            _logger.LogWarning("Usuario sin IdEntidadMedica asignado");
            return View(model);
        }

        try
        {
            // Obtener estadisticas principales
            var stats = await _produccionService.GetDashboardStatsAsync(idEntidadMedica);
            model.TotalPorFacturar = stats.TotalPorFacturar;
            model.FacturasPendientes = stats.Pendientes;
            model.CantidadPendienteFactura = stats.Pendientes;
            model.CantidadFacturaEnviada = stats.Enviadas;
            model.CantidadFacturaEnviadaHHMM = stats.EnviadasHHMM;
            model.CantidadFacturaPagada = stats.Pagadas;

            // Obtener facturas enviadas del mes actual
            model.FacturasEnviadasMes = await _produccionService.GetFacturasEnviadasMesActualAsync(idEntidadMedica);

            // Obtener datos para grafico de barras (ultimos 6 meses)
            var facturasPorMes = await _produccionService.GetFacturasPorMesAsync(idEntidadMedica);
            model.FacturasPorMes = facturasPorMes.Select(f => new FacturasPorMesViewModel
            {
                Mes = ObtenerNombreMes(f.Mes),
                Anio = f.Anio,
                FacturasEnviadas = f.Enviadas,
                FacturasPendientes = f.Pendientes
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadisticas del dashboard");
        }

        return View(model);
    }

    /// <summary>
    /// Obtiene el nombre del mes en espanol.
    /// </summary>
    private static string ObtenerNombreMes(int mes)
    {
        var cultura = new CultureInfo("es-PE");
        var fecha = new DateTime(2024, mes, 1);
        var nombreMes = fecha.ToString("MMMM", cultura);
        return char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
