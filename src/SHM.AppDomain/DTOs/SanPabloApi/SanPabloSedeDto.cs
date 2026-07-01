namespace SHM.AppDomain.DTOs.SanPabloApi;

/// <summary>
/// DTO para la respuesta de sede del API de San Pablo.
/// Mapea los campos que devuelve el endpoint ObtenerSede.
///
/// Mapeo de campos API San Pablo -> SHM:
/// - CODIGO -> CODIGO (Codigo)
/// - DESCRIPCION -> NOMBRE (Nombre)
///
/// <author>ADG Antonio</author>
/// <created>2026-02-24</created>
/// </summary>
public class SanPabloSedeDto
{
    public string? CODIGO { get; set; }
    public string? DESCRIPCION { get; set; }
    public string? RUC { get; set; }
}

/// <summary>
/// DTO para la respuesta del endpoint ObtenerSede del API de San Pablo.
/// La respuesta contiene IsSuccess, Title, Message y Data como array de sedes.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-24</created>
/// </summary>
public class SanPabloSedeResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public List<SanPabloSedeDto>? Data { get; set; }
}
