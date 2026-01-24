using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SHM.AppWebCompaniaMedica.Models;

namespace SHM.AppWebCompaniaMedica.Services;

/// <summary>
/// Resultado de la validacion del XML de factura.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-22</created>
/// </summary>
public class FacturaXmlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string ErrorMessage => string.Join("; ", Errors);
}

/// <summary>
/// Servicio para parsear y validar archivos XML de facturas electronicas UBL 2.1 (SUNAT Peru).
///
/// <author>ADG Antonio</author>
/// <created>2026-01-21</created>
/// <modified>ADG Antonio - 2026-01-22 - Agregado metodo de validacion de XML</modified>
/// </summary>
public class FacturaXmlParserService
{
    private static readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
    private static readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

    /// <summary>
    /// Valida que el XML sea una factura electronica UBL 2.1 valida.
    /// </summary>
    public FacturaXmlValidationResult ValidateFacturaXml(Stream xmlStream)
    {
        var result = new FacturaXmlValidationResult { IsValid = true };

        try
        {
            // Resetear posicion del stream
            if (xmlStream.CanSeek)
                xmlStream.Position = 0;

            var doc = XDocument.Load(xmlStream);
            var root = doc.Root;

            if (root == null)
            {
                result.IsValid = false;
                result.Errors.Add("El archivo no contiene un documento XML valido");
                return result;
            }

            // Validar que sea un documento Invoice o CreditNote o DebitNote
            var rootName = root.Name.LocalName;
            if (rootName != "Invoice" && rootName != "CreditNote" && rootName != "DebitNote")
            {
                result.IsValid = false;
                result.Errors.Add($"El documento XML no es una factura electronica valida. Tipo encontrado: {rootName}");
                return result;
            }

            // Validar numero de documento (ID)
            var id = root.Element(cbc + "ID")?.Value;
            if (string.IsNullOrWhiteSpace(id))
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene el numero de documento (ID)");
            }
            else if (!id.Contains('-'))
            {
                result.IsValid = false;
                result.Errors.Add($"El numero de documento '{id}' no tiene el formato valido (SERIE-NUMERO)");
            }

            // Validar fecha de emision
            var issueDate = root.Element(cbc + "IssueDate")?.Value;
            if (string.IsNullOrWhiteSpace(issueDate))
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene la fecha de emision (IssueDate)");
            }

