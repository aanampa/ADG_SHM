using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.DTOs.Usuario;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class UsuarioController : Controller
{
    private readonly ILogger<UsuarioController> _logger;
    private readonly IUsuarioService _usuarioService;
    private readonly IRolService _rolService;
    private readonly IEntidadMedicaService _entidadMedicaService;

    public UsuarioController(
        ILogger<UsuarioController> logger,
        IUsuarioService usuarioService,
        IRolService rolService,
        IEntidadMedicaService entidadMedicaService)
    {
        _logger = logger;
        _usuarioService = usuarioService;
        _rolService = rolService;
        _entidadMedicaService = entidadMedicaService;
    }

    public IActionResult Externos()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _usuarioService.GetPaginatedExternosAsync(searchTerm, pageNumber, pageSize);

            // Obtener roles y entidades para mostrar nombres
            var roles = await _rolService.GetAllRolesAsync();
            var rolesDict = roles.ToDictionary(r => r.IdRol, r => r.Descripcion ?? "");

            var model = new UsuarioExternoListViewModel
            {
                Items = new List<UsuarioExternoItemViewModel>(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            foreach (var u in items)
            {
                string? entidadNombre = null;
                if (u.IdEntidadMedica.HasValue)
                {
                    var entidad = await _entidadMedicaService.GetEntidadMedicaByIdAsync(u.IdEntidadMedica.Value);
                    entidadNombre = entidad?.RazonSocial;
                }

                model.Items.Add(new UsuarioExternoItemViewModel
                {
                    GuidRegistro = u.GuidRegistro ?? "",
                    Login = u.Login,
                    NombreCompleto = $"{u.Nombres} {u.ApellidoPaterno} {u.ApellidoMaterno}".Trim(),
                    Email = u.Email,
                    NumeroDocumento = u.NumeroDocumento,
                    Celular = u.Celular,
                    EntidadMedicaNombre = entidadNombre,
                    RolDescripcion = u.IdRol.HasValue && rolesDict.TryGetValue(u.IdRol.Value, out var rol) ? rol : "",
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion
                });
            }

            _logger.LogInformation("Listando usuarios externos. Total: {Total}, Pagina: {Page}", totalCount, pageNumber);
            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar usuarios externos");
            return PartialView("_ListPartial", new UsuarioExternoListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateModal()
    {
        var roles = await _rolService.GetAllRolesAsync();
        var model = new UsuarioExternoCreateViewModel
        {
            Roles = roles.Where(r => r.Activo == 1).Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Descripcion
            }).ToList()
        };

        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] UsuarioExternoCreateViewModel model)
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

            var createDto = new CreateUsuarioDto
            {
                TipoUsuario = "E",
                Login = model.Login ?? "",
                Password = "", // Se generara automaticamente
                Email = model.Email,
                Nombres = model.Nombres,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                NumeroDocumento = model.NumeroDocumento,
                Celular = model.Celular,
                IdEntidadMedica = model.IdEntidadMedica,
                IdRol = model.IdRol
            };

            var (success, errorMessage, generatedPassword) = await _usuarioService.CreateUsuarioExternoAsync(createDto, idCreador, model.EnviarCorreo);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage });
            }

            _logger.LogInformation("Usuario externo creado: {Login} por usuario {IdUsuario}", model.Login, idCreador);

            var responseMessage = "Usuario creado exitosamente.";
            if (!model.EnviarCorreo && !string.IsNullOrEmpty(generatedPassword))
            {
                responseMessage += $" Clave generada: {generatedPassword}";
            }
            else if (model.EnviarCorreo)
            {
                responseMessage += " Las credenciales han sido enviadas al correo del usuario.";
            }

            return Json(new { success = true, message = responseMessage, generatedPassword = model.EnviarCorreo ? null : generatedPassword });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario externo");
            return Json(new { success = false, message = "Error al crear el usuario" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var usuario = await _usuarioService.GetUsuarioByGuidAsync(guid);
            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _rolService.GetAllRolesAsync();
            string? entidadNombre = null;
            if (usuario.IdEntidadMedica.HasValue)
            {
                var entidad = await _entidadMedicaService.GetEntidadMedicaByIdAsync(usuario.IdEntidadMedica.Value);
                entidadNombre = entidad?.RazonSocial;
            }

            var model = new UsuarioExternoEditViewModel
            {
                GuidRegistro = usuario.GuidRegistro ?? "",
                Login = usuario.Login,
                Email = usuario.Email,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                NumeroDocumento = usuario.NumeroDocumento,
                Celular = usuario.Celular,
                IdEntidadMedica = usuario.IdEntidadMedica,
                EntidadMedicaNombre = entidadNombre,
                IdRol = usuario.IdRol,
                Activo = usuario.Activo,
                Roles = roles.Where(r => r.Activo == 1).Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Descripcion,
                    Selected = r.IdRol == usuario.IdRol
                }).ToList()
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] UsuarioExternoEditViewModel model)
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

            var usuario = await _usuarioService.GetUsuarioByGuidAsync(model.GuidRegistro);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var updateDto = new UpdateUsuarioDto
            {
                Login = model.Login,
                Email = model.Email,
                Nombres = model.Nombres,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                NumeroDocumento = model.NumeroDocumento,
                Celular = model.Celular,
                IdEntidadMedica = model.IdEntidadMedica,
                IdRol = model.IdRol,
                Activo = model.Activo
            };

            var result = await _usuarioService.UpdateUsuarioAsync(usuario.IdUsuario, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar el usuario" });
            }

            _logger.LogInformation("Usuario externo actualizado: {Login} por usuario {IdUsuario}", model.Login, idModificador);
            return Json(new { success = true, message = "Usuario actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario externo");
            return Json(new { success = false, message = "Error al actualizar el usuario" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var usuario = await _usuarioService.GetUsuarioByGuidAsync(guid);
            if (usuario == null)
            {
                return NotFound();
            }

            var model = new UsuarioExternoDeleteViewModel
            {
                GuidRegistro = usuario.GuidRegistro ?? "",
                NombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}".Trim(),
                Login = usuario.Login,
                Email = usuario.Email
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] UsuarioExternoDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var usuario = await _usuarioService.GetUsuarioByGuidAsync(model.GuidRegistro);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var result = await _usuarioService.DeleteUsuarioAsync(usuario.IdUsuario, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el usuario" });
            }

            _logger.LogInformation("Usuario externo eliminado (logico): {Login} por usuario {IdUsuario}", usuario.Login, idModificador);
            return Json(new { success = true, message = "Usuario eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario externo");
            return Json(new { success = false, message = "Error al eliminar el usuario" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetResetClaveModal(string guid)
    {
        try
        {
            var usuario = await _usuarioService.GetUsuarioByGuidAsync(guid);
            if (usuario == null)
            {
                return NotFound();
            }

            var model = new UsuarioExternoResetClaveViewModel
            {
                GuidRegistro = usuario.GuidRegistro ?? "",
                NombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}".Trim(),
                Login = usuario.Login,
                Email = usuario.Email
            };

            return PartialView("_ResetClaveModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario para reset de clave: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetClave([FromBody] UsuarioExternoResetClaveViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var usuario = await _usuarioService.GetUsuarioByGuidAsync(model.GuidRegistro);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var (success, errorMessage, generatedPassword) = await _usuarioService.ResetearClaveUsuarioAsync(usuario.IdUsuario, idModificador, model.EnviarCorreo);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage });
            }

            _logger.LogInformation("Clave reseteada para usuario: {Login} por usuario {IdUsuario}", usuario.Login, idModificador);

            var responseMessage = "Clave reseteada exitosamente.";
            if (!model.EnviarCorreo && !string.IsNullOrEmpty(generatedPassword))
            {
                responseMessage += $" Nueva clave: {generatedPassword}";
            }
            else if (model.EnviarCorreo)
            {
                responseMessage += " La nueva clave ha sido enviada al correo del usuario.";
            }

            return Json(new { success = true, message = responseMessage, generatedPassword = model.EnviarCorreo ? null : generatedPassword });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resetear clave de usuario");
            return Json(new { success = false, message = "Error al resetear la clave" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> BuscarEntidadesMedicas(string term)
    {
        try
        {
            var (items, _) = await _entidadMedicaService.GetPaginatedAsync(term, 1, 20);
            var result = items.Select(e => new EntidadMedicaSelectItem
            {
                Id = e.IdEntidadMedica,
                Text = e.RazonSocial ?? ""
            }).ToList();

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar entidades medicas");
            return Json(new List<EntidadMedicaSelectItem>());
        }
    }

    #region Usuarios Internos

    public IActionResult Internos()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetListInternos(string? searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _usuarioService.GetPaginatedInternosAsync(searchTerm, pageNumber, pageSize);

            // Obtener roles para mostrar nombres
            var roles = await _rolService.GetAllRolesAsync();
            var rolesDict = roles.ToDictionary(r => r.IdRol, r => r.Descripcion ?? "");

            var model = new UsuarioInternoListViewModel
            {
                Items = new List<UsuarioInternoItemViewModel>(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            foreach (var u in items)
            {
                model.Items.Add(new UsuarioInternoItemViewModel
                {
                    GuidRegistro = u.GuidRegistro ?? "",
                    Login = u.Login,
                    NombreCompleto = $"{u.Nombres} {u.ApellidoPaterno} {u.ApellidoMaterno}".Trim(),
                    Email = u.Email,
                    NumeroDocumento = u.NumeroDocumento,
                    Celular = u.Celular,
                    RolDescripcion = u.IdRol.HasValue && rolesDict.TryGetValue(u.IdRol.Value, out var rol) ? rol : "",
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion
                });
            }

            _logger.LogInformation("Listando usuarios internos. Total: {Total}, Pagina: {Page}", totalCount, pageNumber);
            return PartialView("_ListInternosPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar usuarios internos");
            return PartialView("_ListInternosPartial", new UsuarioInternoListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateInternoModal()
    {
        var roles = await _rolService.GetAllRolesAsync();
        var model = new UsuarioInternoCreateViewModel
        {
            Roles = roles.Where(r => r.Activo == 1).Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Descripcion
            }).ToList()
        };

        return PartialView("_CreateInternoModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInterno([FromBody] UsuarioInternoCreateViewModel model)
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

            var createDto = new CreateUsuarioDto
            {
                TipoUsuario = "I",
                Login = model.Login ?? "",
                Password = "", // Se generara automaticamente
                Email = model.Email,
                Nombres = model.Nombres,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                NumeroDocumento = model.NumeroDocumento,
                Celular = model.Celular,
                IdEntidadMedica = null, // Usuario interno no tiene entidad medica
                IdRol = model.IdRol
            };

            var (success, errorMessage, generatedPassword) = await _usuarioService.CreateUsuarioInternoAsync(createDto, idCreador, model.EnviarCorreo);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage });
            }

            _logger.LogInformation("Usuario interno creado: {Login} por usuario {IdUsuario}", model.Login, idCreador);

            var responseMessage = "Usuario creado exitosamente.";
            if (!model.EnviarCorreo && !string.IsNullOrEmpty(generatedPassword))
            {
                responseMessage += $" Clave generada: {generatedPassword}";
            }
            else if (model.EnviarCorreo)
            {
                responseMessage += " Las credenciales han sido enviadas al correo del usuario.";
            }

            return Json(new { success = true, message = responseMessage, generatedPassword = model.EnviarCorreo ? null : generatedPassword });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario interno");
            return Json(new { success = false, message = "Error al crear el usuario" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditInternoModal(string guid)
    {
        try
        {
            var usuario = await _usuarioService.GetUsuarioByGuidAsync(guid);
            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _rolService.GetAllRolesAsync();

            var model = new UsuarioInternoEditViewModel
            {
                GuidRegistro = usuario.GuidRegistro ?? "",
                Login = usuario.Login,
                Email = usuario.Email,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                NumeroDocumento = usuario.NumeroDocumento,
                Celular = usuario.Celular,
                IdRol = usuario.IdRol,
                Activo = usuario.Activo,
                Roles = roles.Where(r => r.Activo == 1).Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Descripcion,
                    Selected = r.IdRol == usuario.IdRol
                }).ToList()
            };

            return PartialView("_EditInternoModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario interno para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInterno([FromBody] UsuarioInternoEditViewModel model)
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

            var usuario = await _usuarioService.GetUsuarioByGuidAsync(model.GuidRegistro);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var updateDto = new UpdateUsuarioDto
            {
                Login = model.Login,
                Email = model.Email,
                Nombres = model.Nombres,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                NumeroDocumento = model.NumeroDocumento,
                Celular = model.Celular,
                IdEntidadMedica = null, // Usuario interno no tiene entidad medica
                IdRol = model.IdRol,
                Activo = model.Activo
            };

            var result = await _usuarioService.UpdateUsuarioAsync(usuario.IdUsuario, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar el usuario" });
            }

            _logger.LogInformation("Usuario interno actualizado: {Login} por usuario {IdUsuario}", model.Login, idModificador);
            return Json(new { success = true, message = "Usuario actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario interno");
            return Json(new { success = false, message = "Error al actualizar el usuario" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInternoModal(string guid)
    {
        try
        {
            var usuario = await _usuarioService.GetUsuarioByGuidAsync(guid);
            if (usuario == null)
            {
                return NotFound();
            }

            var model = new UsuarioInternoDeleteViewModel
            {
                GuidRegistro = usuario.GuidRegistro ?? "",
                NombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}".Trim(),
                Login = usuario.Login,
                Email = usuario.Email
            };

            return PartialView("_DeleteInternoModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario interno para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteInterno([FromBody] UsuarioInternoDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var usuario = await _usuarioService.GetUsuarioByGuidAsync(model.GuidRegistro);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var result = await _usuarioService.DeleteUsuarioAsync(usuario.IdUsuario, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el usuario" });
            }

            _logger.LogInformation("Usuario interno eliminado (logico): {Login} por usuario {IdUsuario}", usuario.Login, idModificador);
            return Json(new { success = true, message = "Usuario eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario interno");
            return Json(new { success = false, message = "Error al eliminar el usuario" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetResetClaveInternoModal(string guid)
    {
        try
        {
            var usuario = await _usuarioService.GetUsuarioByGuidAsync(guid);
            if (usuario == null)
            {
                return NotFound();
            }

            var model = new UsuarioInternoResetClaveViewModel
            {
                GuidRegistro = usuario.GuidRegistro ?? "",
                NombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}".Trim(),
                Login = usuario.Login,
                Email = usuario.Email
            };

            return PartialView("_ResetClaveInternoModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario interno para reset de clave: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetClaveInterno([FromBody] UsuarioInternoResetClaveViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var usuario = await _usuarioService.GetUsuarioByGuidAsync(model.GuidRegistro);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var (success, errorMessage, generatedPassword) = await _usuarioService.ResetearClaveUsuarioAsync(usuario.IdUsuario, idModificador, model.EnviarCorreo);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage });
            }

            _logger.LogInformation("Clave reseteada para usuario interno: {Login} por usuario {IdUsuario}", usuario.Login, idModificador);

            var responseMessage = "Clave reseteada exitosamente.";
            if (!model.EnviarCorreo && !string.IsNullOrEmpty(generatedPassword))
            {
                responseMessage += $" Nueva clave: {generatedPassword}";
            }
            else if (model.EnviarCorreo)
            {
                responseMessage += " La nueva clave ha sido enviada al correo del usuario.";
            }

            return Json(new { success = true, message = responseMessage, generatedPassword = model.EnviarCorreo ? null : generatedPassword });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resetear clave de usuario interno");
            return Json(new { success = false, message = "Error al resetear la clave" });
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
