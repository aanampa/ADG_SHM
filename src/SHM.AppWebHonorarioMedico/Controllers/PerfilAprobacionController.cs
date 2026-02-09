using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.PerfilAprobacion;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para el mantenimiento de perfiles de aprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
[Authorize]
public class PerfilAprobacionController : Controller
{
    private readonly ILogger<PerfilAprobacionController> _logger;
    private readonly IPerfilAprobacionService _perfilAprobacionService;

    public PerfilAprobacionController(
        ILogger<PerfilAprobacionController> logger,
        IPerfilAprobacionService perfilAprobacionService)
    {
        _logger = logger;
        _perfilAprobacionService = perfilAprobacionService;
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
            var items = await _perfilAprobacionService.GetAllAsync();

            var model = new PerfilAprobacionListViewModel
            {
                Items = items.Select(p => new PerfilAprobacionItemViewModel
                {
                    GuidRegistro = p.GuidRegistro ?? "",
                    GrupoFlujoTrabajo = p.GrupoFlujoTrabajo,
                    Codigo = p.Codigo,
                    Descripcion = p.Descripcion,
                    Nivel = p.Nivel,
                    Orden = p.Orden,
                    Activo = p.Activo
                }).ToList()
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de perfiles de aprobacion");
            return PartialView("_ListPartial", new PerfilAprobacionListViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetCreateModal()
    {
        var model = new PerfilAprobacionCreateViewModel();
        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] PerfilAprobacionCreateViewModel model)
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

            // Verificar si ya existe un perfil con el mismo codigo
            var existente = await _perfilAprobacionService.GetByCodigoAsync(model.Codigo!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe un perfil de aprobacion con ese codigo" });
            }

            var createDto = new CreatePerfilAprobacionDto
            {
                GrupoFlujoTrabajo = model.GrupoFlujoTrabajo,
                Codigo = model.Codigo,
                Descripcion = model.Descripcion,
                Nivel = model.Nivel,
                Orden = model.Orden
            };

            await _perfilAprobacionService.CreateAsync(createDto, idCreador);
            _logger.LogInformation("Perfil de aprobacion creado: {Codigo} - {Descripcion} por usuario {IdUsuario}",
                model.Codigo, model.Descripcion, idCreador);

            return Json(new { success = true, message = "Perfil de aprobacion creado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear perfil de aprobacion");
            return Json(new { success = false, message = "Error al crear el perfil de aprobacion" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var perfil = await _perfilAprobacionService.GetByGuidAsync(guid);
            if (perfil == null)
            {
                return NotFound();
            }

            var model = new PerfilAprobacionEditViewModel
            {
                GuidRegistro = perfil.GuidRegistro ?? "",
                GrupoFlujoTrabajo = perfil.GrupoFlujoTrabajo,
                Codigo = perfil.Codigo,
                Descripcion = perfil.Descripcion,
                Nivel = perfil.Nivel,
                Orden = perfil.Orden,
                Activo = perfil.Activo
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil de aprobacion para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] PerfilAprobacionEditViewModel model)
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

            var perfil = await _perfilAprobacionService.GetByGuidAsync(model.GuidRegistro);
            if (perfil == null)
            {
                return Json(new { success = false, message = "Perfil de aprobacion no encontrado" });
            }

            // Verificar si ya existe otro perfil con el mismo codigo
            var existente = await _perfilAprobacionService.GetByCodigoAsync(model.Codigo!);
            if (existente != null && existente.IdPerfilAprobacion != perfil.IdPerfilAprobacion)
            {
                return Json(new { success = false, message = "Ya existe otro perfil de aprobacion con ese codigo" });
            }

            var updateDto = new UpdatePerfilAprobacionDto
            {
                IdPerfilAprobacion = perfil.IdPerfilAprobacion,
                GrupoFlujoTrabajo = model.GrupoFlujoTrabajo,
                Codigo = model.Codigo,
                Descripcion = model.Descripcion,
                Nivel = model.Nivel,
                Orden = model.Orden
            };

            var result = await _perfilAprobacionService.UpdateAsync(updateDto, idModificador);
            if (result == null)
            {
                return Json(new { success = false, message = "No se pudo actualizar el perfil de aprobacion" });
            }

            _logger.LogInformation("Perfil de aprobacion actualizado: {Codigo} - {Descripcion} por usuario {IdUsuario}",
                model.Codigo, model.Descripcion, idModificador);

            return Json(new { success = true, message = "Perfil de aprobacion actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil de aprobacion");
            return Json(new { success = false, message = "Error al actualizar el perfil de aprobacion" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var perfil = await _perfilAprobacionService.GetByGuidAsync(guid);
            if (perfil == null)
            {
                return NotFound();
            }

            var model = new PerfilAprobacionDeleteViewModel
            {
                GuidRegistro = perfil.GuidRegistro ?? "",
                GrupoFlujoTrabajo = perfil.GrupoFlujoTrabajo,
                Codigo = perfil.Codigo,
                Descripcion = perfil.Descripcion,
                Nivel = perfil.Nivel,
                Orden = perfil.Orden
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil de aprobacion para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] PerfilAprobacionDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var perfil = await _perfilAprobacionService.GetByGuidAsync(model.GuidRegistro);
            if (perfil == null)
            {
                return Json(new { success = false, message = "Perfil de aprobacion no encontrado" });
            }

            var result = await _perfilAprobacionService.DeleteAsync(model.GuidRegistro, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el perfil de aprobacion" });
            }

            _logger.LogInformation("Perfil de aprobacion eliminado (logico): {Codigo} - {Descripcion} por usuario {IdUsuario}",
                perfil.Codigo, perfil.Descripcion, idModificador);

            return Json(new { success = true, message = "Perfil de aprobacion eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar perfil de aprobacion");
            return Json(new { success = false, message = "Error al eliminar el perfil de aprobacion" });
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
