namespace SHM.AppDomain.Constants;

/// <summary>
/// Estados de la liberacion de pagos en SAP (SHM_ORDEN_PAGO_LIBERACION.ESTADO_LIBERACION).
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public static class EstadoLiberacion
{
    public const string Pendiente = "PENDIENTE";
    public const string Liberado  = "LIBERADO";
    public const string Error     = "ERROR";

    public static string GetDescripcion(string? estado) => estado switch
    {
        Pendiente => "Pendiente",
        Liberado  => "Liberado",
        Error     => "Con error",
        _         => estado ?? "-"
    };

    public static string GetBadgeClass(string? estado) => estado switch
    {
        Pendiente => "badge-warning",
        Liberado  => "badge-success",
        Error     => "badge-danger",
        _         => "badge-secondary"
    };
}
