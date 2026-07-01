using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApiHonorarioMedico.Controllers;

/// <summary>
/// Controlador para verificar el estado del API y sus dependencias.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-21</created>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private static readonly DateTime _startTime = DateTime.Now;
    private readonly HealthCheckService _healthCheckService;
    private readonly ISapApiService _sapApiService;
    private readonly ISanPabloApiService _sanPabloApiService;
    private readonly ILogger<StatusController> _logger;

    public StatusController(
        HealthCheckService healthCheckService,
        ISapApiService sapApiService,
        ISanPabloApiService sanPabloApiService,
        ILogger<StatusController> logger)
    {
        _healthCheckService = healthCheckService;
        _sapApiService = sapApiService;
        _sanPabloApiService = sanPabloApiService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica el estado del API, la conexion a Oracle Database y los servicios externos.
    /// Retorna 200 OK si el API esta corriendo, independientemente del estado de las dependencias.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        _logger.LogInformation("Consulta de estado del API");

        // Uptime del proceso
        var uptime = DateTime.Now - _startTime;
        var uptimeStr = $"{(int)uptime.TotalHours}h {uptime.Minutes}m {uptime.Seconds}s";

        // Verificar BD Oracle y servicios externos en paralelo
        var taskDb       = _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("db"));
        var taskSap      = _sapApiService.CheckConnectionAsync();
        var taskSanPablo = _sanPabloApiService.CheckConnectionAsync();

        await Task.WhenAll(taskDb, taskSap, taskSanPablo);

        // BD Oracle
        var dbEntry   = taskDb.Result.Entries.FirstOrDefault();
        var dbStatus  = dbEntry.Value.Status == HealthStatus.Healthy ? "OK" : "ERROR";
        var dbMessage = dbEntry.Value.Description ?? "Sin descripcion";

        var status = new
        {
            status = "OK",
            api = "SHM Honorario Medico API",
            version = "1.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "No definido",
            serverDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            uptime = uptimeStr,
            database = new
            {
                status  = dbStatus,
                message = dbMessage
            },
            serviciosExternos = new
            {
                sap = new
                {
                    status  = taskSap.Result.Ok ? "OK" : "ERROR",
                    mensaje = taskSap.Result.Mensaje
                },
                sanPablo = new
                {
                    status  = taskSanPablo.Result.Ok ? "OK" : "ERROR",
                    mensaje = taskSanPablo.Result.Mensaje
                }
            }
        };

        return Ok(status);
    }
}
