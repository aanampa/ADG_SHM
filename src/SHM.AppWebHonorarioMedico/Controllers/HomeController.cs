using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUsuarioService _usuarioService;

    public HomeController(ILogger<HomeController> logger, IUsuarioService usuarioService)
    {
        _logger = logger;
        _usuarioService = usuarioService;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Accediendo a la página principal");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Accediendo a la página de privacidad");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> MisDatos()
    {
        var userIdClaim = User.FindFirstValue("IdUsuario");
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idUsuario))
        {
            _logger.LogWarning("No se pudo obtener el IdUsuario del usuario autenticado");
            return RedirectToAction("Login", "Auth");
        }

        var usuario = await _usuarioService.GetUsuarioByIdAsync(idUsuario);
        if (usuario == null)
        {
            _logger.LogWarning("Usuario con ID {IdUsuario} no encontrado", idUsuario);
            return RedirectToAction("Login", "Auth");
        }

        var model = new MisDatosViewModel
        {
            IdUsuario = usuario.IdUsuario,
            Login = usuario.Login,
            TipoUsuario = usuario.TipoUsuario,
            Nombres = usuario.Nombres,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            Email = usuario.Email,
            NumeroDocumento = usuario.NumeroDocumento,
            Celular = usuario.Celular,
            FechaCreacion = usuario.FechaCreacion,
            FechaModificacion = usuario.FechaModificacion
        };

        _logger.LogInformation("Usuario {Login} accediendo a Mis Datos", usuario.Login);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MisDatos(MisDatosViewModel model)
    {
        var userIdClaim = User.FindFirstValue("IdUsuario");
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idUsuario))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Asegurar que el usuario solo pueda modificar sus propios datos
        if (model.IdUsuario != idUsuario)
        {
            _logger.LogWarning("Intento de modificar datos de otro usuario. IdUsuario: {IdUsuario}, IdModel: {IdModel}", idUsuario, model.IdUsuario);
            return Forbid();
        }

        // Validar cambio de contraseña si se solicita
        if (model.CambiarPassword)
        {
            if (string.IsNullOrEmpty(model.PasswordActual))
            {
                ModelState.AddModelError("PasswordActual", "Debe ingresar su contraseña actual");
            }
            if (string.IsNullOrEmpty(model.PasswordNueva))
            {
                ModelState.AddModelError("PasswordNueva", "Debe ingresar la nueva contraseña");
            }
            if (model.PasswordNueva != model.ConfirmarPassword)
            {
                ModelState.AddModelError("ConfirmarPassword", "Las contraseñas no coinciden");
            }
        }

        // Remover validaciones de campos de contraseña si no se va a cambiar
        if (!model.CambiarPassword)
        {
            ModelState.Remove("PasswordActual");
            ModelState.Remove("PasswordNueva");
            ModelState.Remove("ConfirmarPassword");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Actualizar datos personales
            var (successDatos, errorDatos) = await _usuarioService.ActualizarMisDatosAsync(
                idUsuario,
                model.Nombres,
                model.ApellidoPaterno,
                model.ApellidoMaterno,
                model.Email,
                model.NumeroDocumento,
                model.Celular);

            if (!successDatos)
            {
                _logger.LogWarning("Error al actualizar datos del usuario {IdUsuario}: {Error}", idUsuario, errorDatos);
                ViewBag.ErrorMessage = errorDatos;
                return View(model);
            }

            // Cambiar contraseña si se solicitó
            if (model.CambiarPassword && !string.IsNullOrEmpty(model.PasswordActual) && !string.IsNullOrEmpty(model.PasswordNueva))
            {
                var (successPassword, errorPassword) = await _usuarioService.CambiarPasswordAsync(
                    idUsuario,
                    model.PasswordActual,
                    model.PasswordNueva);

                if (!successPassword)
                {
                    _logger.LogWarning("Error al cambiar contraseña del usuario {IdUsuario}: {Error}", idUsuario, errorPassword);
                    ViewBag.ErrorMessage = errorPassword;
                    return View(model);
                }

                _logger.LogInformation("Usuario {IdUsuario} cambió su contraseña exitosamente", idUsuario);
            }

            _logger.LogInformation("Usuario {IdUsuario} actualizó sus datos exitosamente", idUsuario);
            ViewBag.SuccessMessage = "Sus datos han sido actualizados correctamente";

            // Recargar datos actualizados
            var usuarioActualizado = await _usuarioService.GetUsuarioByIdAsync(idUsuario);
            if (usuarioActualizado != null)
            {
                model.FechaModificacion = usuarioActualizado.FechaModificacion;
            }

            // Limpiar campos de contraseña
            model.PasswordActual = null;
            model.PasswordNueva = null;
            model.ConfirmarPassword = null;
            model.CambiarPassword = false;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar datos del usuario {IdUsuario}", idUsuario);
            ViewBag.ErrorMessage = "Ocurrió un error al procesar la solicitud";
            return View(model);
        }
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("Se ha producido un error - RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
