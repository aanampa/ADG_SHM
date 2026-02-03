namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para la respuesta de entidad medica del API de San Pablo.
/// Mapea los campos que devuelve el endpoint ListarObtenerEntidad.
///
/// Mapeo de campos API San Pablo -> SHM:
/// - Tipo_Entidad -> TIPO_ENTIDAD_MEDICA
/// - Nombre -> RAZON_SOCIAL
/// - Ruc -> RUC
/// - Direccion -> DIRECCION
/// - Codigo_SAP -> CODIGO_ACREEDOR
/// - Codigo_Correntista -> CODIGO_CORRIENTISTA
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// <modified>ADG Antonio - 2026-02-02 - Agregado CodigoSap y CodigoCorrientista</modified>
/// </summary>
public class SanPabloEntidadMedicaDto
{
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Ruc { get; set; }
    public string? Tipo_Entidad { get; set; }
    public string? Direccion { get; set; }
    public string? Codigo_SAP { get; set; }
    public string? Codigo_Correntista { get; set; }
}

/// <summary>
/// DTO para la respuesta del endpoint ListarObtenerEntidad del API de San Pablo.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public class SanPabloEntidadMedicaResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public SanPabloEntidadMedicaDto? Data { get; set; }
}
