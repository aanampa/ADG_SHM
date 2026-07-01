namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para la respuesta de entidad medica del API de San Pablo.
/// Mapea los campos que devuelve el endpoint ListarObtenerEntidad.
///
/// Mapeo de campos API San Pablo -> SHM:
/// - CODIGO -> CODIGO_ENTIDAD (CodigoEntidad)
/// - CODIGO_TIPOENTIDAD -> TIPO_ENTIDAD_MEDICA (TipoEntidadMedica)
/// - NOMBRE -> RAZON_SOCIAL (RazonSocial)
/// - RUC -> RUC (Ruc)
/// - DIRECCION -> DIRECCION (Direccion)
/// - CODIGO_SAP -> CODIGO_ACREEDOR (CodigoAcreedor)
/// - CODIGO_CORRENTISTA -> CODIGO_CORRIENTISTA (CodigoCorrientista)
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// <modified>ADG Antonio - 2026-02-02 - Actualizado segun nueva estructura del API</modified>
/// </summary>
public class SanPabloEntidadMedicaDto
{
    public string? CODIGO { get; set; }
    public string? CODIGO_TIPOENTIDAD { get; set; }
    public string? TIPO_ENTIDAD { get; set; }
    public string? NOMBRE { get; set; }
    public string? RUC { get; set; }
    public string? DIRECCION { get; set; }
    public string? CODIGO_SAP { get; set; }
    public string? CODIGO_CORRENTISTA { get; set; }
}

/// <summary>
/// DTO para la respuesta del endpoint ListarObtenerEntidad del API de San Pablo.
/// La respuesta contiene IsSuccess, Title, Message y Data como array de entidades.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// <modified>ADG Antonio - 2026-02-02 - Data es ahora una lista de entidades</modified>
/// </summary>
public class SanPabloEntidadMedicaResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public List<SanPabloEntidadMedicaDto>? Data { get; set; }
}
