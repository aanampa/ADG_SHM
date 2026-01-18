using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.Interfaces.Services;
using SHM.AppWebHonorarioMedico.Models;

namespace SHM.AppWebHonorarioMedico.Controllers;

[Authorize]
public class OpcionController : Controller
{
    private readonly IOpcionService _opcionService;
    private readonly ILogger<OpcionController> _logger;

    public OpcionController(
        IOpcionService opcionService,
        ILogger<OpcionController> logger)
    {
        _opcionService = opcionService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new OpcionIndexViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        try
        {
            var opciones = await _opcionService.GetAllOpcionesAsync();
            var opcionesList = opciones.ToList();

            // Construir el arbol de opciones
            var opcionesRaiz = opcionesList
                .Where(o => o.IdOpcionPadre == null || o.IdOpcionPadre == 0)
                .OrderBy(o => o.Orden)
                .ThenBy(o => o.IdOpcion)
                .Select(o => BuildOpcionTree(o, opcionesList))
                .ToList();

            var model = new OpcionListViewModel
            {
                Items = opcionesRaiz
            };

            return PartialView("_ListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de opciones");
            return PartialView("_ListPartial", new OpcionListViewModel());
        }
    }

    private OpcionTreeItemViewModel BuildOpcionTree(OpcionResponseDto opcion, List<OpcionResponseDto> todasOpciones)
    {
        var hijos = todasOpciones
            .Where(o => o.IdOpcionPadre == opcion.IdOpcion)
            .OrderBy(o => o.Orden)
            .ThenBy(o => o.IdOpcion)
            .Select(o => BuildOpcionTree(o, todasOpciones))
            .ToList();

        var padre = opcion.IdOpcionPadre.HasValue && opcion.IdOpcionPadre.Value > 0
            ? todasOpciones.FirstOrDefault(o => o.IdOpcion == opcion.IdOpcionPadre.Value)
            : null;

        return new OpcionTreeItemViewModel
        {
            IdOpcion = opcion.IdOpcion,
            GuidRegistro = opcion.GuidRegistro ?? "",
            Nombre = opcion.Nombre,
            Url = opcion.Url,
            Icono = opcion.Icono,
            Orden = opcion.Orden,
            IdOpcionPadre = opcion.IdOpcionPadre,
            NombrePadre = padre?.Nombre,
            Activo = opcion.Activo,
            Hijos = hijos
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateModal(int? idPadre)
    {
        var opciones = await _opcionService.GetAllOpcionesAsync();
        var opcionesActivas = opciones.Where(o => o.Activo == 1).ToList();

        var model = new OpcionCreateViewModel
        {
            IdOpcionPadre = idPadre,
            OpcionesPadre = BuildOpcionesPadreSelectList(opcionesActivas, null)
        };

        return PartialView("_CreateModal", model);
    }

    private List<SelectListItem> BuildOpcionesPadreSelectList(
        List<OpcionResponseDto> opciones,
        int? excludeId,
        int? selectedId = null)
    {
        var items = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Ninguno (Opcion Raiz) --" }
        };

        // Solo permitir como padres las opciones raiz (nivel 1) o que no tengan URL (son contenedores)
        var opcionesRaiz = opciones
            .Where(o => (o.IdOpcionPadre == null || o.IdOpcionPadre == 0) && o.IdOpcion != excludeId)
            .OrderBy(o => o.Orden)
            .ThenBy(o => o.Nombre);

        foreach (var opcion in opcionesRaiz)
        {
            items.Add(new SelectListItem
            {
                Value = opcion.IdOpcion.ToString(),
                Text = opcion.Nombre,
                Selected = selectedId.HasValue && selectedId.Value == opcion.IdOpcion
            });
        }

        return items;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] OpcionCreateViewModel model)
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

            var createDto = new CreateOpcionDto
            {
                Nombre = model.Nombre!,
                Url = model.Url,
                Icono = model.Icono,
                Orden = model.Orden,
                IdOpcionPadre = model.IdOpcionPadre
            };

