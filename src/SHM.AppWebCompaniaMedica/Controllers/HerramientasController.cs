using Microsoft.AspNetCore.Mvc;
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
    /// Recibe un archivo XML, lo valida y retorna sus datos parseados en JSON.
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

            if (esRhe)
            {
                // Validar
                RheXmlValidationResult validacion;
                using (var stream = archivoXml.OpenReadStream())
                    validacion = _rheXmlParserService.ValidateRheXml(stream);

                if (!validacion.IsValid)
                    return Json(new { success = false, message = $"El XML no es válido: {validacion.ErrorMessage}" });

                // Parsear
                using var streamParse = archivoXml.OpenReadStream();
                var datos = _rheXmlParserService.ParseRheXml(streamParse);

                return Json(new { success = true, tipoParser = "RHE", datos = BuildResponse(datos) });
            }
            else
            {
                // Validar
                FacturaXmlValidationResult validacion;
                using (var stream = archivoXml.OpenReadStream())
                    validacion = _facturaXmlParserService.ValidateFacturaXml(stream);

                if (!validacion.IsValid)
                    return Json(new { success = false, message = $"El XML no es válido: {validacion.ErrorMessage}" });

                // Parsear
                using var streamParse = archivoXml.OpenReadStream();
                var datos = _facturaXmlParserService.ParseFacturaXml(streamParse);

                return Json(new { success = true, tipoParser = "FACTURA", datos = BuildResponse(datos) });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al evaluar XML");
            return Json(new { success = false, message = $"Error inesperado: {ex.Message}" });
        }
    }

    private static object BuildResponse(SHM.AppWebCompaniaMedica.Models.FacturaXmlData d) => new
    {
        // Datos generales
        numeroDocumento   = d.DatosGenerales.NumeroFactura,
        tipoDocumento     = d.DatosGenerales.TipoDocumento,
        codigoTipo        = d.DatosGenerales.CodigoTipoDocumento,
        fechaEmision      = d.DatosGenerales.FechaEmision,
        horaEmision       = d.DatosGenerales.HoraEmision,
        moneda            = d.DatosGenerales.Moneda,
        totalEnLetras     = d.DatosGenerales.TotalEnLetras,

        // Emisor
        emisorRuc         = d.Emisor.Ruc,
        emisorNombre      = d.Emisor.RazonSocial,
        emisorDireccion   = d.Emisor.Direccion,
        emisorUbicacion   = d.Emisor.Ubicacion,

        // Cliente
        clienteTipoDoc    = d.Cliente.TipoDocumento,
        clienteNumeroDoc  = d.Cliente.NumeroDocumento,
        clienteNombre     = d.Cliente.RazonSocial,
        clienteDireccion  = d.Cliente.Direccion,

        // Totales
        valorVenta        = d.DesgloseTotales.ValorVenta,
        igv               = d.DesgloseTotales.Igv,
        retencion         = d.DesgloseTotales.Retencion,
        importeTotal      = d.DesgloseTotales.ImporteTotal,

        // Impuestos
        porcentajeRetencion = d.Impuestos.PorcentajeRetencion,

        // Items
        items = d.DetalleItems.Select(i => new
        {
            numero      = i.NumeroItem,
            descripcion = i.Descripcion,
            cantidad    = i.Cantidad,
            precio      = i.PrecioUnitario,
            total       = i.ValorVenta
        }).ToList()
    };
}
