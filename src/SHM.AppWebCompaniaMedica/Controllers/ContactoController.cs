using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.EntidadContacto;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebCompaniaMedica.Models;

namespace SHM.AppWebCompaniaMedica.Controllers;

/// <summary>
/// Controlador para la gestion de contactos de notificacion de la compania medica.
/// Cada usuario solo gestiona los contactos de su propia entidad medica.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-17</created>
/// </summary>
public class ContactoController : BaseController
{
    private readonly IEntidadContactoService _contactoService;
    private readonly ILogger<ContactoController> _logger;

    public ContactoController(
        IEntidadContactoService contactoService,
        ILogger<ContactoController> logger)
    {
        _contactoService = contactoService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Mis Contactos";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        try
        {
            var idEntidadMedica = GetIdEntidadMedica();
            if (idEntidadMedica == 0) return PartialView("_ListPartial", new List<ContactoItemViewModel>());

            var contactos = await _contactoService.GetByEntidadMedicaAsync(idEntidadMedica, soloActivos: false);

            var items = contactos.Select(c => new ContactoItemViewModel
            {
                GuidRegistro    = c.GuidRegistro,
                ApellidoPaterno = c.ApellidoPaterno,
                ApellidoMaterno = c.ApellidoMaterno,
                Nombres         = c.Nombres,
                Email           = c.Email,
                Celular         = c.Celular,
                Cargo           = c.Cargo,
                Activo          = c.Activo
            }).ToList();

            return PartialView("_ListPartial", items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar contactos");
            return PartialView("_ListPartial", new List<ContactoItemViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] ContactoCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var idEntidadMedica = GetIdEntidadMedica();
            if (idEntidadMedica == 0)
                return Json(new { success = false, message = "Sesion no valida" });

            var existeEmail = await _contactoService.ExisteEmailEnEntidadAsync(idEntidadMedica, model.Email!);
            if (existeEmail)
                return Json(new { success = false, message = "Ya existe un contacto con ese correo" });

            var idCreador = GetIdUsuario();

            await _contactoService.CreateAsync(new CreateEntidadContactoDto
            {
                IdEntidadMedica = idEntidadMedica,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                Nombres         = model.Nombres,
                Email           = model.Email ?? "",
                Celular         = model.Celular,
                Cargo           = model.Cargo
            }, idCreador);

            _logger.LogInformation("Contacto creado por usuario {Id} en entidad {Entidad}", idCreador, idEntidadMedica);
            return Json(new { success = true, message = "Contacto creado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear contacto");
            return Json(new { success = false, message = "Error al crear el contacto" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var contacto = await _contactoService.GetByGuidAsync(guid);
            if (contacto == null || contacto.IdEntidadMedica != GetIdEntidadMedica())
                return NotFound();

            var model = new ContactoEditViewModel
            {
                GuidRegistro      = contacto.GuidRegistro,
                IdEntidadContacto = contacto.IdEntidadContacto,
                ApellidoPaterno   = contacto.ApellidoPaterno,
                ApellidoMaterno   = contacto.ApellidoMaterno,
                Nombres           = contacto.Nombres,
                Email             = contacto.Email,
                Celular           = contacto.Celular,
                Cargo             = contacto.Cargo,
                Activo            = contacto.Activo
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar modal de edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] ContactoEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var contacto = await _contactoService.GetByGuidAsync(model.GuidRegistro);
            if (contacto == null || contacto.IdEntidadMedica != GetIdEntidadMedica())
                return Json(new { success = false, message = "Contacto no encontrado" });

            var existeEmail = await _contactoService.ExisteEmailEnEntidadAsync(
                GetIdEntidadMedica(), model.Email!, model.IdEntidadContacto);
            if (existeEmail)
                return Json(new { success = false, message = "Ya existe otro contacto con ese correo" });

            var resultado = await _contactoService.UpdateAsync(model.GuidRegistro, new UpdateEntidadContactoDto
            {
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                Nombres         = model.Nombres,
                Email           = model.Email,
                Celular         = model.Celular,
                Cargo           = model.Cargo,
                Activo          = model.Activo
            }, GetIdUsuario());

            if (!resultado)
                return Json(new { success = false, message = "No se pudo actualizar el contacto" });

            _logger.LogInformation("Contacto actualizado: {Guid}", model.GuidRegistro);
            return Json(new { success = true, message = "Contacto actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contacto");
            return Json(new { success = false, message = "Error al actualizar el contacto" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActivo([FromBody] ContactoDeleteViewModel model)
    {
        try
        {
            var contacto = await _contactoService.GetByGuidAsync(model.GuidRegistro);
            if (contacto == null || contacto.IdEntidadMedica != GetIdEntidadMedica())
                return Json(new { success = false, message = "Contacto no encontrado" });

            var resultado = await _contactoService.ToggleActivoAsync(model.GuidRegistro, GetIdUsuario());
            if (!resultado)
                return Json(new { success = false, message = "No se pudo cambiar el estado del contacto" });

            var nuevoEstado = contacto.Activo == 1 ? "inactivado" : "activado";
            _logger.LogInformation("Contacto {Estado}: {Guid}", nuevoEstado, model.GuidRegistro);
            return Json(new { success = true, message = $"Contacto {nuevoEstado} exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del contacto");
            return Json(new { success = false, message = "Error al cambiar el estado del contacto" });
        }
    }

    private int GetIdEntidadMedica()
    {
        var claim = User.FindFirstValue("IdEntidadMedica");
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private int GetIdUsuario()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
