using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Controlador comun para la descarga de archivos del sistema.
/// Soporta almacenamiento dual: FILE (sistema de archivos) y BLOB (base de datos).
///
/// <author>ADG Antonio</author>
/// <created>2026-02-15</created>
/// </summary>
[Authorize]
public class ArchivoController : Controller
{
    private readonly ILogger<ArchivoController> _logger;
    private readonly IArchivoService _archivoService;

    public ArchivoController(
        ILogger<ArchivoController> logger,
        IArchivoService archivoService)
    {
        _logger = logger;
        _archivoService = archivoService;
    }

    /// <summary>
    /// Descarga un archivo por su GUID.
    /// Para PDFs muestra inline en el navegador, para otros archivos fuerza la descarga.
    /// </summary>
    [HttpGet]
    [Route("Archivo/Descargar/{guid}")]
    public async Task<IActionResult> Descargar(string guid)
    {
        try
        {
            if (string.IsNullOrEmpty(guid))
                return NotFound("Archivo no encontrado");

            var archivoContenido = await _archivoService.GetArchivoContenidoByGuidAsync(guid);
            if (archivoContenido == null)
            {
                _logger.LogWarning("Archivo no encontrado o sin contenido: {Guid}", guid);
                return NotFound("Archivo no encontrado");
            }

            // Para PDFs, mostrar inline en el navegador (visor embebido)
            if (archivoContenido.Extension?.ToLower() == ".pdf")
            {
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{archivoContenido.NombreArchivo}\"");
                return File(archivoContenido.Contenido, archivoContenido.ContentType ?? "application/pdf");
            }

            // Para otros archivos, forzar descarga
            return File(
                archivoContenido.Contenido,
                archivoContenido.ContentType ?? "application/octet-stream",
                archivoContenido.NombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo con GUID: {Guid}", guid);
            return StatusCode(500, "Error al descargar el archivo");
        }
    }
}
