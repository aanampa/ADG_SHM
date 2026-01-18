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
