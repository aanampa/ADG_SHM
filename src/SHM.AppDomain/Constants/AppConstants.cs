namespace SHM.AppDomain.Constants;

/// <summary>
/// Constantes generales de la aplicacion SHM.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-04-06</created>
/// </summary>
public static class AppConstants
{
    // URL del Portal de Compañías Médicas (usuarios externos).
    // Reemplazado por AppSettings:UrlPortalCompaniaMedica en appsettings.json - 2026-04-11
    // public const string UrlPortalCompaniaMedica = "https://whm.sanpablo.com.pe";

    // URL del Portal Administrativo (usuarios internos).
    // Reemplazado por AppSettings:UrlPortalAdministrativo en appsettings.json - 2026-04-11
    // public const string UrlPortalAdministrativo = "https://whm-admin.sanpablo.com.pe";

    /// <summary>
    /// Constantes relacionadas a la emision de comprobantes de pago de Cias Medicas.
    /// </summary>
    public static class FacturacionConstants
    {
        public const string DescTipoOperacion = "Operación Sujeta al Sistema de Pago de Obligaciones Tributarias con el Gobierno Central";

        public const string FormaPago            = "CREDITO";
        public const int    NroCuotas            = 1;

        public const string CodigoConceptoDetraccion = "037";
        public const string DescConceptoDetraccion   = "Demás servicios gravados con el IGV";

        public const string CodigoMedioPago      = "001";
        public const string DescMedioPago         = "Depósito en cuenta";
    }

    /// <summary>
    /// Constantes para la descripcion de monedas a partir de su codigo ISO 4217.
    /// </summary>
    /// <author>ADG Vladimir D</author>
    /// <created>2026-04-28</created>
    public static class MonedaConstants
    {
        public static string GetDescripcion(string? codigo) => codigo switch
        {
            "PEN" => "Soles",
            "USD" => "Dólares",
            "EUR" => "Euros",
            _     => codigo ?? "-"
        };
    }
}
