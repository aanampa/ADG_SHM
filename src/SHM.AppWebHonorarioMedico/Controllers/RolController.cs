using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Rol;
using SHM.AppDomain.DTOs.RolOpcion;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class RolController : Controller
{
    private readonly IRolService _rolService;
    private readonly IOpcionService _opcionService;
    private readonly IRolOpcionService _rolOpcionService;
    private readonly ILogger<RolController> _logger;

    public RolController(
        IRolService rolService,
        IOpcionService opcionService,
        IRolOpcionService rolOpcionService,
        ILogger<RolController> logger)
    {
        _rolService = rolService;
        _opcionService = opcionService;
        _rolOpcionService = rolOpcionService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new RolIndexViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        try
        {
            var roles = await _rolService.GetAllRolesAsync();

            var model = new RolListViewModel
            {
                Items = roles.Select(r => new RolItemViewModel
                {
                    GuidRegistro = r.GuidRegistro ?? "",
                    Codigo = r.Codigo,
                    Descripcion = r.Descripcion,
                    Activo = r.Activo
                }).ToList()
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de roles");
            return PartialView("_ListPartial", new RolListViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetCreateModal()
    {
        var model = new RolCreateViewModel();
        return PartialView("_CreateModal", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] RolCreateViewModel model)
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

            // Verificar si ya existe un rol con el mismo codigo
            var existente = await _rolService.GetRolByCodigoAsync(model.Codigo!);
            if (existente != null)
            {
                return Json(new { success = false, message = "Ya existe un rol con ese codigo" });
            }

            var createDto = new CreateRolDto
            {
                Codigo = model.Codigo!,
                Descripcion = model.Descripcion!
            };

            await _rolService.CreateRolAsync(createDto, idCreador);
            _logger.LogInformation("Rol creado: {Codigo} por usuario {IdUsuario}",
                model.Codigo, idCreador);

            return Json(new { success = true, message = "Rol creado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear rol");
            return Json(new { success = false, message = "Error al crear el rol" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var rol = await _rolService.GetRolByGuidAsync(guid);
            if (rol == null)
            {
                return NotFound();
            }

            var model = new RolEditViewModel
            {
                GuidRegistro = rol.GuidRegistro ?? "",
                Codigo = rol.Codigo,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] RolEditViewModel model)
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

            var rol = await _rolService.GetRolByGuidAsync(model.GuidRegistro);
            if (rol == null)
            {
                return Json(new { success = false, message = "Rol no encontrado" });
            }

            // Verificar si ya existe otro rol con el mismo codigo
            var existente = await _rolService.GetRolByCodigoAsync(model.Codigo!);
            if (existente != null && existente.IdRol != rol.IdRol)
            {
                return Json(new { success = false, message = "Ya existe otro rol con ese codigo" });
            }

            var updateDto = new UpdateRolDto
            {
                Codigo = model.Codigo,
                Descripcion = model.Descripcion,
                Activo = model.Activo
            };

            var result = await _rolService.UpdateRolAsync(rol.IdRol, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar el rol" });
            }

            _logger.LogInformation("Rol actualizado: {Codigo} por usuario {IdUsuario}",
                model.Codigo, idModificador);

            return Json(new { success = true, message = "Rol actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol");
            return Json(new { success = false, message = "Error al actualizar el rol" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var rol = await _rolService.GetRolByGuidAsync(guid);
            if (rol == null)
            {
                return NotFound();
            }

            var model = new RolDeleteViewModel
            {
                GuidRegistro = rol.GuidRegistro ?? "",
                Codigo = rol.Codigo,
                Descripcion = rol.Descripcion
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] RolDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var rol = await _rolService.GetRolByGuidAsync(model.GuidRegistro);
            if (rol == null)
            {
                return Json(new { success = false, message = "Rol no encontrado" });
            }

            var result = await _rolService.DeleteRolAsync(rol.IdRol, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar el rol" });
            }

            _logger.LogInformation("Rol eliminado (logico): {Codigo} por usuario {IdUsuario}",
                rol.Codigo, idModificador);

            return Json(new { success = true, message = "Rol eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol");
            return Json(new { success = false, message = "Error al eliminar el rol" });
        }
    }

    // ==================== Asignacion de Opciones ====================

    [HttpGet]
    public async Task<IActionResult> GetAssignOpcionesModal(string guid)
    {
        try
        {
            var rol = await _rolService.GetRolByGuidAsync(guid);
            if (rol == null)
            {
                return NotFound();
            }

            // Obtener todas las opciones activas
            var todasOpciones = await _opcionService.GetAllOpcionesAsync();
            var opcionesActivas = todasOpciones.Where(o => o.Activo == 1).ToList();

            // Obtener las opciones asignadas al rol
            var opcionesAsignadas = await _rolOpcionService.GetOpcionesByRolAsync(rol.IdRol);
            var idsAsignados = opcionesAsignadas
                .Where(ro => ro.Activo == 1)
                .Select(ro => ro.IdOpcion)
                .ToHashSet();

            // Construir el arbol de opciones
            var opcionesRaiz = opcionesActivas
                .Where(o => o.IdOpcionPadre == null || o.IdOpcionPadre == 0)
                .OrderBy(o => o.Orden)
                .Select(o => BuildOpcionTree(o, opcionesActivas, idsAsignados))
                .ToList();

            var model = new RolOpcionesAssignViewModel
            {
                GuidRegistro = rol.GuidRegistro ?? "",
                Codigo = rol.Codigo,
                Descripcion = rol.Descripcion,
                IdRol = rol.IdRol,
                Opciones = opcionesRaiz
            };

            return PartialView("_AssignOpcionesModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opciones para asignar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    private OpcionMenuItemViewModel BuildOpcionTree(
        SHM.AppDomain.DTOs.Opcion.OpcionResponseDto opcion,
        IEnumerable<SHM.AppDomain.DTOs.Opcion.OpcionResponseDto> todasOpciones,
        HashSet<int> idsAsignados)
    {
        var hijos = todasOpciones
            .Where(o => o.IdOpcionPadre == opcion.IdOpcion)
            .OrderBy(o => o.Orden)
            .Select(o => BuildOpcionTree(o, todasOpciones, idsAsignados))
            .ToList();

        return new OpcionMenuItemViewModel
        {
            IdOpcion = opcion.IdOpcion,
            Nombre = opcion.Nombre,
            Icono = opcion.Icono,
            IdOpcionPadre = opcion.IdOpcionPadre,
            Orden = opcion.Orden,
            Asignado = idsAsignados.Contains(opcion.IdOpcion),
            Hijos = hijos
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOpciones([FromBody] SaveRolOpcionesRequest model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var rol = await _rolService.GetRolByGuidAsync(model.GuidRegistro);
            if (rol == null)
            {
                return Json(new { success = false, message = "Rol no encontrado" });
            }

            // Obtener las opciones actualmente asignadas
            var opcionesActuales = await _rolOpcionService.GetOpcionesByRolAsync(rol.IdRol);
            var idsActuales = opcionesActuales
                .Where(ro => ro.Activo == 1)
                .Select(ro => ro.IdOpcion)
                .ToHashSet();

            var idsNuevos = model.OpcionesSeleccionadas.ToHashSet();

            // Opciones a agregar (estan en nuevos pero no en actuales)
            var opcionesAgregar = idsNuevos.Except(idsActuales);

            // Opciones a quitar (estan en actuales pero no en nuevos)
            var opcionesQuitar = idsActuales.Except(idsNuevos);

            // Agregar nuevas asignaciones
            foreach (var idOpcion in opcionesAgregar)
            {
                // Verificar si existe una asignacion inactiva
                var existente = await _rolOpcionService.GetRolOpcionByIdAsync(rol.IdRol, idOpcion);
                if (existente != null)
                {
                    // Reactivar la asignacion existente
                    var updateDto = new UpdateRolOpcionDto { Activo = 1 };
                    await _rolOpcionService.UpdateRolOpcionAsync(rol.IdRol, idOpcion, updateDto, idModificador);
                }
                else
                {
                    // Crear nueva asignacion
                    var createDto = new CreateRolOpcionDto
                    {
                        IdRol = rol.IdRol,
                        IdOpcion = idOpcion
                    };
                    await _rolOpcionService.CreateRolOpcionAsync(createDto, idModificador);
                }
            }

            // Desactivar asignaciones removidas
            foreach (var idOpcion in opcionesQuitar)
            {
                await _rolOpcionService.DeleteRolOpcionAsync(rol.IdRol, idOpcion, idModificador);
            }

            _logger.LogInformation("Opciones actualizadas para rol {Codigo} por usuario {IdUsuario}. Agregadas: {Agregadas}, Quitadas: {Quitadas}",
                rol.Codigo, idModificador, opcionesAgregar.Count(), opcionesQuitar.Count());

            return Json(new { success = true, message = "Opciones asignadas exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar opciones del rol");
            return Json(new { success = false, message = "Error al guardar las opciones" });
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
