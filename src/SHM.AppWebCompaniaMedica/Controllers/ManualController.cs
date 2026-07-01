using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SHM.AppWebCompaniaMedica.Controllers;

[Authorize]
public class ManualController : BaseController
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Manual de Usuario";
        return View();
    }
}
