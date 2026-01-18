using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;
using System.Text.Json;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class MenuController : Controller
{
    private readonly ILogger<MenuController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUsuarioService _usuarioService;

    public MenuController(
        ILogger<MenuController> logger,
        IConfiguration configuration,
        IUsuarioService usuarioService)
    {
        _logger = logger;
        _configuration = configuration;
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> GetMenu()
    {
        string instancia = _configuration["AppSettings:InstanceName"] ?? "";

        if (!IsUserAuthenticated())
        {
            await LogoutAndRedirect();
            return RedirectToLogin();
        }

        try
        {
            var usuarioActual = User.Identity?.Name;
            if (string.IsNullOrEmpty(usuarioActual))
            {
                await LogoutAndRedirect();
                return RedirectToLogin();
            }

            // Modo PROTOTIPO - menú estático
            if (instancia == "PROTOTIPO")
            {
                return PartialView("_MenuEstatico");
            }

            // Obtener menú dinámico según rol del usuario
            var menuItems = await GetOrLoadMenuItems(usuarioActual);

            var model = new MenuViewModel
            {
                ListaOpciones = menuItems
            };

            return PartialView("_Menu", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el menú para usuario: {Usuario}", User.Identity?.Name);
            await LogoutAndRedirect();
            return RedirectToLogin();
        }
    }

    private bool IsUserAuthenticated() =>
        User?.Identity != null && User.Identity.IsAuthenticated;

    private async Task LogoutAndRedirect() =>
        await HttpContext.SignOutAsync();

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Auth");

    private async Task<List<OpcionMenuDTO>> GetOrLoadMenuItems(string usuarioActual)
    {
        const string menuCacheKey = "ShmMenuKey";

        // Intentar obtener del Session
        var cachedMenu = HttpContext.Session.GetString(menuCacheKey);
        if (!string.IsNullOrEmpty(cachedMenu))
        {
            var menuCachedItems = JsonSerializer.Deserialize<List<OpcionMenuDTO>>(cachedMenu);
            if (menuCachedItems != null && menuCachedItems.Count > 0)
            {
                return menuCachedItems;
            }
        }

        // Cargar desde BD
        var menuItems = (await _usuarioService.ListarMenuPorLoginAsync(usuarioActual)).ToList();

        // Guardar en Session
        if (menuItems.Count > 0)
        {
            HttpContext.Session.SetString(menuCacheKey, JsonSerializer.Serialize(menuItems));
        }

        return menuItems;
    }
}
