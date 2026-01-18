using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Banco;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class BancoController : Controller
{
    private readonly ILogger<BancoController> _logger;
    private readonly IBancoService _bancoService;

    public BancoController(
        ILogger<BancoController> logger,
        IBancoService bancoService)
    {
        _logger = logger;
        _bancoService = bancoService;
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
            var bancos = await _bancoService.GetAllBancosAsync();

            var model = new BancoListViewModel
            {
                Items = bancos.Select(b => new BancoItemViewModel
                {
                    GuidRegistro = b.GuidRegistro ?? "",
                    CodigoBanco = b.CodigoBanco,
                    NombreBanco = b.NombreBanco,
                    Activo = b.Activo
                }).ToList()
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de bancos");
            return PartialView("_ListPartial", new BancoListViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetCreateModal()
    {
        var model = new BancoCreateViewModel();
        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] BancoCreateViewModel model)
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

            // Verificar si ya existe un banco con el mismo codigo
            var existente = await _bancoService.GetBancoByCodigoAsync(model.CodigoBanco!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe un banco con ese codigo" });
            }

            var createDto = new CreateBancoDto
            {
                CodigoBanco = model.CodigoBanco,
                NombreBanco = model.NombreBanco
            };

            await _bancoService.CreateBancoAsync(createDto, idCreador);
            _logger.LogInformation("Banco creado: {CodigoBanco} - {NombreBanco} por usuario {IdUsuario}",
                model.CodigoBanco, model.NombreBanco, idCreador);

            return Json(new { success = true, message = "Banco creado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear banco");
            return Json(new { success = false, message = "Error al crear el banco" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var banco = await _bancoService.GetBancoByGuidAsync(guid);
            if (banco == null)
            {
                return NotFound();
            }

            var model = new BancoEditViewModel
            {
                GuidRegistro = banco.GuidRegistro ?? "",
                CodigoBanco = banco.CodigoBanco,
                NombreBanco = banco.NombreBanco,
                Activo = banco.Activo
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener banco para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] BancoEditViewModel model)
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

            var banco = await _bancoService.GetBancoByGuidAsync(model.GuidRegistro);
            if (banco == null)
            {
                return Json(new { success = false, message = "Banco no encontrado" });
            }

            // Verificar si ya existe otro banco con el mismo codigo
            var existente = await _bancoService.GetBancoByCodigoAsync(model.CodigoBanco!);
            if (existente != null && existente.IdBanco != banco.IdBanco)
            {
                return Json(new { success = false, message = "Ya existe otro banco con ese codigo" });
            }

            var updateDto = new UpdateBancoDto
            {
                CodigoBanco = model.CodigoBanco,
                NombreBanco = model.NombreBanco,
                Activo = model.Activo
            };

            var result = await _bancoService.UpdateBancoAsync(banco.IdBanco, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar el banco" });
            }

            _logger.LogInformation("Banco actualizado: {CodigoBanco} - {NombreBanco} por usuario {IdUsuario}",
                model.CodigoBanco, model.NombreBanco, idModificador);

            return Json(new { success = true, message = "Banco actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar banco");
            return Json(new { success = false, message = "Error al actualizar el banco" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var banco = await _bancoService.GetBancoByGuidAsync(guid);
            if (banco == null)
            {
                return NotFound();
            }

            var model = new BancoDeleteViewModel
            {
                GuidRegistro = banco.GuidRegistro ?? "",
                CodigoBanco = banco.CodigoBanco,
                NombreBanco = banco.NombreBanco
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener banco para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] BancoDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var banco = await _bancoService.GetBancoByGuidAsync(model.GuidRegistro);
            if (banco == null)
            {
                return Json(new { success = false, message = "Banco no encontrado" });
            }

            var result = await _bancoService.DeleteBancoAsync(banco.IdBanco, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el banco" });
            }

            _logger.LogInformation("Banco eliminado (logico): {CodigoBanco} - {NombreBanco} por usuario {IdUsuario}",
                banco.CodigoBanco, banco.NombreBanco, idModificador);

            return Json(new { success = true, message = "Banco eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar banco");
            return Json(new { success = false, message = "Error al eliminar el banco" });
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
