using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppWebHonorarioMedico.Controllers;

/// <summary>
/// Endpoints auxiliares para casos puntuales de la liberacion de pagos en SAP:
/// regenerar la trama JSON de una orden y reenviar la liberacion.
/// Se invocan desde fuera del portal (Postman, archivo .http), no tienen pantalla.
///
/// Seguridad: requieren el header X-Api-Key con el valor de la configuracion
/// AuxiliarApi:ApiKey. Si la clave no esta configurada, los endpoints quedan
/// deshabilitados (responden 404).
///
/// <author>ADG Antonio</author>
/// <created>2026-11-14</created>
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/auxiliar/liberacion")]
public class AuxiliarLiberacionController : ControllerBase
{
    private readonly IOrdenPagoLiberacionService _liberacionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuxiliarLiberacionController> _logger;

    public AuxiliarLiberacionController(
        IOrdenPagoLiberacionService liberacionService,
        IConfiguration configuration,
        ILogger<AuxiliarLiberacionController> logger)
    {
        _liberacionService = liberacionService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Regenera la trama JSON que se enviaria a SAP para la orden, con los datos actuales.
    /// No envia nada a SAP ni modifica datos. Si la orden es valida responde el JSON tal
    /// como se enviaria. Si algunos comprobantes tienen errores devuelve la trama de los validos
    /// y los errores en el header X-Comprobantes-Sin-Enviar; si ninguno es valido, responde 422.
    /// </summary>
    /// <param name="numeroOrden">Numero de la orden de pago (ej. OP-09-20261104).</param>
    /// <param name="indentado">true para devolver el JSON con sangria (solo para leerlo).</param>
    [HttpGet("{numeroOrden}/trama")]
    public async Task<IActionResult> Trama(string numeroOrden, [FromQuery] bool indentado = false)
    {
        var rechazo = ValidarAcceso();
        if (rechazo != null) return rechazo;

        _logger.LogInformation("Auxiliar liberacion: regenerar trama. Orden {NumeroOrden}, IP {Ip}",
            numeroOrden, HttpContext.Connection.RemoteIpAddress);

        var trama = await _liberacionService.GenerarTramaAsync(numeroOrden);
        if (trama.Request == null)
        {
            return UnprocessableEntity(new
            {
                valida = false,
                numeroOrden = trama.NumeroOrden,
                estadoOrden = trama.EstadoOrden,
                errores = trama.Errores
            });
        }

        // Liberacion parcial: la trama lleva solo los comprobantes validos; los errores van en un header
        if (!trama.Valida)
            Response.Headers["X-Comprobantes-Sin-Enviar"] = string.Join(" | ", trama.Errores).Replace("\r", " ").Replace("\n", " ");

        // Misma serializacion que usa el envio real (propiedades en PascalCase)
        var json = JsonSerializer.Serialize(trama.Request, new JsonSerializerOptions { WriteIndented = indentado });
        return Content(json, "application/json");
    }

    /// <summary>
    /// Reenvia a SAP la liberacion de la orden. Por defecto no reenvia si todos los
    /// comprobantes ya estan liberados; con forzar=true la reenvia igual (caso puntual,
    /// por ejemplo cuando SAP necesita reprocesar una trama ya recibida).
    /// El resultado se guarda y queda en la bitacora de la orden, igual que una liberacion normal.
    /// </summary>
    /// <param name="numeroOrden">Numero de la orden de pago (ej. OP-09-20261104).</param>
    /// <param name="forzar">Reenviar aunque la orden figure como liberada.</param>
    [HttpPost("{numeroOrden}/reenviar")]
    public async Task<IActionResult> Reenviar(string numeroOrden, [FromQuery] bool forzar = false)
    {
        var rechazo = ValidarAcceso();
        if (rechazo != null) return rechazo;

        var idUsuario = _configuration.GetValue<int?>("AuxiliarApi:IdUsuario") ?? 1;

        _logger.LogWarning("Auxiliar liberacion: reenviar. Orden {NumeroOrden}, forzar {Forzar}, IP {Ip}",
            numeroOrden, forzar, HttpContext.Connection.RemoteIpAddress);

        var (exito, mensaje) = await _liberacionService.LiberarPorNumeroAsync(numeroOrden, idUsuario, forzar);

        var comprobantes = (await _liberacionService.GetByNumeroOrdenPagoAsync(numeroOrden))
            .Select(l => new
            {
                documento = l.Documento,
                documentoDt = l.DocumentoDt,
                estado = l.EstadoLiberacion,
                intentos = l.Intentos,
                fechaLiberacion = l.FechaLiberacion,
                mensajeSap = l.MensajeSap
            })
            .ToList();

        var respuesta = new { success = exito, message = mensaje, numeroOrden, comprobantes };
        return exito ? Ok(respuesta) : UnprocessableEntity(respuesta);
    }

    /// <summary>
    /// Verifica el header X-Api-Key contra AuxiliarApi:ApiKey. Devuelve null si es valido.
    /// </summary>
    private IActionResult? ValidarAcceso()
    {
        var configurada = _configuration["AuxiliarApi:ApiKey"];
        if (string.IsNullOrWhiteSpace(configurada))
            return NotFound(); // herramienta deshabilitada

        var enviada = Request.Headers["X-Api-Key"].ToString();
        var a = Encoding.UTF8.GetBytes(configurada);
        var b = Encoding.UTF8.GetBytes(enviada);

        if (a.Length != b.Length || !CryptographicOperations.FixedTimeEquals(a, b))
        {
            _logger.LogWarning("Auxiliar liberacion: clave invalida. IP {Ip}, ruta {Ruta}",
                HttpContext.Connection.RemoteIpAddress, Request.Path);
            return Unauthorized(new { message = "Clave invalida." });
        }

        return null;
    }
}
