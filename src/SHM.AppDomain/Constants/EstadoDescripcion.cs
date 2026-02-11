namespace SHM.AppDomain.Constants;

/// <summary>
/// Clase estatica que centraliza las descripciones y clases CSS
/// para los campos Estado de las entidades del sistema.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-11</created>
/// </summary>
public static class EstadoDescripcion
{
    /// <summary>
    /// Informacion de presentacion de un estado.
    /// </summary>
    public record EstadoInfo(string Descripcion, string BadgeClass, string BgClass, string TextClass);

    /// <summary>
    /// Estados de la Produccion (SHM_PRODUCCION.ESTADO).
    /// </summary>
    public static class Produccion
    {
        public const string FacturaPendiente   = "FACTURA_PENDIENTE";
        public const string FacturaSolicitada  = "FACTURA_SOLICITADA";
        public const string FacturaEnviada     = "FACTURA_ENVIADA";
        public const string FacturaAceptada    = "FACTURA_ACEPTADA";
        public const string FacturaEnviadaHhmm = "FACTURA_ENVIADA_HHMM";
        public const string FacturaLiquidada   = "FACTURA_LIQUIDADA";
        public const string FacturaOrdenPago   = "FACTURA_ORDEN_PAGO";
        public const string FacturaPagada      = "FACTURA_PAGADA";
        public const string FacturaDevuelta    = "FACTURA_DEVUELTA";
        public const string FacturaAnulada     = "FACTURA_ANULADA";

        private static readonly Dictionary<string, EstadoInfo> _estados = new()
        {
            [FacturaPendiente]   = new("Factura Pendiente",       "badge-secondary",  "bg-gradient-secondary", "text-secondary"),
            [FacturaSolicitada]  = new("Factura Solicitada",      "badge-warning",    "bg-gradient-warning",   "text-warning"),
            [FacturaEnviada]     = new("Factura Enviada",         "badge-info",       "bg-gradient-info",      "text-info"),
            [FacturaAceptada]    = new("Factura Aceptada",        "badge-primary",    "bg-gradient-primary",   "text-primary"),
            [FacturaEnviadaHhmm] = new("Factura Enviada a HHMM", "badge-info",       "bg-gradient-info",      "text-info"),
            [FacturaLiquidada]   = new("Factura Liquidada",       "badge-success",    "bg-gradient-success",   "text-success"),
            [FacturaOrdenPago]   = new("Factura en Orden de Pago","badge-success",    "bg-gradient-success",   "text-success"),
            [FacturaPagada]      = new("Factura Pagada",          "badge-success",    "bg-gradient-success",   "text-success"),
            [FacturaDevuelta]    = new("Factura Devuelta",        "badge-danger",     "bg-gradient-danger",    "text-danger"),
            [FacturaAnulada]     = new("Factura Anulada",         "badge-dark",       "bg-gradient-dark",      "text-dark"),
        };

        public static string GetDescripcion(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.Descripcion : estado ?? "-";

        public static string GetBadgeClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BadgeClass : "badge-secondary";

        public static string GetBgClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BgClass : "bg-gradient-secondary";

        public static string GetTextClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.TextClass : "text-secondary";
    }

    /// <summary>
    /// Estados del Comprobante (SHM_PRODUCCION.ESTADO_COMPROBANTE).
    /// </summary>
    public static class EstadoComprobante
    {
        public const string PorEnviar = "POR_ENVIAR";
        public const string Enviado   = "ENVIADO";
        public const string Aceptado  = "ACEPTADO";
        public const string Devuelto  = "DEVUELTO";
        public const string Pagado    = "PAGADO";

        private static readonly Dictionary<string, EstadoInfo> _estados = new()
        {
            [PorEnviar] = new("Por Enviar", "badge-secondary", "bg-gradient-secondary", "text-secondary"),
            [Enviado]   = new("Enviado",    "badge-info",      "bg-gradient-info",      "text-info"),
            [Aceptado]  = new("Aceptado",   "badge-success",   "bg-gradient-success",   "text-success"),
            [Devuelto]  = new("Devuelto",   "badge-danger",    "bg-gradient-danger",    "text-danger"),
            [Pagado]    = new("Pagado",     "badge-success",   "bg-gradient-success",   "text-success"),
        };

        public static string GetDescripcion(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.Descripcion : estado ?? "-";

        public static string GetBadgeClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BadgeClass : "badge-secondary";

        public static string GetBgClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BgClass : "bg-gradient-secondary";

        public static string GetTextClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.TextClass : "text-secondary";
    }

    /// <summary>
    /// Estados de la Orden de Pago (SHM_ORDEN_PAGO.ESTADO).
    /// </summary>
    public static class OrdenPago
    {
        public const string AprobacionPendiente = "APROBACION_PENDIENTE";
        public const string Devuelto            = "DEVUELTO";
        public const string Aprobado            = "APROBADO";

        private static readonly Dictionary<string, EstadoInfo> _estados = new()
        {
            [AprobacionPendiente] = new("Aprobaci\u00f3n Pendiente", "badge-warning", "bg-gradient-info",    "text-info"),
            [Devuelto]            = new("Devuelto",              "badge-danger",  "bg-gradient-danger",  "text-danger"),
            [Aprobado]            = new("Aprobado",              "badge-success", "bg-gradient-success", "text-success"),
        };

        public static string GetDescripcion(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.Descripcion : estado ?? "-";

        public static string GetBadgeClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BadgeClass : "badge-secondary";

        public static string GetBgClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BgClass : "bg-gradient-secondary";

        public static string GetTextClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.TextClass : "text-secondary";
    }

    /// <summary>
    /// Estados de la Aprobacion de Orden de Pago (SHM_ORDEN_PAGO_APROBACION.ESTADO).
    /// </summary>
    public static class Aprobacion
    {
        public const string Pendiente = "APROBACION_PENDIENTE";
        public const string Aprobado  = "APROBADO";
        public const string Devuelto  = "DEVUELTO";

        private static readonly Dictionary<string, EstadoInfo> _estados = new()
        {
            [Pendiente] = new("Aprobaci\u00f3n Pendiente", "badge-warning", "bg-gradient-warning", "text-warning"),
            [Aprobado]  = new("Aprobado",              "badge-success", "bg-gradient-success", "text-success"),
            [Devuelto]  = new("Devuelto",              "badge-danger",  "bg-gradient-danger",  "text-danger"),
        };

        public static string GetDescripcion(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.Descripcion : estado ?? "-";

        public static string GetBadgeClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BadgeClass : "badge-secondary";

        public static string GetBgClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.BgClass : "bg-gradient-secondary";

        public static string GetTextClass(string? estado) =>
            estado != null && _estados.TryGetValue(estado, out var info) ? info.TextClass : "text-secondary";
    }
}
