namespace SHM.AppWebCompaniaMedica.Models;

/// <summary>
/// Modelo para almacenar los datos extraidos del XML de factura electronica.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-21</created>
/// </summary>
public class FacturaXmlData
{
    public DatosGenerales DatosGenerales { get; set; } = new();
    public Emisor Emisor { get; set; } = new();
    public Cliente Cliente { get; set; } = new();
    public DetallesPago DetallesPago { get; set; } = new();
    public Impuestos Impuestos { get; set; } = new();
    public DesgloseTotales DesgloseTotales { get; set; } = new();
    public List<DetalleItem> DetalleItems { get; set; } = new();
    public FirmaElectronica? FirmaElectronica { get; set; }
}

public class DatosGenerales
{
    public string NumeroFactura { get; set; } = string.Empty;
    public string FechaEmision { get; set; } = string.Empty;
    public string HoraEmision { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string CodigoTipoDocumento { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty;
    public string TotalEnLetras { get; set; } = string.Empty;
}

public class Emisor
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
    public string Ubigeo { get; set; } = string.Empty;
}

public class Cliente
{
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public class DetallesPago
{
    public string FormaPago { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public List<CuotaPago> Cuotas { get; set; } = new();
}

public class CuotaPago
{
    public string NumeroCuota { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string FechaVencimiento { get; set; } = string.Empty;
}

public class Impuestos
{
    public decimal Igv { get; set; }
    public decimal PorcentajeIgv { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal TotalGravado { get; set; }
    public decimal TotalExonerado { get; set; }
    public decimal TotalInafecto { get; set; }
    public decimal TotalGratuito { get; set; }
}

public class DesgloseTotales
{
    public decimal ValorVenta { get; set; }
    public decimal Igv { get; set; }
    public decimal ImporteTotal { get; set; }
    public decimal Descuentos { get; set; }
    public decimal OtrosCargos { get; set; }
}

public class DetalleItem
{
    public int NumeroItem { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string CodigoUnidadMedida { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal ValorVenta { get; set; }
    public decimal Igv { get; set; }
    public decimal PrecioVentaPublico { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string TipoAfectacionIgv { get; set; } = string.Empty;
}

public class FirmaElectronica
{
    public string CertificadoEmisor { get; set; } = string.Empty;
    public string ValidoHasta { get; set; } = string.Empty;
    public bool FirmaValida { get; set; }
    public string DigestValue { get; set; } = string.Empty;
    public string SignatureValue { get; set; } = string.Empty;
}
