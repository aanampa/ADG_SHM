namespace SHM.AppDomain.DTOs.Produccion;

/// <summary>
/// DTO para el resultado de la creacion masiva de producciones a traves de interface.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// </summary>
public class InterfaceProduccionResultDto
{
    /// <summary>
    /// Cantidad de registros creados exitosamente.
    /// </summary>
    public int CantidadCreados { get; set; }

    /// <summary>
    /// Cantidad de registros obviados por ya existir en el sistema.
    /// </summary>
    public int CantidadObviados { get; set; }

    /// <summary>
    /// Total de registros procesados.
    /// </summary>
    public int TotalProcesados => CantidadCreados + CantidadObviados;
}
