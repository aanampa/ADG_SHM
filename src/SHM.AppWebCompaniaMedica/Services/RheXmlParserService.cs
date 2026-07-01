using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SHM.AppWebCompaniaMedica.Models;

namespace SHM.AppWebCompaniaMedica.Services;

/// <summary>
/// Resultado de la validacion del XML de Recibo por Honorarios Electronico.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-14</created>
/// </summary>
public class RheXmlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string ErrorMessage => string.Join("; ", Errors);
}

/// <summary>
/// Servicio para parsear y validar archivos XML de Recibos por Honorarios Electronicos (RHE).
/// Los RHE tienen una estructura similar a las facturas UBL pero con diferencias clave:
/// - El RUC del emisor esta en cbc:CustomerAssignedAccountID (no en PartyIdentification)
/// - La razon social esta en cac:PartyName/cbc:Name
/// - Los impuestos representan retencion de 4ta categoria (no IGV)
/// - TaxCategory ID = "RET 4TA" con porcentaje 8%
///
/// <author>ADG Antonio</author>
/// <created>2026-02-14</created>
/// </summary>
public class RheXmlParserService
{
    private static readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
    private static readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

    /// <summary>
    /// Valida que el XML sea un Recibo por Honorarios Electronico valido.
    /// </summary>
    public RheXmlValidationResult ValidateRheXml(Stream xmlStream)
    {
        var result = new RheXmlValidationResult { IsValid = true };

        try
        {
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

            // Validar que sea un documento Invoice (los RHE usan el mismo elemento raiz)
            var rootName = root.Name.LocalName;
            if (rootName != "Invoice")
            {
                result.IsValid = false;
                result.Errors.Add($"El documento XML no es un Recibo por Honorarios valido. Tipo encontrado: {rootName}");
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

            // Validar emisor (AccountingSupplierParty)
            var supplierParty = root.Element(cac + "AccountingSupplierParty");
            if (supplierParty == null)
            {
                result.IsValid = false;
                result.Errors.Add("El XML no contiene informacion del emisor (AccountingSupplierParty)");
            }
            else
            {
                // En RHE, el RUC esta en cbc:CustomerAssignedAccountID
                var supplierRuc = supplierParty.Element(cbc + "CustomerAssignedAccountID")?.Value;
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
                var customerDoc = customerParty.Element(cbc + "CustomerAssignedAccountID")?.Value;
                if (string.IsNullOrWhiteSpace(customerDoc))
                {
                    result.IsValid = false;
                    result.Errors.Add("El XML no contiene el documento del cliente");
                }
            }

            // Validar que tenga al menos un item
            var invoiceLines = root.Elements(cac + "InvoiceLine").ToList();
            if (invoiceLines.Count == 0)
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
    /// Parsea un archivo XML de Recibo por Honorarios Electronico y extrae sus datos.
    /// Retorna un FacturaXmlData para mantener compatibilidad con el flujo existente.
    /// </summary>
    public FacturaXmlData ParseRheXml(Stream xmlStream)
    {
        var doc = XDocument.Load(xmlStream);
        var root = doc.Root;

        if (root == null)
            throw new InvalidOperationException("El archivo XML no tiene elemento raiz");

        var rheData = new FacturaXmlData
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

        return rheData;
    }

    private DatosGenerales ExtractDatosGenerales(XElement root)
    {
        var datos = new DatosGenerales();

        // Numero de recibo (ID) - formato EXXX-NNN
        datos.NumeroFactura = root.Element(cbc + "ID")?.Value ?? string.Empty;

        // Fecha de emision
        datos.FechaEmision = root.Element(cbc + "IssueDate")?.Value ?? string.Empty;

        // Hora de emision
        datos.HoraEmision = root.Element(cbc + "IssueTime")?.Value ?? string.Empty;

        // Tipo de documento - forzar a "02" (Recibo por Honorarios)
        datos.CodigoTipoDocumento = "02";
        datos.TipoDocumento = "Recibo por Honorarios";

        // Moneda (del PayableAmount o TaxAmount)
        var taxTotal = root.Element(cac + "TaxTotal");
        var currencyId = taxTotal?.Element(cbc + "TaxAmount")?.Attribute("currencyID")?.Value;
        datos.Moneda = currencyId ?? "PEN";

        // Total en letras (Note sin languageLocaleID)
        var note = root.Element(cbc + "Note")?.Value;
        if (!string.IsNullOrWhiteSpace(note))
        {
            datos.TotalEnLetras = note.Trim();
        }

        return datos;
    }

    private Emisor ExtractEmisor(XElement root)
    {
        var emisor = new Emisor();

        var supplierParty = root.Element(cac + "AccountingSupplierParty");
        if (supplierParty == null) return emisor;

        // RUC del emisor - en RHE esta en cbc:CustomerAssignedAccountID
        emisor.Ruc = supplierParty.Element(cbc + "CustomerAssignedAccountID")?.Value?.Trim() ?? string.Empty;

        var party = supplierParty.Element(cac + "Party");
        if (party != null)
        {
            // Razon Social - en RHE esta en PartyName/Name
            var partyName = party.Element(cac + "PartyName");
            emisor.RazonSocial = partyName?.Element(cbc + "Name")?.Value?.Trim() ?? string.Empty;

            // Direccion
            var postalAddress = party.Element(cac + "PostalAddress");
            if (postalAddress != null)
            {
                emisor.Direccion = postalAddress.Element(cbc + "StreetName")?.Value?.Trim() ?? string.Empty;
                emisor.Distrito = postalAddress.Element(cbc + "District")?.Value?.Trim() ?? string.Empty;
                emisor.Provincia = postalAddress.Element(cbc + "CityName")?.Value?.Trim() ?? string.Empty;
                emisor.Departamento = postalAddress.Element(cbc + "CountrySubentity")?.Value?.Trim() ?? string.Empty;

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
        if (customerParty == null) return cliente;

        // RUC del cliente - en RHE esta en cbc:CustomerAssignedAccountID
        cliente.NumeroDocumento = customerParty.Element(cbc + "CustomerAssignedAccountID")?.Value?.Trim() ?? string.Empty;

        // Tipo de documento (AdditionalAccountID: 6 = RUC)
        var additionalAccountID = customerParty.Element(cbc + "AdditionalAccountID")?.Value;
        cliente.TipoDocumento = additionalAccountID switch
        {
            "6" => "6",  // RUC
            "1" => "1",  // DNI
            _ => additionalAccountID ?? string.Empty
        };

        var party = customerParty.Element(cac + "Party");
        if (party != null)
        {
            // Razon Social
            var partyName = party.Element(cac + "PartyName");
            cliente.RazonSocial = partyName?.Element(cbc + "Name")?.Value?.Trim() ?? string.Empty;

            // Direccion
            var postalAddress = party.Element(cac + "PostalAddress");
            if (postalAddress != null)
            {
                cliente.Direccion = postalAddress.Element(cbc + "StreetName")?.Value?.Trim() ?? string.Empty;
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
        }

        return detalles;
    }

    private Impuestos ExtractImpuestos(XElement root)
    {
        var impuestos = new Impuestos();

        var taxTotal = root.Element(cac + "TaxTotal");
        if (taxTotal == null) return impuestos;

        // En RHE, TaxAmount a nivel raiz es la retencion total
        impuestos.Retencion = ParseDecimal(taxTotal.Element(cbc + "TaxAmount")?.Value);

        var taxSubtotal = taxTotal.Element(cac + "TaxSubtotal");
        if (taxSubtotal != null)
        {
            // TaxableAmount = monto bruto (antes de retencion)
            var taxableAmount = ParseDecimal(taxSubtotal.Element(cbc + "TaxableAmount")?.Value);
            impuestos.BaseImponible = taxableAmount;
            impuestos.TotalGravado = taxableAmount;

            // Obtener porcentaje de retencion del item (donde esta TaxCategory "RET 4TA")
            var invoiceLine = root.Element(cac + "InvoiceLine");
            var lineTaxTotal = invoiceLine?.Element(cac + "TaxTotal");
            var lineTaxSubtotal = lineTaxTotal?.Element(cac + "TaxSubtotal");
            var taxCategory = lineTaxSubtotal?.Element(cac + "TaxCategory");
            var taxCategoryId = taxCategory?.Element(cbc + "ID")?.Value;

            if (taxCategoryId == "RET 4TA")
            {
                var percent = ParseDecimal(lineTaxSubtotal?.Element(cbc + "Percent")?.Value);
                impuestos.PorcentajeRetencion = percent;
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
            totales.ImporteTotal = ParseDecimal(legalMonetaryTotal.Element(cbc + "PayableAmount")?.Value);
        }

        // En RHE: ValorVenta = TaxableAmount (monto bruto), Retencion = TaxAmount
        var taxTotal = root.Element(cac + "TaxTotal");
        var taxSubtotal = taxTotal?.Element(cac + "TaxSubtotal");
        totales.ValorVenta = ParseDecimal(taxSubtotal?.Element(cbc + "TaxableAmount")?.Value);
        totales.Retencion = ParseDecimal(taxTotal?.Element(cbc + "TaxAmount")?.Value);

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

            // En RHE, InvoicedQuantity suele ser 0 pero se lee de todas formas
            item.Cantidad = ParseDecimal(line.Element(cbc + "InvoicedQuantity")?.Value);

            var invoicedQuantity = line.Element(cbc + "InvoicedQuantity");
            item.CodigoUnidadMedida = invoicedQuantity?.Attribute("unitCode")?.Value ?? string.Empty;
            item.UnidadMedida = item.CodigoUnidadMedida == "ZZ" ? "Servicio" : item.CodigoUnidadMedida;

            item.ValorVenta = ParseDecimal(line.Element(cbc + "LineExtensionAmount")?.Value);

            // Precio del servicio
            var price = line.Element(cac + "Price");
            item.PrecioUnitario = ParseDecimal(price?.Element(cbc + "PriceAmount")?.Value);

            // Descripcion del servicio
            var itemElement = line.Element(cac + "Item");
            item.Descripcion = itemElement?.Element(cbc + "Description")?.Value?.Trim() ?? string.Empty;

            // Si no hay descripcion en Item, buscar en Note del InvoiceLine
            if (string.IsNullOrEmpty(item.Descripcion))
            {
                item.Descripcion = line.Element(cbc + "Note")?.Value?.Trim() ?? string.Empty;
            }

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
}
