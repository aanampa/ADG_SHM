using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la vista de material de capacitacion (manuales y videos).
/// Accesible tanto de forma publica (sin autenticacion) como desde el menu del sistema ya autenticado.
///
/// <author>Claude</author>
/// <created>2026-06-29</created>
/// </summary>
[AllowAnonymous]
public class ManualesController : Controller
{
    [HttpGet]
    [Route("/Publico/Manuales")]
    [Route("/Home/Manuales")]
    public IActionResult Index()
    {
        return View();
    }
}
