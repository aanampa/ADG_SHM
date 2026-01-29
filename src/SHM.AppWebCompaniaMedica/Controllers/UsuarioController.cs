using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebCompaniaMedica.Models;

namespace SHM.AppWebCompaniaMedica.Controllers;

public class UsuarioController : BaseController
{
    private readonly IUsuarioService _usuarioService;
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly IEntidadCuentaBancariaService _cuentaBancariaService;
    private readonly IBancoService _bancoService;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(
        IUsuarioService usuarioService,
        IEntidadMedicaService entidadMedicaService,
        IEntidadCuentaBancariaService cuentaBancariaService,
        IBancoService bancoService,
        ILogger<UsuarioController> logger)
    {
        _usuarioService = usuarioService;
        _entidadMedicaService = entidadMedicaService;
        _cuentaBancariaService = cuentaBancariaService;
        _bancoService = bancoService;
        _logger = logger;
    }

    // GET: Usuario/Perfil
    public async Task<IActionResult> Perfil()
    {
        ViewData["Title"] = "Mi Perfil";

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var usuario = await _usuarioService.GetUsuarioByIdAsync(userId);
            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new PerfilViewModel
            {
                Login = usuario.Login,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                NombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}".Trim(),
                Email = usuario.Email,
                NumeroDocumento = usuario.NumeroDocumento,
                Celular = usuario.Celular,
                Telefono = usuario.Telefono,
                Cargo = usuario.Cargo,
                FechaCreacion = usuario.FechaCreacion
            };

            // Calcular iniciales
            var iniciales = "";
            if (!string.IsNullOrEmpty(usuario.Nombres) && usuario.Nombres.Length > 0)
                iniciales += usuario.Nombres[0];
            if (!string.IsNullOrEmpty(usuario.ApellidoPaterno) && usuario.ApellidoPaterno.Length > 0)
                iniciales += usuario.ApellidoPaterno[0];
            model.Iniciales = string.IsNullOrEmpty(iniciales) ? "U" : iniciales.ToUpper();

            // Cargar datos de la entidad medica si existe
            if (usuario.IdEntidadMedica.HasValue)
            {
                var entidad = await _entidadMedicaService.GetEntidadMedicaByIdAsync(usuario.IdEntidadMedica.Value);
                if (entidad != null)
                {
                    model.CodigoEntidad = entidad.CodigoEntidad;
                    model.RazonSocial = entidad.RazonSocial;
                    model.Ruc = entidad.Ruc;
                    model.DireccionEntidad = entidad.Direccion;
                    model.TelefonoEntidad = entidad.Telefono;
                    model.CelularEntidad = entidad.Celular;

                    // Cargar cuentas bancarias de la entidad medica
                    var cuentasBancarias = await _cuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(usuario.IdEntidadMedica.Value);
                    foreach (var cuenta in cuentasBancarias.Where(c => c.Activo == 1))
                    {
                        var cuentaVm = new CuentaBancariaViewModel
                        {
                            CuentaCorriente = cuenta.CuentaCorriente,
                            CuentaCci = cuenta.CuentaCci,
                            Moneda = cuenta.Moneda
                        };

                        // Obtener nombre del banco
                        if (cuenta.IdBanco.HasValue)
                        {
                            var banco = await _bancoService.GetBancoByIdAsync(cuenta.IdBanco.Value);
                            cuentaVm.NombreBanco = banco?.NombreBanco;
                        }

                        model.CuentasBancarias.Add(cuentaVm);
                    }
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar perfil de usuario");
            return View(new PerfilViewModel());
        }
    }

    // GET: Usuario/Configuracion
    public IActionResult Configuracion()
    {
        ViewData["Title"] = "Configuracion";
        return View();
    }

    // POST: Usuario/ActualizarPerfil
    [HttpPost]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilViewModel model)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var (success, errorMessage) = await _usuarioService.ActualizarMisDatosAsync(
                userId,
                model.Nombres,
                model.ApellidoPaterno,
                model.ApellidoMaterno,
                model.Email,
                model.NumeroDocumento,
                model.Celular);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage ?? "Error al actualizar los datos" });
            }

            return Json(new { success = true, message = "Datos actualizados correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil");
            return Json(new { success = false, message = "Error al actualizar los datos" });
        }
    }

    // POST: Usuario/CambiarPassword
    [HttpPost]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordViewModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.PasswordActual) || string.IsNullOrEmpty(model.PasswordNueva))
            {
                return Json(new { success = false, message = "Todos los campos son requeridos" });
            }

            if (model.PasswordNueva != model.ConfirmarPassword)
            {
                return Json(new { success = false, message = "Las contrasenas no coinciden" });
            }

            if (model.PasswordNueva.Length < 8)
            {
                return Json(new { success = false, message = "La contrasena debe tener al menos 8 caracteres" });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var (success, errorMessage) = await _usuarioService.CambiarPasswordAsync(
                userId,
                model.PasswordActual,
                model.PasswordNueva);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage ?? "Error al cambiar la contrasena" });
            }

            return Json(new { success = true, message = "Contrasena actualizada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contrasena");
            return Json(new { success = false, message = "Error al cambiar la contrasena" });
        }
    }

    // POST: Usuario/ActualizarConfiguracion
    [HttpPost]
    public IActionResult ActualizarConfiguracion()
    {
        return Json(new { success = true, message = "Configuracion actualizada exitosamente" });
    }
}
