namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para el request de registro de comprobante en el API de San Pablo.
/// Mapea los campos que espera el endpoint RegistrarComprobante.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-25</created>
/// </summary>
public class SanPabloComprobanteRequestDto
{
    public string? COD_SEDE { get; set; }
    public string? FLG_CIAMEDICA { get; set; }
    public string? COD_ENTIDAD { get; set; }
    public string? COD_PROD { get; set; }
    public string? FLG_PORTAL { get; set; }
    public string? CPM_TIPO { get; set; }
    public string? CPM_SERIE { get; set; }
    public string? CPM_NUMERO { get; set; }
    public string? CPM_FECEMI { get; set; }
    public string? CPM_GLOSA { get; set; }
    public string? CPM_MTOTAL { get; set; }
    public string? CPM_FECREG { get; set; }
}

/// <summary>
/// DTO para la respuesta del endpoint RegistrarComprobante del API de San Pablo.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-25</created>
/// </summary>
public class SanPabloComprobanteResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}
