namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// Request del servicio de liberacion de pagos de SAP (LIBERA_PAGO_CABSet).
/// Una cabecera (la orden de pago) con la lista de documentos (comprobantes).
/// Los nombres de las propiedades coinciden con los del JSON de SAP (PascalCase).
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public class SapLiberaPagoRequestDto
{
    public string IdOrden { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public List<SapLiberaPagoDocumentoDto> Documentos { get; set; } = new();
}

/// <summary>
/// Documento (comprobante) dentro del request de liberacion de pagos.
/// </summary>
public class SapLiberaPagoDocumentoDto
{
    public string IdOrden { get; set; } = string.Empty;
    public string Sociedad { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Ejercicio { get; set; } = string.Empty;
    public string DocumentoDt { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public string ProveedorName { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public string FechaEmision { get; set; } = string.Empty;
    public string Importe { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty;
    public string UserLiberador { get; set; } = string.Empty;
    public string FechaLibera { get; set; } = string.Empty;
    public string HoraLibera { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de interpretar la respuesta de SAP a la liberacion de pagos.
/// Exito solo es true si SAP respondio 2xx y d.Status = "OK".
/// </summary>
public class SapLiberaPagoResultadoDto
{
    public bool Exito { get; set; }
    public int? HttpStatus { get; set; }
    public string? Status { get; set; }
    public string? Mensaje { get; set; }
    public string? RespuestaRaw { get; set; }
}
