    using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;
using System.Security.Claims;

namespace SHM.AppWebHonorarioMedico.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUsuarioService _usuarioService;
    private readonly IEmailService _emailService;
    private readonly IUsuarioSedeRepository _usuarioSedeRepository;

    public AuthController(
        ILogger<AuthController> logger,
        IConfiguration configuration,
        IUsuarioService usuarioService,
        IEmailService emailService,
        IUsuarioSedeRepository usuarioSedeRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _usuarioService = usuarioService;
        _emailService = emailService;
        _usuarioSedeRepository = usuarioSedeRepository;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        // Si ya está autenticado, redirigir al Home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        LoginView model = new LoginView();
        SetModelView(model);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            ViewBag.LoginMessage = "Tu sesión ha expirado. Por favor, inicia sesión nuevamente.";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginView model)
    {
        string instancia = _configuration["AppSettings:InstanceName"] ?? "";

        try
        {
            // Limpiar username si viene con @
            if (!string.IsNullOrEmpty(model.Username) && model.Username.Contains("@"))
            {
                model.Username = model.Username.Split('@')[0];
            }

            // Validar campos requeridos
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                SetModelView(model);
                ViewBag.LoginMessage = "Usuario y contraseña son requeridos";
                return View(model);
            }

            _logger.LogInformation("Intento de login para usuario: {Username}", model.Username);

            // Modo PROTOTIPO - login sin validación real
            if (instancia == "PROTOTIPO")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "9999"),
                    new Claim(ClaimTypes.Name, model.Username.ToUpper().Trim()),
                    new Claim("Usuario", model.Username.ToUpper().Trim()),
                    new Claim("NombreUsuario", "Usuario Demo"),
                    new Claim("IdUsuario", "9999"),
                    new Claim("Email", "demo@mail.com"),
                    new Claim("IdSede", "999"),
                    new Claim("NombreSede", "Sede Demo"),
                    new Claim("IdArea", "0"),
                    new Claim("NombreArea", "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    }
                );

                _logger.LogInformation("Login PROTOTIPO exitoso para usuario: {Username}", model.Username);
                return RedirectToAction("Index", "Home");
            }

            // Modo DEV - login sin validación de contraseña pero con datos reales
            if (instancia == "DEV")
            {
                // Obtener usuario solo por login (sin validar password)
                var usuarioDev = await _usuarioService.GetUsuarioByLoginAsync(model.Username);

                if (usuarioDev == null)
                {
                    _logger.LogWarning("Usuario no encontrado en modo DEV: {Username}", model.Username);
                    SetModelView(model);
                    ViewBag.LoginMessage = "Usuario no encontrado";
                    return View(model);
                }

                if (usuarioDev.Activo != 1)
                {
                    SetModelView(model);
                    ViewBag.LoginMessage = "Usuario inactivo";
                    return View(model);
                }

                // Obtener sede del usuario (prioridad: ES_ULTIMA_SEDE = 1, sino el primer registro)
                var sedeDevInfo = await _usuarioSedeRepository.GetSedeSeleccionadaLoginAsync(usuarioDev.IdUsuario);
                var idSedeDev = sedeDevInfo?.IdSede.ToString() ?? "0";
                var nombreSedeDev = sedeDevInfo?.NombreSede ?? "";

                // Crear Claims con datos reales del usuario
                var devClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioDev.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuarioDev.Login),
                    new Claim("Usuario", usuarioDev.Login.ToUpper()),
                    new Claim("NombreUsuario", $"{usuarioDev.Nombres} {usuarioDev.ApellidoPaterno}".Trim()),
                    new Claim("IdUsuario", usuarioDev.IdUsuario.ToString()),
                    new Claim("Email", usuarioDev.Email ?? ""),
                    new Claim("IdSede", idSedeDev),
                    new Claim("NombreSede", nombreSedeDev),
                    new Claim("IdArea", "0"),
                    new Claim("NombreArea", "")
                };

                if (usuarioDev.IdRol.HasValue)
                    devClaims.Add(new Claim(ClaimTypes.Role, usuarioDev.IdRol.Value.ToString()));

                if (usuarioDev.IdEntidadMedica.HasValue)
                    devClaims.Add(new Claim("IdEntidadMedica", usuarioDev.IdEntidadMedica.Value.ToString()));

                var devIdentity = new ClaimsIdentity(devClaims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(devIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    }
                );

                _logger.LogInformation("Login DEV exitoso para usuario: {Username} (ID: {UserId})",
                    model.Username, usuarioDev.IdUsuario);
                return RedirectToAction("Index", "Home");
            }

            // Validar credenciales contra la base de datos
            var usuario = await _usuarioService.ValidarCredencialesAsync(model.Username, model.Password);

            if (usuario == null)
            {
                _logger.LogWarning("Intento de login fallido para usuario: {Username}", model.Username);
                SetModelView(model);
                ViewBag.LoginMessage = "Credenciales ingresadas son inválidas";
                return View(model);
            }

            if (usuario.Activo != 1)
            {
                SetModelView(model);
                ViewBag.LoginMessage = "Usuario inactivo";
                return View(model);
            }

            // Obtener sede del usuario (prioridad: ES_ULTIMA_SEDE = 1, sino el primer registro)
            var sedeInfo = await _usuarioSedeRepository.GetSedeSeleccionadaLoginAsync(usuario.IdUsuario);
            var idSede = sedeInfo?.IdSede.ToString() ?? "0";
            var nombreSede = sedeInfo?.NombreSede ?? "";

            // Crear Claims para el usuario autenticado
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Login),
                new Claim("Usuario", usuario.Login.ToUpper()),
                new Claim("NombreUsuario", $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim()),
                new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                new Claim("Email", usuario.Email ?? ""),
                new Claim("IdSede", idSede),
                new Claim("NombreSede", nombreSede),
                new Claim("IdArea", "0"),
                new Claim("NombreArea", "")
            };

            if (usuario.IdRol.HasValue)
                userClaims.Add(new Claim(ClaimTypes.Role, usuario.IdRol.Value.ToString()));

            if (usuario.IdEntidadMedica.HasValue)
                userClaims.Add(new Claim("IdEntidadMedica", usuario.IdEntidadMedica.Value.ToString()));

            var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                }
            );

            _logger.LogInformation("Login exitoso para usuario: {Username} (ID: {UserId})", model.Username, usuario.IdUsuario);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login para usuario: {Username}", model.Username);
            SetModelView(model);
            ViewBag.LoginMessage = "Ocurrió un error al procesar la solicitud";
            return View(model);
        }
    }

    [NonAction]
    private void SetModelView(LoginView model)
    {
        string instancia = _configuration["AppSettings:InstanceName"] ?? "";

        model.InstanceName = instancia;
        model.Year = _configuration["AppSettings:Year"] ?? "";
        model.CompanyName = _configuration["AppSettings:CompanyName"] ?? "";
        model.Version = _configuration["AppSettings:Version"] ?? "";
        model.WebRootPath = Url.Action("", "") ?? "";

        if (model.WebRootPath == "/")
            model.WebRootPath = "";

        model.InstanceDescription = "";

        switch (instancia.ToUpper())
        {
            case "PROD":
                model.LoginBackgroundColor = _configuration["AppSettings:LoginBackgroundColor"] ?? "";
                break;
            case "DEV":
                model.LoginBackgroundColor = _configuration["AppSettings:LoginBackgroundColorDEV"] ?? "";
                model.InstanceDescription = "INSTANCIA DE DESARROLLO";
                break;
            case "TEST":
                model.LoginBackgroundColor = _configuration["AppSettings:LoginBackgroundColorTEST"] ?? "";
                model.InstanceDescription = "INSTANCIA DE PRUEBAS";
                break;
            case "QA":
                model.LoginBackgroundColor = _configuration["AppSettings:LoginBackgroundColorQA"] ?? "";
                model.InstanceDescription = "INSTANCIA QA";
                break;
            default:
                model.LoginBackgroundColor = _configuration["AppSettings:LoginBackgroundColorDEV"] ?? "";
                model.InstanceDescription = $"INSTANCIA {instancia.ToUpper()}";
                break;
        }
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userLogin = User.FindFirstValue(ClaimTypes.Name);

        // Limpiar la sesion (incluye cache del menu)
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("Logout exitoso para usuario: {UserLogin} (ID: {UserId})", userLogin, userId);

        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult RecuperarClave()
    {
        LoginView model = new LoginView();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarClave(LoginView model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                ViewBag.LoginMessage = "Por favor ingresa tu usuario o correo electrónico";
                return View(model);
            }

            _logger.LogInformation("Solicitud de recuperación de contraseña para: {Username}", model.Username);

            // Generar token de recuperación
            var (success, token, email, nombres) = await _usuarioService.GenerarTokenRecuperacionAsync(model.Username);

            _logger.LogInformation("Resultado GenerarToken - Success: {Success}, Token: {Token}, Email: {Email}, Nombres: {Nombres}",
                success, token ?? "NULL", email ?? "NULL", nombres ?? "NULL");

            if (success && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(email))
            {
                // Obtener URL base
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                _logger.LogInformation("BaseUrl: {BaseUrl}", baseUrl);

                // Enviar email
                var emailSent = await _emailService.EnviarEmailRecuperacionAsync(email, nombres ?? "Usuario", token, baseUrl);

                _logger.LogInformation("Email enviado: {EmailSent}", emailSent);

                if (!emailSent)
                {
                    _logger.LogWarning("No se pudo enviar el email de recuperación a: {Email}", email);
                }
            }
            else
            {
                _logger.LogWarning("No se generó token - Usuario no encontrado o sin email. Username: {Username}", model.Username);
            }

            // Por seguridad, siempre mostramos el mismo mensaje
            ViewBag.LoginMessage = "Si el usuario existe y tiene un correo registrado, se han enviado las instrucciones de recuperación.";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar recuperación de contraseña para: {Username}", model.Username);
            ViewBag.LoginMessage = "Ocurrió un error al procesar la solicitud";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RestablecerClave(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login");
        }

        // Validar token
        var (isValid, errorMessage) = await _usuarioService.ValidarTokenRecuperacionAsync(token);

        if (!isValid)
        {
            ViewBag.LoginMessage = errorMessage ?? "Token inválido o expirado";
            return View("RecuperarClave", new LoginView());
        }

        var model = new LoginView
        {
            ResetCode = token
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerClave(LoginView model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.ResetCode))
            {
                ViewBag.LoginMessage = "Token de recuperación inválido";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ViewBag.LoginMessage = "La contraseña es requerida";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.LoginMessage = "Las contraseñas no coinciden";
                return View(model);
            }

            if (model.Password.Length < 6)
            {
                ViewBag.LoginMessage = "La contraseña debe tener al menos 6 caracteres";
                return View(model);
            }

            var (success, errorMessage) = await _usuarioService.RestablecerClaveAsync(model.ResetCode, model.Password);

            if (!success)
            {
                ViewBag.LoginMessage = errorMessage;
                return View(model);
            }

            _logger.LogInformation("Contraseña restablecida exitosamente");

            return RedirectToAction("RestablecerClaveExitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer contraseña");
            ViewBag.LoginMessage = "Ocurrió un error al procesar la solicitud";
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult RestablecerClaveExitoso()
    {
        return View(new LoginView());
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
