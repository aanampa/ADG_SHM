namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para el resultado de la creacion masiva de producciones a traves de interface.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-02-08 - Agregado detalle por registro y CantidadErrores</modified>
/// </summary>
public class InterfaceProduccionResultDto
{
    /// <summary>
    /// Cantidad de registros creados/actualizados exitosamente.
    /// </summary>
    public int CantidadCreados { get; set; }

    /// <summary>
    /// Cantidad de registros obviados por ya existir en el sistema.
    /// </summary>
    public int CantidadObviados { get; set; }

    /// <summary>
    /// Cantidad de registros con error.
    /// </summary>
    public int CantidadErrores { get; set; }

    /// <summary>
    /// Total de registros procesados.
    /// </summary>
    public int TotalProcesados => CantidadCreados + CantidadObviados + CantidadErrores;

    /// <summary>
    /// Detalle del estado de proceso de cada registro recibido.
    /// </summary>
    public List<InterfaceProduccionDetalleDto> Detalle { get; set; } = new();
}

/// <summary>
/// DTO para el detalle del estado de proceso de cada registro.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class InterfaceProduccionDetalleDto
{
    public string CodigoSede { get; set; } = string.Empty;
    public string CodigoEntidad { get; set; } = string.Empty;
    public string CodigoProduccion { get; set; } = string.Empty;
    public string? TipoEntidadMedica { get; set; }

    /// <summary>
    /// Estado del proceso: OK (Correcto), ER (Error).
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo. Si hay error, muestra el detalle del error.
    /// </summary>
    public string? Mensaje { get; set; }
}