            // Validar tipo de documento
            var invoiceTypeCode = root.Element(cbc + "InvoiceTypeCode")?.Value;
            if (rootName == "Invoice" && string.IsNullOrWhiteSpace(invoiceTypeCode))
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene el tipo de documento (InvoiceTypeCode)");
            }

            // Validar emisor (AccountingSupplierParty)
            var supplierParty = root.Element(cac + "AccountingSupplierParty");
            if (supplierParty == null)
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene informacion del emisor (AccountingSupplierParty)");
            }
            else
            {
                var supplierRuc = supplierParty
                    .Element(cac + "Party")?
                    .Element(cac + "PartyIdentification")?
                    .Element(cbc + "ID")?.Value;

                if (string.IsNullOrWhiteSpace(supplierRuc))
                {
                    result.IsValid = false;
                    result.Errors.Add("El XML no contiene el RUC del emisor");
                }
            }

            // Validar cliente (AccountingCustomerParty)
            var customerParty = root.Element(cac + "AccountingCustomerParty");
            if (customerParty == null)
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene informacion del cliente (AccountingCustomerParty)");
            }
            else
            {
                var customerDoc = customerParty
                    .Element(cac + "Party")?
                    .Element(cac + "PartyIdentification")?
                    .Element(cbc + "ID")?.Value;

                if (string.IsNullOrWhiteSpace(customerDoc))
                {
                    result.IsValid = false;
                    result.Errors.Add("El XML no contiene el documento del cliente");
                }
            }

            // Validar que tenga al menos un item
            var invoiceLines = root.Elements(cac + "InvoiceLine").ToList();
            var creditNoteLines = root.Elements(cac + "CreditNoteLine").ToList();
            var debitNoteLines = root.Elements(cac + "DebitNoteLine").ToList();

            if (invoiceLines.Count == 0 && creditNoteLines.Count == 0 && debitNoteLines.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene items de detalle");
            }

            // Validar montos totales
            var legalMonetaryTotal = root.Element(cac + "LegalMonetaryTotal");
            if (legalMonetaryTotal == null)
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene los totales del documento (LegalMonetaryTotal)");
            }
            else
            {
                var payableAmount = legalMonetaryTotal.Element(cbc + "PayableAmount")?.Value;
                if (string.IsNullOrWhiteSpace(payableAmount))
                {
                    result.IsValid = false;
                    result.Errors.Add("El XML no contiene el importe total a pagar (PayableAmount)");
                }
            }

            // Resetear posicion del stream para uso posterior
            if (xmlStream.CanSeek)
                xmlStream.Position = 0;
        }
        catch (XmlException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"El archivo no es un XML valido: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Error al validar el XML: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Parsea un archivo XML de factura electronica y extrae sus datos.
    /// </summary>
    public FacturaXmlData ParseFacturaXml(Stream xmlStream)
    {
        var doc = XDocument.Load(xmlStream);
        var root = doc.Root;

        if (root == null)
            throw new InvalidOperationException("El archivo XML no tiene elemento raiz");

        var facturaData = new FacturaXmlData
        {
            DatosGenerales = ExtractDatosGenerales(root),
            Emisor = ExtractEmisor(root),
            Cliente = ExtractCliente(root),
            DetallesPago = ExtractDetallesPago(root),
            Impuestos = ExtractImpuestos(root),
            DesgloseTotales = ExtractDesgloseTotales(root),
            DetalleItems = ExtractDetalleItems(root),
            FirmaElectronica = ExtractFirmaElectronica(root)
        };

        return facturaData;
    }

    /// <summary>
    /// Parsea un archivo XML de factura electronica desde una ruta de archivo.
    /// </summary>
    public FacturaXmlData ParseFacturaXml(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return ParseFacturaXml(stream);
    }

    private DatosGenerales ExtractDatosGenerales(XElement root)
    {
        var datos = new DatosGenerales();

        // Numero de factura (ID)
        datos.NumeroFactura = root.Element(cbc + "ID")?.Value ?? string.Empty;

        // Fecha de emision
        datos.FechaEmision = root.Element(cbc + "IssueDate")?.Value ?? string.Empty;

        // Hora de emision
        datos.HoraEmision = root.Element(cbc + "IssueTime")?.Value ?? string.Empty;

        // Tipo de documento
        var invoiceTypeCode = root.Element(cbc + "InvoiceTypeCode");
        datos.CodigoTipoDocumento = invoiceTypeCode?.Value ?? string.Empty;
        datos.TipoDocumento = GetTipoDocumentoDescripcion(datos.CodigoTipoDocumento);

        // Moneda
        var documentCurrencyCode = root.Element(cbc + "DocumentCurrencyCode");
        datos.Moneda = documentCurrencyCode?.Value ?? string.Empty;

        // Total en letras (Note con codigo 1000)
        var notes = root.Elements(cbc + "Note");
        foreach (var note in notes)
        {
            var languageLocaleID = note.Attribute("languageLocaleID")?.Value;
            if (languageLocaleID == "1000")
            {
                datos.TotalEnLetras = note.Value;
                break;
            }
        }

        return datos;
    }

    private Emisor ExtractEmisor(XElement root)
    {
        var emisor = new Emisor();

        var supplierParty = root.Element(cac + "AccountingSupplierParty");
        var party = supplierParty?.Element(cac + "Party");

        if (party != null)
        {
            // RUC
            var partyIdentification = party.Element(cac + "PartyIdentification");
            emisor.Ruc = partyIdentification?.Element(cbc + "ID")?.Value ?? string.Empty;

            // Razon Social
            var partyName = party.Element(cac + "PartyName");
            emisor.RazonSocial = partyName?.Element(cbc + "Name")?.Value ?? string.Empty;

            // Si no hay PartyName, buscar en PartyLegalEntity
            if (string.IsNullOrEmpty(emisor.RazonSocial))
            {
                var partyLegalEntity = party.Element(cac + "PartyLegalEntity");
                emisor.RazonSocial = partyLegalEntity?.Element(cbc + "RegistrationName")?.Value ?? string.Empty;
            }

            // Direccion
            var postalAddress = party.Element(cac + "PostalAddress")
                ?? party.Element(cac + "PartyLegalEntity")?.Element(cac + "RegistrationAddress");

            if (postalAddress != null)
            {
                var streetName = postalAddress.Element(cbc + "StreetName")?.Value ?? string.Empty;
                var addressLine = postalAddress.Element(cac + "AddressLine")?.Element(cbc + "Line")?.Value;
                emisor.Direccion = !string.IsNullOrEmpty(addressLine) ? addressLine : streetName;

                emisor.Distrito = postalAddress.Element(cbc + "District")?.Value ?? string.Empty;
                emisor.Provincia = postalAddress.Element(cbc + "CityName")?.Value ?? string.Empty;
                emisor.Departamento = postalAddress.Element(cbc + "CountrySubentity")?.Value ?? string.Empty;
                emisor.Ubigeo = postalAddress.Element(cbc + "ID")?.Value ?? string.Empty;

                if (!string.IsNullOrEmpty(emisor.Departamento) || !string.IsNullOrEmpty(emisor.Provincia) || !string.IsNullOrEmpty(emisor.Distrito))
                {
                    emisor.Ubicacion = $"{emisor.Departamento} - {emisor.Provincia} - {emisor.Distrito}".Trim(' ', '-');
                }
            }
        }

        return emisor;
    }

    private Cliente ExtractCliente(XElement root)
    {
        var cliente = new Cliente();

        var customerParty = root.Element(cac + "AccountingCustomerParty");
        var party = customerParty?.Element(cac + "Party");

        if (party != null)
        {
            // Tipo y numero de documento
            var partyIdentification = party.Element(cac + "PartyIdentification");
            var idElement = partyIdentification?.Element(cbc + "ID");
            cliente.NumeroDocumento = idElement?.Value ?? string.Empty;
            cliente.TipoDocumento = idElement?.Attribute("schemeID")?.Value ?? string.Empty;

            // Razon Social
            var partyLegalEntity = party.Element(cac + "PartyLegalEntity");
            cliente.RazonSocial = partyLegalEntity?.Element(cbc + "RegistrationName")?.Value ?? string.Empty;

            // Direccion
            var registrationAddress = partyLegalEntity?.Element(cac + "RegistrationAddress");
            if (registrationAddress != null)
            {
                var addressLine = registrationAddress.Element(cac + "AddressLine")?.Element(cbc + "Line")?.Value;
                var streetName = registrationAddress.Element(cbc + "StreetName")?.Value;
                cliente.Direccion = !string.IsNullOrEmpty(addressLine) ? addressLine : streetName ?? string.Empty;
            }
        }

        return cliente;
    }

    private DetallesPago ExtractDetallesPago(XElement root)
    {
        var detalles = new DetallesPago();

        var paymentTerms = root.Elements(cac + "PaymentTerms").ToList();

        foreach (var paymentTerm in paymentTerms)
        {
            var paymentMeansID = paymentTerm.Element(cbc + "PaymentMeansID")?.Value;

            if (paymentMeansID == "Contado" || paymentMeansID == "Credito")
            {
                detalles.FormaPago = paymentMeansID;

                var amount = paymentTerm.Element(cbc + "Amount");
                if (amount != null)
                {
                    detalles.MontoTotal = ParseDecimal(amount.Value);
                    detalles.Moneda = amount.Attribute("currencyID")?.Value ?? string.Empty;
                }
            }
            else if (paymentMeansID?.StartsWith("Cuota") == true)
            {
                var cuota = new CuotaPago
                {
                    NumeroCuota = paymentMeansID
                };

                var amount = paymentTerm.Element(cbc + "Amount");
                if (amount != null)
                {
                    cuota.Monto = ParseDecimal(amount.Value);
                }

                var paymentDueDate = paymentTerm.Element(cbc + "PaymentDueDate");
                cuota.FechaVencimiento = paymentDueDate?.Value ?? string.Empty;

                detalles.Cuotas.Add(cuota);
            }
        }

        return detalles;
    }

    private Impuestos ExtractImpuestos(XElement root)
    {
        var impuestos = new Impuestos();

        var taxTotal = root.Element(cac + "TaxTotal");
        if (taxTotal != null)
        {
            var taxAmount = taxTotal.Element(cbc + "TaxAmount");
            impuestos.Igv = ParseDecimal(taxAmount?.Value);

            var taxSubtotal = taxTotal.Element(cac + "TaxSubtotal");
            if (taxSubtotal != null)
            {
                var taxableAmount = taxSubtotal.Element(cbc + "TaxableAmount");
                impuestos.BaseImponible = ParseDecimal(taxableAmount?.Value);

                var taxCategory = taxSubtotal.Element(cac + "TaxCategory");
                var percent = taxCategory?.Element(cbc + "Percent");
                impuestos.PorcentajeIgv = ParseDecimal(percent?.Value);

                impuestos.TotalGravado = impuestos.BaseImponible;
            }
        }

        return impuestos;
    }

    private DesgloseTotales ExtractDesgloseTotales(XElement root)
    {
        var totales = new DesgloseTotales();

        var legalMonetaryTotal = root.Element(cac + "LegalMonetaryTotal");
        if (legalMonetaryTotal != null)
        {
            totales.ValorVenta = ParseDecimal(legalMonetaryTotal.Element(cbc + "LineExtensionAmount")?.Value);
            totales.ImporteTotal = ParseDecimal(legalMonetaryTotal.Element(cbc + "PayableAmount")?.Value);
            totales.Descuentos = ParseDecimal(legalMonetaryTotal.Element(cbc + "AllowanceTotalAmount")?.Value);
            totales.OtrosCargos = ParseDecimal(legalMonetaryTotal.Element(cbc + "ChargeTotalAmount")?.Value);
        }

        var taxTotal = root.Element(cac + "TaxTotal");
        totales.Igv = ParseDecimal(taxTotal?.Element(cbc + "TaxAmount")?.Value);

        return totales;
    }

    private List<DetalleItem> ExtractDetalleItems(XElement root)
    {
        var items = new List<DetalleItem>();

        var invoiceLines = root.Elements(cac + "InvoiceLine");
        foreach (var line in invoiceLines)
        {
            var item = new DetalleItem();

            item.NumeroItem = int.TryParse(line.Element(cbc + "ID")?.Value, out var num) ? num : 0;
            item.Cantidad = ParseDecimal(line.Element(cbc + "InvoicedQuantity")?.Value);

            var invoicedQuantity = line.Element(cbc + "InvoicedQuantity");
            item.CodigoUnidadMedida = invoicedQuantity?.Attribute("unitCode")?.Value ?? string.Empty;
            item.UnidadMedida = GetUnidadMedidaDescripcion(item.CodigoUnidadMedida);

            item.ValorVenta = ParseDecimal(line.Element(cbc + "LineExtensionAmount")?.Value);

            // Precio unitario
            var pricingReference = line.Element(cac + "PricingReference");
            var alternativeConditionPrice = pricingReference?.Element(cac + "AlternativeConditionPrice");
            item.PrecioVentaPublico = ParseDecimal(alternativeConditionPrice?.Element(cbc + "PriceAmount")?.Value);

            var price = line.Element(cac + "Price");
            item.PrecioUnitario = ParseDecimal(price?.Element(cbc + "PriceAmount")?.Value);

            // IGV del item
            var taxTotal = line.Element(cac + "TaxTotal");
            item.Igv = ParseDecimal(taxTotal?.Element(cbc + "TaxAmount")?.Value);

            var taxSubtotal = taxTotal?.Element(cac + "TaxSubtotal");
            var taxCategory = taxSubtotal?.Element(cac + "TaxCategory");
            item.TipoAfectacionIgv = taxCategory?.Element(cbc + "TaxExemptionReasonCode")?.Value ?? string.Empty;

            // Descripcion del item
            var itemElement = line.Element(cac + "Item");
            item.Descripcion = itemElement?.Element(cbc + "Description")?.Value ?? string.Empty;

            var sellersItemIdentification = itemElement?.Element(cac + "SellersItemIdentification");
            item.CodigoProducto = sellersItemIdentification?.Element(cbc + "ID")?.Value ?? string.Empty;

            items.Add(item);
        }

        return items;
    }

    private FirmaElectronica? ExtractFirmaElectronica(XElement root)
    {
        var signature = root.Descendants(ds + "Signature").FirstOrDefault();
        if (signature == null)
            return null;

        var firma = new FirmaElectronica();

        var x509Certificate = signature.Descendants(ds + "X509Certificate").FirstOrDefault();
        if (x509Certificate != null)
        {
            firma.FirmaValida = true;
        }

        var digestValue = signature.Descendants(ds + "DigestValue").FirstOrDefault();
        firma.DigestValue = digestValue?.Value ?? string.Empty;

        var signatureValue = signature.Descendants(ds + "SignatureValue").FirstOrDefault();
        firma.SignatureValue = signatureValue?.Value ?? string.Empty;

        // Intentar extraer info del certificado desde UBLExtensions
        var ublExtensions = root.Element(ext + "UBLExtensions");
        var extensionContent = ublExtensions?.Descendants(ext + "ExtensionContent").FirstOrDefault();

        // Buscar informacion del certificado
        var x509IssuerName = signature.Descendants(ds + "X509IssuerName").FirstOrDefault();
        firma.CertificadoEmisor = x509IssuerName?.Value ?? string.Empty;

        return firma;
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }

    private static string GetTipoDocumentoDescripcion(string codigo)
    {
        return codigo switch
        {
            "01" => "Factura",
            "03" => "Boleta de Venta",
            "07" => "Nota de Credito",
            "08" => "Nota de Debito",
            "09" => "Guia de Remision Remitente",
            "31" => "Guia de Remision Transportista",
            _ => codigo
        };
    }

    private static string GetUnidadMedidaDescripcion(string codigo)
    {
        return codigo switch
        {
            "NIU" => "Unidad",
            "ZZ" => "Servicio",
            "KGM" => "Kilogramo",
            "LTR" => "Litro",
            "MTR" => "Metro",
            "GLL" => "Galon",
            "HUR" => "Hora",
            "DAY" => "Dia",
            "MON" => "Mes",
            _ => codigo
        };
    }
}
