using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.DTOs.Archivo;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppWebCompaniaMedica.Controllers;

/// <summary>
/// Controlador de pruebas para validar funcionalidades del sistema.
/// Permite probar subida y descarga de archivos con ambos tipos de almacenamiento (FILE/BLOB).
///
/// <author>ADG Vladimir</author>
/// <created>2026-01-29</created>
/// </summary>
public class TestController : BaseController
{
    private readonly IArchivoService _archivoService;
    private readonly IParametroService _parametroService;
    private readonly ILogger<TestController> _logger;
    private readonly IConfiguration _configuration;

    public TestController(
        IArchivoService archivoService,
        IParametroService parametroService,
        ILogger<TestController> logger,
        IConfiguration configuration)
    {
        _archivoService = archivoService;
        _parametroService = parametroService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Muestra el formulario de prueba para subir archivos.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Archivo()
    {
        // Obtener el tipo de almacenamiento actual
        var tipoAlmacenamiento = await _parametroService.GetValorByCodigoAsync("SHM_TIPO_ALMACENAMIENTO_ARCHIVO");
        ViewBag.TipoAlmacenamiento = tipoAlmacenamiento ?? "FILE";

        return View();
    }

    /// <summary>
    /// Procesa la subida de un archivo de prueba.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirArchivo(IFormFile archivo)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                return Json(new { success = false, message = "Debe seleccionar un archivo" });
            }

            // Obtener ID del usuario actual
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var idCreador = int.TryParse(userIdClaim, out var userId) ? userId : 1;

            // Preparar datos del archivo
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            var nombreOriginal = archivo.FileName;
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";

            // Leer contenido del archivo
            byte[] contenidoArchivo;
            using (var memoryStream = new MemoryStream())
            {
                await archivo.CopyToAsync(memoryStream);
                contenidoArchivo = memoryStream.ToArray();
            }

            // Obtener tipo de almacenamiento configurado
            var tipoAlmacenamiento = await _parametroService.GetValorByCodigoAsync("SHM_TIPO_ALMACENAMIENTO_ARCHIVO");
            var usarBlob = tipoAlmacenamiento?.ToUpper() == "BLOB";

            // Preparar ruta para almacenamiento FILE
            string? rutaRelativa = null;
            if (!usarBlob)
            {
                var uploadBasePath = _configuration["FileStorage:UploadPath"]
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var carpetaTest = Path.Combine(uploadBasePath, "test");

                if (!Directory.Exists(carpetaTest))
                    Directory.CreateDirectory(carpetaTest);

                var rutaCompleta = Path.Combine(carpetaTest, nombreArchivo);
                await System.IO.File.WriteAllBytesAsync(rutaCompleta, contenidoArchivo);

                rutaRelativa = $"test/{nombreArchivo}";
            }

            // Crear registro en BD
            var createDto = new CreateArchivoDto
            {
                TipoArchivo = "TEST",
                NombreOriginal = nombreOriginal,
                NombreArchivo = nombreArchivo,
                Extension = extension,
                Tamano = (int)archivo.Length,
                Ruta = rutaRelativa,
                ContenidoArchivo = usarBlob ? contenidoArchivo : null
            };

            var archivoCreado = await _archivoService.CreateArchivoAsync(createDto, idCreador);

            _logger.LogInformation(
                "Archivo de prueba subido exitosamente. ID: {IdArchivo}, GUID: {Guid}, Tipo: {Tipo}",
                archivoCreado.IdArchivo,
                archivoCreado.GuidRegistro,
                archivoCreado.TipoAlmacenamiento);

            return Json(new
            {
                success = true,
                message = "Archivo subido exitosamente",
                data = new
                {
                    idArchivo = archivoCreado.IdArchivo,
                    guidRegistro = archivoCreado.GuidRegistro,
                    nombreOriginal = archivoCreado.NombreOriginal,
                    nombreArchivo = archivoCreado.NombreArchivo,
                    extension = archivoCreado.Extension,
                    tamano = archivoCreado.Tamano,
                    tipoAlmacenamiento = archivoCreado.TipoAlmacenamiento,
                    fechaCreacion = archivoCreado.FechaCreacion.ToString("dd/MM/yyyy HH:mm:ss")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo de prueba");
            return Json(new { success = false, message = $"Error al subir archivo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Descarga un archivo de prueba por su GUID.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DescargarArchivo(string guid)
    {
        try
        {
            if (string.IsNullOrEmpty(guid))
                return NotFound("GUID no especificado");

            var archivoContenido = await _archivoService.GetArchivoContenidoByGuidAsync(guid);
            if (archivoContenido == null)
                return NotFound("Archivo no encontrado");

            // Para PDFs, mostrar inline
            if (archivoContenido.Extension?.ToLower() == ".pdf")
            {
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{archivoContenido.NombreArchivo}\"");
                return File(archivoContenido.Contenido, archivoContenido.ContentType ?? "application/pdf");
            }

            return File(
                archivoContenido.Contenido,
                archivoContenido.ContentType ?? "application/octet-stream",
                archivoContenido.NombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo de prueba con GUID: {Guid}", guid);
            return StatusCode(500, "Error al descargar el archivo");
        }
    }
}
