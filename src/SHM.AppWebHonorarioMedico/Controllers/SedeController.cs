using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Sede;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class SedeController : Controller
{
    private readonly ILogger<SedeController> _logger;
    private readonly ISedeService _sedeService;

    public SedeController(
        ILogger<SedeController> logger,
        ISedeService sedeService)
    {
        _logger = logger;
        _sedeService = sedeService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        try
        {
            var sedes = await _sedeService.GetAllSedesAsync();

            var model = new SedeListViewModel
            {
                Items = sedes.Select(s => new SedeItemViewModel
                {
                    GuidRegistro = s.GuidRegistro ?? "",
                    Codigo = s.Codigo,
                    Nombre = s.Nombre,
                    Ruc = s.Ruc,
                    Direccion = s.Direccion,
                    Activo = s.Activo
                }).ToList()
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de sedes");
            return PartialView("_ListPartial", new SedeListViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetCreateModal()
    {
        var model = new SedeCreateViewModel();
        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] SedeCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var idCreador = GetCurrentUserId();
            if (idCreador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Verificar si ya existe una sede con el mismo codigo
            var existente = await _sedeService.GetSedeByCodigoAsync(model.Codigo!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe una sede con ese codigo" });
            }

            var createDto = new CreateSedeDto
            {
                Codigo = model.Codigo!,
                Nombre = model.Nombre!,
                Ruc = model.Ruc,
                Direccion = model.Direccion
            };

            await _sedeService.CreateSedeAsync(createDto, idCreador);
            _logger.LogInformation("Sede creada: {Codigo} - {Nombre} por usuario {IdUsuario}",
                model.Codigo, model.Nombre, idCreador);

            return Json(new { success = true, message = "Sede creada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sede");
            return Json(new { success = false, message = "Error al crear la sede" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var sede = await _sedeService.GetSedeByGuidAsync(guid);
            if (sede == null)
            {
                return NotFound();
            }

            var model = new SedeEditViewModel
            {
                GuidRegistro = sede.GuidRegistro ?? "",
                Codigo = sede.Codigo,
                Nombre = sede.Nombre,
                Ruc = sede.Ruc,
                Direccion = sede.Direccion,
                Activo = sede.Activo
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sede para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] SedeEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var sede = await _sedeService.GetSedeByGuidAsync(model.GuidRegistro);
            if (sede == null)
            {
                return Json(new { success = false, message = "Sede no encontrada" });
            }

            // Verificar si ya existe otra sede con el mismo codigo
            var existente = await _sedeService.GetSedeByCodigoAsync(model.Codigo!);
            if (existente != null && existente.IdSede != sede.IdSede)
            {
                return Json(new { success = false, message = "Ya existe otra sede con ese codigo" });
            }

            var updateDto = new UpdateSedeDto
            {
                Codigo = model.Codigo,
                Nombre = model.Nombre,
                Ruc = model.Ruc,
                Direccion = model.Direccion,
                Activo = model.Activo
            };

            var result = await _sedeService.UpdateSedeAsync(sede.IdSede, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar la sede" });
            }

            _logger.LogInformation("Sede actualizada: {Codigo} - {Nombre} por usuario {IdUsuario}",
                model.Codigo, model.Nombre, idModificador);

            return Json(new { success = true, message = "Sede actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sede");
            return Json(new { success = false, message = "Error al actualizar la sede" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var sede = await _sedeService.GetSedeByGuidAsync(guid);
            if (sede == null)
            {
                return NotFound();
            }

            var model = new SedeDeleteViewModel
            {
                GuidRegistro = sede.GuidRegistro ?? "",
                Codigo = sede.Codigo,
                Nombre = sede.Nombre,
                Ruc = sede.Ruc,
                Direccion = sede.Direccion
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sede para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] SedeDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var sede = await _sedeService.GetSedeByGuidAsync(model.GuidRegistro);
            if (sede == null)
            {
                return Json(new { success = false, message = "Sede no encontrada" });
            }

            var result = await _sedeService.DeleteSedeAsync(sede.IdSede, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar la sede" });
            }

            _logger.LogInformation("Sede eliminada (logico): {Codigo} - {Nombre} por usuario {IdUsuario}",
                sede.Codigo, sede.Nombre, idModificador);

            return Json(new { success = true, message = "Sede eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar sede");
            return Json(new { success = false, message = "Error al eliminar la sede" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("IdUsuario");
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int idUsuario))
        {
            return idUsuario;
        }
        return 0;
    }
}
