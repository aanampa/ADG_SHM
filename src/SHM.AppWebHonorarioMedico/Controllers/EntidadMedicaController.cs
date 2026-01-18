using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.DTOs.EntidadCuentaBancaria;
using SHM.AppDomain.DTOs.EntidadMedica;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class EntidadMedicaController : Controller
{
    private readonly ILogger<EntidadMedicaController> _logger;
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly ITablaDetalleService _tablaDetalleService;
    private readonly IEntidadCuentaBancariaService _cuentaBancariaService;
    private readonly IBancoService _bancoService;

    public EntidadMedicaController(
        ILogger<EntidadMedicaController> logger,
        IEntidadMedicaService entidadMedicaService,
        ITablaDetalleService tablaDetalleService,
        IEntidadCuentaBancariaService cuentaBancariaService,
        IBancoService bancoService)
    {
        _logger = logger;
        _entidadMedicaService = entidadMedicaService;
        _tablaDetalleService = tablaDetalleService;
        _cuentaBancariaService = cuentaBancariaService;
        _bancoService = bancoService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _entidadMedicaService.GetPaginatedAsync(searchTerm, pageNumber, pageSize);
            var tiposEntidad = await _tablaDetalleService.ListarTipoEntidadMedicaAsync();
            var tiposDict = tiposEntidad.ToDictionary(t => t.Codigo ?? "", t => t.Descripcion ?? "");

            var model = new EntidadMedicaListViewModel
            {
                Items = items.Select(e => new EntidadMedicaItemViewModel
                {
                    GuidRegistro = e.GuidRegistro ?? "",
                    CodigoEntidad = e.CodigoEntidad,
                    RazonSocial = e.RazonSocial,
                    Ruc = e.Ruc,
                    TipoEntidadMedica = e.TipoEntidadMedica,
                    TipoEntidadMedicaDescripcion = tiposDict.TryGetValue(e.TipoEntidadMedica ?? "", out var desc) ? desc : e.TipoEntidadMedica,
                    Telefono = e.Telefono,
                    Celular = e.Celular,
                    CodigoAcreedor = e.CodigoAcreedor,
                    Activo = e.Activo,
                    FechaCreacion = e.FechaCreacion
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            _logger.LogInformation("Listando entidades medicas. Total: {Total}, Pagina: {Page}", totalCount, pageNumber);
            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar entidades medicas");
            return PartialView("_ListPartial", new EntidadMedicaListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateModal()
    {
        var tiposEntidad = await _tablaDetalleService.ListarTipoEntidadMedicaAsync();
        var model = new EntidadMedicaCreateViewModel
        {
            TiposEntidadMedica = tiposEntidad.Select(t => new SelectListItem
            {
                Value = t.Codigo,
                Text = t.Descripcion
            }).ToList()
        };

        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] EntidadMedicaCreateViewModel model)
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

            // Verificar si ya existe una entidad con el mismo RUC
            var existeRuc = await _entidadMedicaService.GetEntidadMedicaByRucAsync(model.Ruc!);
            if (existeRuc != null)
            {
                return Json(new { success = false, message = "Ya existe una entidad medica con el mismo RUC" });
            }

            // Verificar si ya existe una entidad con el mismo codigo
            var existeCodigo = await _entidadMedicaService.GetEntidadMedicaByCodigoAsync(model.CodigoEntidad!);
            if (existeCodigo != null)
            {
                return Json(new { success = false, message = "Ya existe una entidad medica con el mismo codigo" });
            }

            var createDto = new CreateEntidadMedicaDto
            {
                CodigoEntidad = model.CodigoEntidad ?? string.Empty,
                RazonSocial = model.RazonSocial,
                Ruc = model.Ruc,
                TipoEntidadMedica = model.TipoEntidadMedica,
                Telefono = model.Telefono,
                Celular = model.Celular,
                CodigoAcreedor = model.CodigoAcreedor
            };

            var result = await _entidadMedicaService.CreateEntidadMedicaAsync(createDto, idCreador);
            _logger.LogInformation("Entidad medica creada: {RazonSocial} por usuario {IdUsuario}", model.RazonSocial, idCreador);

            return Json(new { success = true, message = "Entidad medica creada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear entidad medica");
            return Json(new { success = false, message = "Error al crear la entidad medica" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(guid);
            if (entidad == null)
            {
                return NotFound();
            }

            var tiposEntidad = await _tablaDetalleService.ListarTipoEntidadMedicaAsync();
            var model = new EntidadMedicaEditViewModel
            {
                GuidRegistro = entidad.GuidRegistro ?? "",
                CodigoEntidad = entidad.CodigoEntidad,
                RazonSocial = entidad.RazonSocial,
                Ruc = entidad.Ruc,
                TipoEntidadMedica = entidad.TipoEntidadMedica,
                Telefono = entidad.Telefono,
                Celular = entidad.Celular,
                CodigoAcreedor = entidad.CodigoAcreedor,
                Activo = entidad.Activo,
                TiposEntidadMedica = tiposEntidad.Select(t => new SelectListItem
                {
                    Value = t.Codigo,
                    Text = t.Descripcion,
                    Selected = t.Codigo == entidad.TipoEntidadMedica
                }).ToList()
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entidad medica para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] EntidadMedicaEditViewModel model)
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

            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(model.GuidRegistro);
            if (entidad == null)
            {
                return Json(new { success = false, message = "Entidad medica no encontrada" });
            }

            // Verificar si el RUC ya existe en otra entidad
            var existeRuc = await _entidadMedicaService.GetEntidadMedicaByRucAsync(model.Ruc!);
            if (existeRuc != null && existeRuc.IdEntidadMedica != entidad.IdEntidadMedica)
            {
                return Json(new { success = false, message = "Ya existe otra entidad medica con el mismo RUC" });
            }

            // Verificar si el codigo ya existe en otra entidad
            var existeCodigo = await _entidadMedicaService.GetEntidadMedicaByCodigoAsync(model.CodigoEntidad!);
            if (existeCodigo != null && existeCodigo.IdEntidadMedica != entidad.IdEntidadMedica)
            {
                return Json(new { success = false, message = "Ya existe otra entidad medica con el mismo codigo" });
            }

            var updateDto = new UpdateEntidadMedicaDto
            {
                CodigoEntidad = model.CodigoEntidad,
                RazonSocial = model.RazonSocial,
                Ruc = model.Ruc,
                TipoEntidadMedica = model.TipoEntidadMedica,
                Telefono = model.Telefono,
                Celular = model.Celular,
                CodigoAcreedor = model.CodigoAcreedor,
                Activo = model.Activo
            };

            var result = await _entidadMedicaService.UpdateEntidadMedicaAsync(entidad.IdEntidadMedica, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar la entidad medica" });
            }

            _logger.LogInformation("Entidad medica actualizada: {RazonSocial} por usuario {IdUsuario}", model.RazonSocial, idModificador);
            return Json(new { success = true, message = "Entidad medica actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar entidad medica");
            return Json(new { success = false, message = "Error al actualizar la entidad medica" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(guid);
            if (entidad == null)
            {
                return NotFound();
            }

            var model = new EntidadMedicaDeleteViewModel
            {
                GuidRegistro = entidad.GuidRegistro ?? "",
                RazonSocial = entidad.RazonSocial,
                Ruc = entidad.Ruc,
                CodigoEntidad = entidad.CodigoEntidad
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entidad medica para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] EntidadMedicaDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(model.GuidRegistro);
            if (entidad == null)
            {
                return Json(new { success = false, message = "Entidad medica no encontrada" });
            }

            var result = await _entidadMedicaService.DeleteEntidadMedicaAsync(entidad.IdEntidadMedica, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar la entidad medica" });
            }

            _logger.LogInformation("Entidad medica eliminada (logica): {RazonSocial} por usuario {IdUsuario}", entidad.RazonSocial, idModificador);
            return Json(new { success = true, message = "Entidad medica eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar entidad medica");
            return Json(new { success = false, message = "Error al eliminar la entidad medica" });
        }
    }

    #region Cuentas Bancarias

    [HttpGet]
    public async Task<IActionResult> GetCuentasBancariasModal(string guid)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(guid);
            if (entidad == null)
            {
                return NotFound();
            }

            var cuentas = await _cuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(entidad.IdEntidadMedica);
            var bancos = await _bancoService.GetAllBancosAsync();
            var bancosDict = bancos.ToDictionary(b => b.IdBanco, b => b.NombreBanco ?? "");

            var model = new CuentaBancariaListViewModel
            {
                EntidadGuid = guid,
                EntidadRazonSocial = entidad.RazonSocial ?? "",
                IdEntidadMedica = entidad.IdEntidadMedica,
                Items = cuentas.Where(c => c.Activo == 1).Select(c => new CuentaBancariaItemViewModel
                {
                    GuidRegistro = c.GuidRegistro,
                    BancoNombre = c.IdBanco.HasValue && bancosDict.TryGetValue(c.IdBanco.Value, out var banco) ? banco : "",
                    CuentaCorriente = c.CuentaCorriente,
                    CuentaCci = c.CuentaCci,
                    Moneda = c.Moneda,
                    MonedaDescripcion = c.Moneda == "PEN" ? "Soles" : c.Moneda == "USD" ? "Dolares" : c.Moneda,
                    Activo = c.Activo
                }).ToList()
            };

            return PartialView("_CuentasBancariasModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas bancarias para entidad: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetListCuentasBancarias(string entidadGuid)
    {
        try
        {
            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(entidadGuid);
            if (entidad == null)
            {
                return NotFound();
            }

            var cuentas = await _cuentaBancariaService.GetEntidadCuentasBancariasByEntidadIdAsync(entidad.IdEntidadMedica);
            var bancos = await _bancoService.GetAllBancosAsync();
            var bancosDict = bancos.ToDictionary(b => b.IdBanco, b => b.NombreBanco ?? "");

            var model = new CuentaBancariaListViewModel
            {
                EntidadGuid = entidadGuid,
                EntidadRazonSocial = entidad.RazonSocial ?? "",
                IdEntidadMedica = entidad.IdEntidadMedica,
                Items = cuentas.Where(c => c.Activo == 1).Select(c => new CuentaBancariaItemViewModel
                {
                    GuidRegistro = c.GuidRegistro,
                    BancoNombre = c.IdBanco.HasValue && bancosDict.TryGetValue(c.IdBanco.Value, out var banco) ? banco : "",
                    CuentaCorriente = c.CuentaCorriente,
                    CuentaCci = c.CuentaCci,
                    Moneda = c.Moneda,
                    MonedaDescripcion = c.Moneda == "PEN" ? "Soles" : c.Moneda == "USD" ? "Dolares" : c.Moneda,
                    Activo = c.Activo
                }).ToList()
            };

            return PartialView("_CuentasBancariasListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar cuentas bancarias");
            return PartialView("_CuentasBancariasListPartial", new CuentaBancariaListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateCuentaBancariaModal(string entidadGuid)
    {
        var bancos = await _bancoService.GetAllBancosAsync();
        var model = new CuentaBancariaCreateViewModel
        {
            EntidadGuid = entidadGuid,
            Bancos = bancos.Where(b => b.Activo == 1).Select(b => new SelectListItem
            {
                Value = b.IdBanco.ToString(),
                Text = b.NombreBanco
            }).ToList(),
            Monedas = new List<SelectListItem>
            {
                new SelectListItem { Value = "PEN", Text = "Soles (PEN)" },
                new SelectListItem { Value = "USD", Text = "Dolares (USD)" }
            }
        };

        return PartialView("_CreateCuentaBancariaModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCuentaBancaria([FromBody] CuentaBancariaCreateViewModel model)
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

            var entidad = await _entidadMedicaService.GetEntidadMedicaByGuidAsync(model.EntidadGuid);
            if (entidad == null)
            {
                return Json(new { success = false, message = "Entidad medica no encontrada" });
            }

            var createDto = new CreateEntidadCuentaBancariaDto
            {
                IdEntidad = entidad.IdEntidadMedica,
                IdBanco = model.IdBanco,
                CuentaCorriente = model.CuentaCorriente,
                CuentaCci = model.CuentaCci,
                Moneda = model.Moneda
            };

            await _cuentaBancariaService.CreateEntidadCuentaBancariaAsync(createDto, idCreador);
            _logger.LogInformation("Cuenta bancaria creada para entidad {Entidad} por usuario {IdUsuario}", entidad.RazonSocial, idCreador);

            return Json(new { success = true, message = "Cuenta bancaria creada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cuenta bancaria");
            return Json(new { success = false, message = "Error al crear la cuenta bancaria" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditCuentaBancariaModal(string guid, string entidadGuid)
    {
        try
        {
            var cuenta = await _cuentaBancariaService.GetEntidadCuentaBancariaByGuidAsync(guid);
            if (cuenta == null)
            {
                return NotFound();
            }

            var bancos = await _bancoService.GetAllBancosAsync();
            var model = new CuentaBancariaEditViewModel
            {
                GuidRegistro = cuenta.GuidRegistro,
                EntidadGuid = entidadGuid,
                IdBanco = cuenta.IdBanco,
                CuentaCorriente = cuenta.CuentaCorriente,
                CuentaCci = cuenta.CuentaCci,
                Moneda = cuenta.Moneda,
                Activo = cuenta.Activo,
                Bancos = bancos.Where(b => b.Activo == 1).Select(b => new SelectListItem
                {
                    Value = b.IdBanco.ToString(),
                    Text = b.NombreBanco,
                    Selected = b.IdBanco == cuenta.IdBanco
                }).ToList(),
                Monedas = new List<SelectListItem>
                {
                    new SelectListItem { Value = "PEN", Text = "Soles (PEN)", Selected = cuenta.Moneda == "PEN" },
                    new SelectListItem { Value = "USD", Text = "Dolares (USD)", Selected = cuenta.Moneda == "USD" }
                }
            };

            return PartialView("_EditCuentaBancariaModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuenta bancaria para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCuentaBancaria([FromBody] CuentaBancariaEditViewModel model)
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

            var cuenta = await _cuentaBancariaService.GetEntidadCuentaBancariaByGuidAsync(model.GuidRegistro);
            if (cuenta == null)
            {
                return Json(new { success = false, message = "Cuenta bancaria no encontrada" });
            }

            var updateDto = new UpdateEntidadCuentaBancariaDto
            {
                IdBanco = model.IdBanco,
                CuentaCorriente = model.CuentaCorriente,
                CuentaCci = model.CuentaCci,
                Moneda = model.Moneda,
                Activo = model.Activo
            };

            var result = await _cuentaBancariaService.UpdateEntidadCuentaBancariaAsync(cuenta.IdCuentaBancaria, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar la cuenta bancaria" });
            }

            _logger.LogInformation("Cuenta bancaria actualizada: {CuentaCorriente} por usuario {IdUsuario}", model.CuentaCorriente, idModificador);
            return Json(new { success = true, message = "Cuenta bancaria actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cuenta bancaria");
            return Json(new { success = false, message = "Error al actualizar la cuenta bancaria" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteCuentaBancariaModal(string guid, string entidadGuid)
    {
        try
        {
            var cuenta = await _cuentaBancariaService.GetEntidadCuentaBancariaByGuidAsync(guid);
            if (cuenta == null)
            {
                return NotFound();
            }

            var bancos = await _bancoService.GetAllBancosAsync();
            var bancoNombre = cuenta.IdBanco.HasValue
                ? bancos.FirstOrDefault(b => b.IdBanco == cuenta.IdBanco.Value)?.NombreBanco
                : null;

            var model = new CuentaBancariaDeleteViewModel
            {
                GuidRegistro = cuenta.GuidRegistro,
                EntidadGuid = entidadGuid,
                BancoNombre = bancoNombre,
                CuentaCorriente = cuenta.CuentaCorriente,
                CuentaCci = cuenta.CuentaCci,
                MonedaDescripcion = cuenta.Moneda == "PEN" ? "Soles" : cuenta.Moneda == "USD" ? "Dolares" : cuenta.Moneda
            };

            return PartialView("_DeleteCuentaBancariaModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuenta bancaria para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCuentaBancaria([FromBody] CuentaBancariaDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var cuenta = await _cuentaBancariaService.GetEntidadCuentaBancariaByGuidAsync(model.GuidRegistro);
            if (cuenta == null)
            {
                return Json(new { success = false, message = "Cuenta bancaria no encontrada" });
            }

            var result = await _cuentaBancariaService.DeleteEntidadCuentaBancariaAsync(cuenta.IdCuentaBancaria, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar la cuenta bancaria" });
            }

            _logger.LogInformation("Cuenta bancaria eliminada (logica): {CuentaCorriente} por usuario {IdUsuario}", cuenta.CuentaCorriente, idModificador);
            return Json(new { success = true, message = "Cuenta bancaria eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cuenta bancaria");
            return Json(new { success = false, message = "Error al eliminar la cuenta bancaria" });
        }
    }

    #endregion

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
