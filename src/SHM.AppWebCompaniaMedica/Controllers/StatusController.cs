using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SHM.AppDomain.Interfaces.Services;
using System.Text.Json;

namespace SHM.AppWebCompaniaMedica.Controllers;

/// <summary>
/// Controlador para verificar el estado del portal y sus dependencias.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-06</created>
/// </summary>
public class StatusController : Controller
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ISanPabloApiService _sanPabloApiService;
    private readonly ISapApiService _sapApiService;
    private readonly ILogger<StatusController> _logger;

    public StatusController(
        HealthCheckService healthCheckService,
        ISanPabloApiService sanPabloApiService,
        ISapApiService sapApiService,
        ILogger<StatusController> logger)
    {
        _healthCheckService = healthCheckService;
        _sanPabloApiService = sanPabloApiService;
        _sapApiService = sapApiService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica el estado de la base de datos y los servicios externos.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Consulta de estado del portal de companias medicas");

        // Verificar BD Oracle y servicios externos en paralelo
        var taskDb       = _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("db"));
        var taskSanPablo = _sanPabloApiService.CheckConnectionAsync();
        var taskSap      = _sapApiService.CheckConnectionAsync();

        await Task.WhenAll(taskDb, taskSanPablo, taskSap);

        // BD Oracle
        var dbEntry   = taskDb.Result.Entries.FirstOrDefault();
        var dbStatus  = dbEntry.Value.Status == HealthStatus.Healthy ? "OK" : "ERROR";
        var dbMessage = dbEntry.Value.Description ?? "Sin descripcion";

        var status = new
        {
            database = new
            {
                status  = dbStatus,
                message = dbMessage
            },
            serviciosExternos = new
            {
                sanPablo = new
                {
                    status  = taskSanPablo.Result.Ok ? "OK" : "ERROR",
                    mensaje = taskSanPablo.Result.Mensaje
                },
                sap = new
                {
                    status  = taskSap.Result.Ok ? "OK" : "ERROR",
                    mensaje = taskSap.Result.Mensaje
                }
            }
        };

        var json = JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });
        return Content(json, "application/json");
    }
}
