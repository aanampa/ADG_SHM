using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SHM.AppWebHonorarioMedico.Controllers;


/// <summary>
/// Controlador para la gestion de producciones en el portal administrativo.
///
/// <author>ADG Antonio</author>
/// <created>2025-02-03</created>
/// </summary>
[Authorize]
public class OrdenPagoController : Controller
{
    private readonly ILogger<OrdenPagoController> _logger;
    private readonly IConfiguration _configuration;

    public OrdenPagoController(
       ILogger<OrdenPagoController> logger,
       IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }
}