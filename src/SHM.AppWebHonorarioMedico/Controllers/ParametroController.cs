using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Parametro;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class ParametroController : Controller
{
    private readonly ILogger<ParametroController> _logger;
    private readonly IParametroService _parametroService;

    public ParametroController(
        ILogger<ParametroController> logger,
        IParametroService parametroService)
    {
        _logger = logger;
        _parametroService = parametroService;
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
            var parametros = await _parametroService.GetAllParametrosAsync();

            var model = new ParametroListViewModel
            {
                Items = parametros.Select(p => new ParametroItemViewModel
                {
                    GuidRegistro = p.GuidRegistro ?? "",
                    Codigo = p.Codigo,
                    Valor = p.Valor,
                    Activo = p.Activo
                }).ToList()
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de parametros");
            return PartialView("_ListPartial", new ParametroListViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetCreateModal()
    {
        var model = new ParametroCreateViewModel();
        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] ParametroCreateViewModel model)
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

            // Verificar si ya existe un parametro con el mismo codigo
            var existente = await _parametroService.GetParametroByCodigoAsync(model.Codigo!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe un parametro con ese codigo" });
            }

            var createDto = new CreateParametroDto
            {
                Codigo = model.Codigo!,
                Valor = model.Valor
            };

            await _parametroService.CreateParametroAsync(createDto, idCreador);
            _logger.LogInformation("Parametro creado: {Codigo} por usuario {IdUsuario}",
                model.Codigo, idCreador);

            return Json(new { success = true, message = "Parametro creado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear parametro");
            return Json(new { success = false, message = "Error al crear el parametro" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var parametro = await _parametroService.GetParametroByGuidAsync(guid);
            if (parametro == null)
            {
                return NotFound();
            }

            var model = new ParametroEditViewModel
            {
                GuidRegistro = parametro.GuidRegistro ?? "",
                Codigo = parametro.Codigo,
                Valor = parametro.Valor,
                Activo = parametro.Activo
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener parametro para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] ParametroEditViewModel model)
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

            var parametro = await _parametroService.GetParametroByGuidAsync(model.GuidRegistro);
            if (parametro == null)
            {
                return Json(new { success = false, message = "Parametro no encontrado" });
            }

            // Verificar si ya existe otro parametro con el mismo codigo
            var existente = await _parametroService.GetParametroByCodigoAsync(model.Codigo!);
            if (existente != null && existente.IdParametro != parametro.IdParametro)
            {
                return Json(new { success = false, message = "Ya existe otro parametro con ese codigo" });
            }

            var updateDto = new UpdateParametroDto
            {
                Codigo = model.Codigo,
                Valor = model.Valor,
                Activo = model.Activo
            };

            var result = await _parametroService.UpdateParametroAsync(parametro.IdParametro, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar el parametro" });
            }

            _logger.LogInformation("Parametro actualizado: {Codigo} por usuario {IdUsuario}",
                model.Codigo, idModificador);

            return Json(new { success = true, message = "Parametro actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar parametro");
            return Json(new { success = false, message = "Error al actualizar el parametro" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var parametro = await _parametroService.GetParametroByGuidAsync(guid);
            if (parametro == null)
            {
                return NotFound();
            }

            var model = new ParametroDeleteViewModel
            {
                GuidRegistro = parametro.GuidRegistro ?? "",
                Codigo = parametro.Codigo,
                Valor = parametro.Valor
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener parametro para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] ParametroDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var parametro = await _parametroService.GetParametroByGuidAsync(model.GuidRegistro);
            if (parametro == null)
            {
                return Json(new { success = false, message = "Parametro no encontrado" });
            }

            var result = await _parametroService.DeleteParametroAsync(parametro.IdParametro, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el parametro" });
            }

            _logger.LogInformation("Parametro eliminado (logico): {Codigo} por usuario {IdUsuario}",
                parametro.Codigo, idModificador);

            return Json(new { success = true, message = "Parametro eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar parametro");
            return Json(new { success = false, message = "Error al eliminar el parametro" });
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
