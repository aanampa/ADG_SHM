using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppWebCompaniaMedica.Controllers;

public class AuthController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUsuarioService usuarioService,
        IEntidadMedicaService entidadMedicaService,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _usuarioService = usuarioService;
        _entidadMedicaService = entidadMedicaService;
        _emailService = emailService;
        _logger = logger;
    }

    // GET: Auth/Login
    public IActionResult Login()
    {
        // Si ya está autenticado, redirigir al Dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        return View();
    }

    // POST: Auth/Login
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string captchaAnswer, string captchaExpected)
    {
        try
        {
            _logger.LogInformation("Intento de login para usuario: {Username}", username);

            // Validar campos requeridos
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Usuario y contraseña son requeridos";
                return View();
            }

            // Validar CAPTCHA
            if (string.IsNullOrEmpty(captchaAnswer) ||
                !captchaAnswer.Equals(captchaExpected, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "El código de verificación es incorrecto";
                return View();
            }

            // Validar credenciales contra la base de datos
            var usuario = await _usuarioService.ValidarCredencialesAsync(username, password);

            if (usuario == null)
            {
                _logger.LogWarning("Intento de login fallido para usuario: {Username}", username);
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // Obtener razon social de la entidad medica
            string razonSocial = "";
            if (usuario.IdEntidadMedica.HasValue && usuario.IdEntidadMedica.Value > 0)
            {
                var entidadMedica = await _entidadMedicaService.GetEntidadMedicaByIdAsync(usuario.IdEntidadMedica.Value);
                razonSocial = entidadMedica?.RazonSocial ?? "";
            }

            // Crear Claims para el usuario autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Login),
                new Claim("Nombres", usuario.Nombres ?? ""),
                new Claim("ApellidoPaterno", usuario.ApellidoPaterno ?? ""),
                new Claim("ApellidoMaterno", usuario.ApellidoMaterno ?? ""),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim("TipoUsuario", usuario.TipoUsuario),
                new Claim("RazonSocial", razonSocial)
            };

            if (usuario.IdEntidadMedica.HasValue)
                claims.Add(new Claim("IdEntidadMedica", usuario.IdEntidadMedica.Value.ToString()));
            else
                claims.Add(new Claim("IdEntidadMedica", "0"));

            if (usuario.IdRol.HasValue)
                claims.Add(new Claim(ClaimTypes.Role, usuario.IdRol.Value.ToString()));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Login exitoso para usuario: {Username} (ID: {UserId})", username, usuario.IdUsuario);

            return RedirectToAction("Dashboard", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login para usuario: {Username}", username);
            ViewBag.Error = "Ocurrió un error al procesar la solicitud. Por favor intente nuevamente.";
            return View();
        }
    }

    // GET: Auth/Logout
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userLogin = User.FindFirstValue(ClaimTypes.Name);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("Logout exitoso para usuario: {UserLogin} (ID: {UserId})", userLogin, userId);

        return RedirectToAction("Login");
    }

    // GET: Auth/AccesoDenegado
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    // GET: Auth/RecuperarClave
    public IActionResult RecuperarClave()
    {
        return View();
    }

    // POST: Auth/RecuperarClave
    [HttpPost]
    public async Task<IActionResult> RecuperarClave(string emailOrUsername)
    {
        if (string.IsNullOrEmpty(emailOrUsername))
        {
            ViewBag.Error = "Por favor ingresa tu correo electrónico o usuario";
            return View();
        }

        try
        {
            _logger.LogInformation("Solicitud de recuperación de contraseña para: {EmailOrUsername}", emailOrUsername);

            // Generar token de recuperación
            var (success, token, email, nombres) = await _usuarioService.GenerarTokenRecuperacionAsync(emailOrUsername);

            if (success && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(email))
            {
                // Obtener URL base
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                // Enviar email
                var emailSent = await _emailService.EnviarEmailRecuperacionAsync(email, nombres ?? "Usuario", token, baseUrl);

                if (!emailSent)
                {
                    _logger.LogWarning("No se pudo enviar el email de recuperación a: {Email}", email);
                }
            }

            // Por seguridad, siempre mostramos el mismo mensaje
            ViewBag.Success = "Si el usuario existe y tiene un correo registrado, se han enviado las instrucciones de recuperación.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar recuperación de contraseña para: {EmailOrUsername}", emailOrUsername);
            ViewBag.Error = "Ocurrió un error al procesar la solicitud. Por favor intente nuevamente.";
            return View();
        }
    }

    // GET: Auth/RestablecerClave
    public async Task<IActionResult> RestablecerClave(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Enlace de recuperación inválido";
            return View("RecuperarClave");
        }

        // Validar token
        var (isValid, errorMessage) = await _usuarioService.ValidarTokenRecuperacionAsync(token);

        if (!isValid)
        {
            ViewBag.Error = errorMessage;
            return View("RecuperarClave");
        }

        ViewBag.Token = token;
        return View();
    }

    // POST: Auth/RestablecerClave
    [HttpPost]
    public async Task<IActionResult> RestablecerClave(string token, string password, string confirmPassword)
    {
        ViewBag.Token = token;

        // Validaciones
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Token de recuperación inválido";
            return View();
        }

        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ViewBag.Error = "La contraseña es requerida";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Las contraseñas no coinciden";
            return View();
        }

        if (password.Length < 6)
        {
            ViewBag.Error = "La contraseña debe tener al menos 6 caracteres";
            return View();
        }

        try
        {
            var (success, errorMessage) = await _usuarioService.RestablecerClaveAsync(token, password);

            if (!success)
            {
                ViewBag.Error = errorMessage;
                return View();
            }

            _logger.LogInformation("Contraseña restablecida exitosamente para token: {Token}", token[..8] + "...");

            ViewBag.Success = "Tu contraseña ha sido restablecida exitosamente. Ya puedes iniciar sesión con tu nueva contraseña.";
            return View("RestablecerClaveExitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer contraseña");
            ViewBag.Error = "Ocurrió un error al procesar la solicitud. Por favor intente nuevamente.";
            return View();
        }
    }
}
