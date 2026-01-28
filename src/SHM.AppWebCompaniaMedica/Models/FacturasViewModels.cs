namespace SHM.AppWebCompaniaMedica.Models;

public class FacturaPendienteViewModel
{
    public int IdProduccion { get; set; }
    public string? CodigoProduccion { get; set; }
    public string? NombreSede { get; set; }
    public string? Concepto { get; set; }
    public decimal? MtoTotal { get; set; }
    public DateTime? FechaLimite { get; set; }
    public string? Estado { get; set; }
    public string? GuidRegistro { get; set; }
}

public class FacturasPendientesViewModel
{
    public List<FacturaPendienteViewModel> Facturas { get; set; } = new();
    public string? Busqueda { get; set; }
    public int TotalRegistros { get; set; }
}

public class FacturaEnviadaViewModel
{
    public int IdProduccion { get; set; }
    public string? CodigoProduccion { get; set; }
    public string? NombreSede { get; set; }
    public string? Concepto { get; set; }
    public decimal? MtoTotal { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public string? EstadoComprobante { get; set; }
    public string? GuidRegistro { get; set; }
}

public class FacturasEnviadasViewModel
{
    public List<FacturaEnviadaViewModel> Facturas { get; set; } = new();
    public string? Busqueda { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalRegistros { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRegistros / PageSize);
}

public class SubirFacturaViewModel
{
    public string? GuidRegistro { get; set; }
    public string? CodigoProduccion { get; set; }
    public string? NombreSede { get; set; }
    public string? Concepto { get; set; }
    public decimal? MtoTotal { get; set; }
    public DateTime? FechaLimite { get; set; }

    // Datos de cuenta bancaria
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }

    // Validacion de cuenta bancaria
    public bool RequiereValidacionCuenta { get; set; }
    public bool TieneCuentaBancaria => !string.IsNullOrEmpty(CuentaCorriente) || !string.IsNullOrEmpty(CuentaCci);
    public bool PuedeSubirFactura => !RequiereValidacionCuenta || TieneCuentaBancaria;

    // Control de archivo CDR
    public bool RequiereCdr { get; set; } = true;
}

public class DetalleFacturaViewModel
{
    public int IdProduccion { get; set; }
    public string? GuidRegistro { get; set; }
    public string? CodigoProduccion { get; set; }
    public string? NombreSede { get; set; }
    public string? Concepto { get; set; }
    public string? Descripcion { get; set; }
    public string? Periodo { get; set; }
    public decimal? MtoConsumo { get; set; }
    public decimal? MtoDescuento { get; set; }
    public decimal? MtoSubtotal { get; set; }
    public decimal? MtoIgv { get; set; }
    public decimal? MtoTotal { get; set; }
    public string? TipoComprobante { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaLimite { get; set; }
    public string? Estado { get; set; }
    public string? EstadoComprobante { get; set; }
    public List<ArchivoAdjuntoViewModel> Archivos { get; set; } = new();

    // Datos de cuenta bancaria
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
}

public class ArchivoAdjuntoViewModel
{
    public string? GuidRegistro { get; set; }
    public string? TipoArchivo { get; set; }
    public string? NombreArchivo { get; set; }
    public string? NombreOriginal { get; set; }
    public string? Extension { get; set; }
    public int? Tamano { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// ViewModel para la pagina de Vista Previa de Factura.
/// </summary>
/// <author>ADG Antonio</author>
/// <created>2026-01-25</created>
public class VistaPreviaFacturaViewModel
{
    // Identificador unico de la sesion de vista previa
    public string? SessionId { get; set; }

    // Datos de la produccion
    public string? GuidRegistro { get; set; }
    public string? CodigoProduccion { get; set; }
    public string? NombreSede { get; set; }
    public string? Concepto { get; set; }
    public decimal? MtoTotal { get; set; }
    public DateTime? FechaLimite { get; set; }

    // Datos del formulario ingresados
    public string? TipoComprobante { get; set; }
    public string? Serie { get; set; }
    public string? Numero { get; set; }
    public DateTime? FechaEmision { get; set; }

    // Datos bancarios
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }

    // Rutas temporales de archivos
    public string? PdfTempPath { get; set; }
    public string? XmlTempPath { get; set; }
    public string? CdrTempPath { get; set; }

    // Datos parseados del XML
    public FacturaXmlData? DatosXml { get; set; }
}
