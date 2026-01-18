using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.DTOs.Tabla;
using SHM.AppDomain.DTOs.TablaDetalle;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class TablaDetalleController : Controller
{
    private readonly ILogger<TablaDetalleController> _logger;
    private readonly ITablaDetalleService _tablaDetalleService;
    private readonly ITablaService _tablaService;

    public TablaDetalleController(
        ILogger<TablaDetalleController> logger,
        ITablaDetalleService tablaDetalleService,
        ITablaService tablaService)
    {
        _logger = logger;
        _tablaDetalleService = tablaDetalleService;
        _tablaService = tablaService;
    }

    public async Task<IActionResult> Index()
    {
        var tablas = await _tablaService.GetAllTablasAsync();
        var model = new TablaDetalleIndexViewModel
        {
            Tablas = tablas.Select(t => new SelectListItem
            {
                Value = t.IdTabla.ToString(),
                Text = $"{t.Codigo} - {t.Descripcion}"
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(int? idTabla)
    {
        try
        {
            var tablaDetalles = idTabla.HasValue
                ? await _tablaDetalleService.GetTablaDetallesByTablaIdAsync(idTabla.Value)
                : await _tablaDetalleService.GetAllTablaDetallesAsync();

            var tablas = await _tablaService.GetAllTablasAsync();
            var tablasDict = tablas.ToDictionary(t => t.IdTabla, t => $"{t.Codigo} - {t.Descripcion}");

            var model = new TablaDetalleListViewModel
            {
                Items = tablaDetalles.Select(td => new TablaDetalleItemViewModel
                {
                    GuidRegistro = td.GuidRegistro ?? "",
                    IdTabla = td.IdTabla,
                    NombreTabla = tablasDict.TryGetValue(td.IdTabla, out var nombre) ? nombre : "",
                    Codigo = td.Codigo,
                    Descripcion = td.Descripcion,
                    Orden = td.Orden,
                    Activo = td.Activo
                }).ToList()
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de tabla detalles");
            return PartialView("_ListPartial", new TablaDetalleListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateModal(int? idTabla)
    {
        var tablas = await _tablaService.GetAllTablasAsync();
        var model = new TablaDetalleCreateViewModel
        {
            IdTabla = idTabla ?? 0,
            Tablas = tablas.Select(t => new SelectListItem
            {
                Value = t.IdTabla.ToString(),
                Text = $"{t.Codigo} - {t.Descripcion}",
                Selected = idTabla.HasValue && t.IdTabla == idTabla.Value
            }).ToList()
        };
        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] TablaDetalleCreateViewModel model)
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

            // Verificar si ya existe un detalle con el mismo codigo en la misma tabla
            var existente = await _tablaDetalleService.GetTablaDetalleByCodigoAsync(model.IdTabla, model.Codigo!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe un detalle con ese codigo en la tabla seleccionada" });
            }

            var createDto = new CreateTablaDetalleDto
            {
                IdTabla = model.IdTabla,
                Codigo = model.Codigo!,
                Descripcion = model.Descripcion,
                Orden = model.Orden
            };

            await _tablaDetalleService.CreateTablaDetalleAsync(createDto, idCreador);
            _logger.LogInformation("TablaDetalle creado: {Codigo} en tabla {IdTabla} por usuario {IdUsuario}",
                model.Codigo, model.IdTabla, idCreador);

            return Json(new { success = true, message = "Detalle creado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tabla detalle");
            return Json(new { success = false, message = "Error al crear el detalle" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByGuidAsync(guid);
            if (tablaDetalle == null)
            {
                return NotFound();
            }

            var tablas = await _tablaService.GetAllTablasAsync();
            var model = new TablaDetalleEditViewModel
            {
                GuidRegistro = tablaDetalle.GuidRegistro ?? "",
                IdTabla = tablaDetalle.IdTabla,
                Codigo = tablaDetalle.Codigo,
                Descripcion = tablaDetalle.Descripcion,
                Orden = tablaDetalle.Orden,
                Activo = tablaDetalle.Activo,
                Tablas = tablas.Select(t => new SelectListItem
                {
                    Value = t.IdTabla.ToString(),
                    Text = $"{t.Codigo} - {t.Descripcion}",
                    Selected = t.IdTabla == tablaDetalle.IdTabla
                }).ToList()
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla detalle para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] TablaDetalleEditViewModel model)
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

            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByGuidAsync(model.GuidRegistro);
            if (tablaDetalle == null)
            {
                return Json(new { success = false, message = "Detalle no encontrado" });
            }

            // Verificar si ya existe otro detalle con el mismo codigo en la misma tabla
            var existente = await _tablaDetalleService.GetTablaDetalleByCodigoAsync(model.IdTabla, model.Codigo!);
            if (existente != null && existente.IdTablaDetalle != tablaDetalle.IdTablaDetalle)
            {
                return Json(new { success = false, message = "Ya existe otro detalle con ese codigo en la tabla seleccionada" });
            }

            var updateDto = new UpdateTablaDetalleDto
            {
                IdTabla = model.IdTabla,
                Codigo = model.Codigo,
                Descripcion = model.Descripcion,
                Orden = model.Orden,
                Activo = model.Activo
            };

            var result = await _tablaDetalleService.UpdateTablaDetalleAsync(tablaDetalle.IdTablaDetalle, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar el detalle" });
            }

            _logger.LogInformation("TablaDetalle actualizado: {Codigo} en tabla {IdTabla} por usuario {IdUsuario}",
                model.Codigo, model.IdTabla, idModificador);

            return Json(new { success = true, message = "Detalle actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tabla detalle");
            return Json(new { success = false, message = "Error al actualizar el detalle" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByGuidAsync(guid);
            if (tablaDetalle == null)
            {
                return NotFound();
            }

            var tabla = await _tablaService.GetTablaByIdAsync(tablaDetalle.IdTabla);

            var model = new TablaDetalleDeleteViewModel
            {
                GuidRegistro = tablaDetalle.GuidRegistro ?? "",
                NombreTabla = tabla != null ? $"{tabla.Codigo} - {tabla.Descripcion}" : "",
                Codigo = tablaDetalle.Codigo,
                Descripcion = tablaDetalle.Descripcion
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla detalle para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] TablaDetalleDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var tablaDetalle = await _tablaDetalleService.GetTablaDetalleByGuidAsync(model.GuidRegistro);
            if (tablaDetalle == null)
            {
                return Json(new { success = false, message = "Detalle no encontrado" });
            }

            var result = await _tablaDetalleService.DeleteTablaDetalleAsync(tablaDetalle.IdTablaDetalle);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el detalle" });
            }

            _logger.LogInformation("TablaDetalle eliminado (logico): {Codigo} por usuario {IdUsuario}",
                tablaDetalle.Codigo, idModificador);

            return Json(new { success = true, message = "Detalle eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tabla detalle");
            return Json(new { success = false, message = "Error al eliminar el detalle" });
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

    // ==================== CRUD para Tabla ====================

    [HttpGet]
    public async Task<IActionResult> GetTablaList()
    {
        try
        {
            var tablas = await _tablaService.GetAllTablasAsync();

            var model = new TablaListViewModel
            {
                Items = tablas.Select(t => new TablaItemViewModel
                {
                    GuidRegistro = t.GuidRegistro ?? "",
                    Codigo = t.Codigo,
                    Descripcion = t.Descripcion,
                    Activo = t.Activo
                }).ToList()
            };

            return PartialView("_TablaListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de tablas");
            return PartialView("_TablaListPartial", new TablaListViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetCreateTablaModal()
    {
        var model = new TablaCreateViewModel();
        return PartialView("_CreateTablaModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTabla([FromBody] TablaCreateViewModel model)
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

            // Verificar si ya existe una tabla con el mismo codigo
            var existente = await _tablaService.GetTablaByCodigoAsync(model.Codigo!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe una tabla con ese codigo" });
            }

            var createDto = new CreateTablaDto
            {
                Codigo = model.Codigo!,
                Descripcion = model.Descripcion
            };

            var nuevaTabla = await _tablaService.CreateTablaAsync(createDto, idCreador);
            _logger.LogInformation("Tabla creada: {Codigo} por usuario {IdUsuario}",
                model.Codigo, idCreador);

            return Json(new { success = true, message = "Tabla creada exitosamente", idTabla = nuevaTabla.IdTabla });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tabla");
            return Json(new { success = false, message = "Error al crear la tabla" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditTablaModal(string guid)
    {
        try
        {
            var tabla = await _tablaService.GetTablaByGuidAsync(guid);
            if (tabla == null)
            {
                return NotFound();
            }

            var model = new TablaEditViewModel
            {
                GuidRegistro = tabla.GuidRegistro ?? "",
                Codigo = tabla.Codigo,
                Descripcion = tabla.Descripcion,
                Activo = tabla.Activo
            };

            return PartialView("_EditTablaModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTabla([FromBody] TablaEditViewModel model)
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

            var tabla = await _tablaService.GetTablaByGuidAsync(model.GuidRegistro);
            if (tabla == null)
            {
                return Json(new { success = false, message = "Tabla no encontrada" });
            }

            // Verificar si ya existe otra tabla con el mismo codigo
            var existente = await _tablaService.GetTablaByCodigoAsync(model.Codigo!);
            if (existente != null && existente.IdTabla != tabla.IdTabla)
            {
                return Json(new { success = false, message = "Ya existe otra tabla con ese codigo" });
            }

            var updateDto = new UpdateTablaDto
            {
                Codigo = model.Codigo,
                Descripcion = model.Descripcion,
                Activo = model.Activo
            };

            var result = await _tablaService.UpdateTablaAsync(tabla.IdTabla, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar la tabla" });
            }

            _logger.LogInformation("Tabla actualizada: {Codigo} por usuario {IdUsuario}",
                model.Codigo, idModificador);

            return Json(new { success = true, message = "Tabla actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tabla");
            return Json(new { success = false, message = "Error al actualizar la tabla" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteTablaModal(string guid)
    {
        try
        {
            var tabla = await _tablaService.GetTablaByGuidAsync(guid);
            if (tabla == null)
            {
                return NotFound();
            }

            var model = new TablaDeleteViewModel
            {
                GuidRegistro = tabla.GuidRegistro ?? "",
                Codigo = tabla.Codigo,
                Descripcion = tabla.Descripcion
            };

            return PartialView("_DeleteTablaModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTabla([FromBody] TablaDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var tabla = await _tablaService.GetTablaByGuidAsync(model.GuidRegistro);
            if (tabla == null)
            {
                return Json(new { success = false, message = "Tabla no encontrada" });
            }

            var result = await _tablaService.DeleteTablaAsync(tabla.IdTabla);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar la tabla" });
            }

            _logger.LogInformation("Tabla eliminada (logico): {Codigo} por usuario {IdUsuario}",
                tabla.Codigo, idModificador);

            return Json(new { success = true, message = "Tabla eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tabla");
            return Json(new { success = false, message = "Error al eliminar la tabla" });
        }
    }
}
