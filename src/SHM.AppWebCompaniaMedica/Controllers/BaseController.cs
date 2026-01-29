using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SHM.AppWebCompaniaMedica.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        // Cargar datos de Claims en ViewData para las vistas
        var nombres = User.FindFirstValue("Nombres") ?? "";
        var apellidoPaterno = User.FindFirstValue("ApellidoPaterno") ?? "";
        var apellidoMaterno = User.FindFirstValue("ApellidoMaterno") ?? "";
        var email = User.FindFirstValue(ClaimTypes.Email) ?? "";

        var nombreCompleto = $"{nombres} {apellidoPaterno}".Trim();
        if (string.IsNullOrEmpty(nombreCompleto))
        {
            nombreCompleto = User.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
        }

        // Calcular iniciales
        var iniciales = "";
        if (!string.IsNullOrEmpty(nombres) && nombres.Length > 0)
            iniciales += nombres[0];
        if (!string.IsNullOrEmpty(apellidoPaterno) && apellidoPaterno.Length > 0)
            iniciales += apellidoPaterno[0];

        if (string.IsNullOrEmpty(iniciales))
            iniciales = "U";

        ViewData["Usuario"] = nombreCompleto;
        ViewData["Email"] = email;
        ViewData["Iniciales"] = iniciales.ToUpper();
        ViewData["RazonSocial"] = User.FindFirstValue("RazonSocial") ?? "Portal de Honorarios";

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
            ViewData["UserId"] = userId;

        ViewData["UserTipo"] = User.FindFirstValue("TipoUsuario");
    }
}
