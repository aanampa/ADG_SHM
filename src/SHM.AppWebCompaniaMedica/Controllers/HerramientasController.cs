using Microsoft.AspNetCore.Mvc;
using SHM.AppWebCompaniaMedica.Models;
using SHM.AppWebCompaniaMedica.Services;

namespace SHM.AppWebCompaniaMedica.Controllers;

/// <summary>
/// Controlador de herramientas utilitarias del portal.
/// </summary>
/// <author>ADG Antonio</author>
/// <created>2026-03-25</created>
public class HerramientasController : BaseController
{
    private readonly FacturaXmlParserService _facturaXmlParserService;
    private readonly RheXmlParserService _rheXmlParserService;
    private readonly ILogger<HerramientasController> _logger;

    public HerramientasController(
        FacturaXmlParserService facturaXmlParserService,
        RheXmlParserService rheXmlParserService,
        ILogger<HerramientasController> logger)
    {
        _facturaXmlParserService = facturaXmlParserService;
        _rheXmlParserService = rheXmlParserService;
        _logger = logger;
    }

    /// <summary>
    /// Muestra la pantalla del evaluador de XML.
    /// </summary>
    [HttpGet]
    public IActionResult EvaluadorXml()
    {
        ViewData["Title"] = "Evaluador de XML";
        return View();
    }

    /// <summary>
    /// Recibe un archivo XML, lo valida y retorna el partial _DatosXmlPartial con los datos extraídos.
    /// Retorna JSON con success=false en caso de error de validación.
    /// </summary>
    [HttpPost]
    public IActionResult EvaluarXml([FromForm] IFormFile? archivoXml, [FromForm] string? tipoComprobante)
    {
        try
        {
            if (archivoXml == null || archivoXml.Length == 0)
                return Json(new { success = false, message = "Debe seleccionar un archivo XML." });

            if (!archivoXml.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "El archivo debe tener extension .xml" });

            if (string.IsNullOrEmpty(tipoComprobante))
                return Json(new { success = false, message = "Debe indicar el tipo de comprobante." });

            var esRhe = tipoComprobante == "22" || tipoComprobante == "02";
            FacturaXmlData datos;

            if (esRhe)
            {
                RheXmlValidationResult validacion;
                using (var stream = archivoXml.OpenReadStream())
                    validacion = _rheXmlParserService.ValidateRheXml(stream);

                if (!validacion.IsValid)
                    return Json(new { success = false, message = $"El XML no es válido: {validacion.ErrorMessage}" });

                using var streamParse = archivoXml.OpenReadStream();
                datos = _rheXmlParserService.ParseRheXml(streamParse);
            }
            else
            {
                FacturaXmlValidationResult validacion;
                using (var stream = archivoXml.OpenReadStream())
                    validacion = _facturaXmlParserService.ValidateFacturaXml(stream);

                if (!validacion.IsValid)
                    return Json(new { success = false, message = $"El XML no es válido: {validacion.ErrorMessage}" });

                using var streamParse = archivoXml.OpenReadStream();
                datos = _facturaXmlParserService.ParseFacturaXml(streamParse);
            }

            var viewModel = new VistaPreviaFacturaViewModel
            {
                TipoComprobante = tipoComprobante,
                DatosXml        = datos
            };

            return PartialView("../Facturas/_DatosXmlPartial", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al evaluar XML");
            return Json(new { success = false, message = $"Error inesperado: {ex.Message}" });
        }
    }
}
