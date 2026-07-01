namespace SHM.AppWebCompaniaMedica.Models;

/// <summary>
/// ViewModel para el Dashboard del portal de companias medicas.
/// Contiene estadisticas de producciones por entidad medica.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-24</created>
/// </summary>
public class DashboardViewModel
{
    // Estadisticas principales (cards superiores)
    public decimal TotalPorFacturar { get; set; }
    public int FacturasPendientes { get; set; }
    public int FacturasEnviadasMes { get; set; }

    // Resumen del mes
    public decimal TotalFacturadoMes { get; set; }
    public int FacturasProcesadasMes { get; set; }
    public decimal TiempoPromedioDias { get; set; }

    // Conteo por estados para grafico de dona
    public int CantidadPendienteFactura { get; set; }
    public int CantidadFacturaEnviada { get; set; }
    public int CantidadFacturaEnviadaHHMM { get; set; }
    public int CantidadFacturaPagada { get; set; }

    // Datos para grafico de barras (ultimos 6 meses)
    public List<FacturasPorMesViewModel> FacturasPorMes { get; set; } = new();
}

/// <summary>
/// ViewModel para la pagina de Configuracion del usuario.
/// </summary>
public class ConfiguracionViewModel
{
    public int FacturasTotal { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

/// <summary>
/// ViewModel para datos de facturas por mes (grafico de barras).
/// </summary>
public class FacturasPorMesViewModel
{
    public string? Mes { get; set; }
    public int Anio { get; set; }
    public int FacturasEnviadas { get; set; }
    public int FacturasPendientes { get; set; }
}