            await _opcionService.CreateOpcionAsync(createDto, idCreador);
            _logger.LogInformation("Opcion creada: {Nombre} por usuario {IdUsuario}",
                model.Nombre, idCreador);

            return Json(new { success = true, message = "Opcion creada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear opcion");
            return Json(new { success = false, message = $"Error al crear la opcion: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEditModal(string guid)
    {
        try
        {
            var opcion = await _opcionService.GetOpcionByGuidAsync(guid);
            if (opcion == null)
            {
                return NotFound();
            }

            var opciones = await _opcionService.GetAllOpcionesAsync();
            var opcionesActivas = opciones.Where(o => o.Activo == 1).ToList();

            var model = new OpcionEditViewModel
            {
                GuidRegistro = opcion.GuidRegistro ?? "",
                Nombre = opcion.Nombre,
                Url = opcion.Url,
                Icono = opcion.Icono,
                Orden = opcion.Orden,
                IdOpcionPadre = opcion.IdOpcionPadre,
                Activo = opcion.Activo,
                OpcionesPadre = BuildOpcionesPadreSelectList(opcionesActivas, opcion.IdOpcion, opcion.IdOpcionPadre)
            };

            return PartialView("_EditModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opcion para edicion: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] OpcionEditViewModel model)
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

            var opcion = await _opcionService.GetOpcionByGuidAsync(model.GuidRegistro);
            if (opcion == null)
            {
                return Json(new { success = false, message = "Opcion no encontrada" });
            }

            var updateDto = new UpdateOpcionDto
            {
                Nombre = model.Nombre,
                Url = model.Url,
                Icono = model.Icono,
                Orden = model.Orden,
                IdOpcionPadre = model.IdOpcionPadre,
                Activo = model.Activo
            };

            var result = await _opcionService.UpdateOpcionAsync(opcion.IdOpcion, updateDto, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo actualizar la opcion" });
            }

            _logger.LogInformation("Opcion actualizada: {Nombre} por usuario {IdUsuario}",
                model.Nombre, idModificador);

            return Json(new { success = true, message = "Opcion actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar opcion");
            return Json(new { success = false, message = "Error al actualizar la opcion" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteModal(string guid)
    {
        try
        {
            var opcion = await _opcionService.GetOpcionByGuidAsync(guid);
            if (opcion == null)
            {
                return NotFound();
            }

            var opciones = await _opcionService.GetAllOpcionesAsync();
            var cantidadHijos = opciones.Count(o => o.IdOpcionPadre == opcion.IdOpcion && o.Activo == 1);

            string? nombrePadre = null;
            if (opcion.IdOpcionPadre.HasValue && opcion.IdOpcionPadre.Value > 0)
            {
                var padre = opciones.FirstOrDefault(o => o.IdOpcion == opcion.IdOpcionPadre.Value);
                nombrePadre = padre?.Nombre;
            }

            var model = new OpcionDeleteViewModel
            {
                GuidRegistro = opcion.GuidRegistro ?? "",
                Nombre = opcion.Nombre,
                Url = opcion.Url,
                NombrePadre = nombrePadre,
                CantidadHijos = cantidadHijos
            };

            return PartialView("_DeleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener opcion para eliminar: {Guid}", guid);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] OpcionDeleteViewModel model)
    {
        try
        {
            var idModificador = GetCurrentUserId();
            if (idModificador == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var opcion = await _opcionService.GetOpcionByGuidAsync(model.GuidRegistro);
            if (opcion == null)
            {
                return Json(new { success = false, message = "Opcion no encontrada" });
            }

            var result = await _opcionService.DeleteOpcionAsync(opcion.IdOpcion, idModificador);
            if (!result)
            {
                return Json(new { success = false, message = "No se pudo eliminar la opcion" });
            }

            _logger.LogInformation("Opcion eliminada (logico): {Nombre} por usuario {IdUsuario}",
                opcion.Nombre, idModificador);

            return Json(new { success = true, message = "Opcion eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar opcion");
            return Json(new { success = false, message = "Error al eliminar la opcion" });
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
