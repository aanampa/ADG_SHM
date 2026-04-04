using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Repositories;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador para la consulta del log de correos electronicos enviados por el sistema.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-03</created>
/// </summary>
[Authorize]
public class EmailLogController : Controller
{
    private readonly ILogger<EmailLogController> _logger;
    private readonly IEmailLogRepository _emailLogRepository;

    public EmailLogController(
        ILogger<EmailLogController> logger,
        IEmailLogRepository emailLogRepository)
    {
        _logger = logger;
        _emailLogRepository = emailLogRepository;
    }

    /// <summary>
    /// Vista principal del log de correos.
    /// </summary>
    [HttpGet]
    [Route("EmailLog")]
    [Route("EmailLog/Index")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Obtiene el listado paginado de logs de email (AJAX).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        string? tipoEmail, string? estado, string? emailDestino,
        int pageNumber = 1, int pageSize = 15)
    {
        try
        {
            var (items, totalCount) = await _emailLogRepository.GetPaginatedListAsync(
                tipoEmail, estado, emailDestino, pageNumber, pageSize);

            ViewBag.TipoEmail = tipoEmail;
            ViewBag.Estado = estado;
            ViewBag.EmailDestino = emailDestino;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            _logger.LogInformation("Email log listado. Total: {Total}, Pagina: {Page}", totalCount, pageNumber);

            return PartialView("_ListPartial", items.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar email log");
            return PartialView("_ListPartial", new List<SHM.AppDomain.Entities.EmailLog>());
        }
    }

    /// <summary>
    /// Retorna el contenido HTML del email para mostrarlo en el visor (AJAX).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> VerContenido(int id)
    {
        try
        {
            var emailLog = await _emailLogRepository.GetByIdAsync(id);
            if (emailLog == null)
                return NotFound();

            return Content(emailLog.Contenido ?? "<p class='text-muted'>Sin contenido</p>", "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contenido del email log {Id}", id);
            return BadRequest();
        }
    }
}
