namespace SHM.AppDomain.DTOs.SapApi;

/// <summary>
/// Resultado de regenerar la trama de liberacion de pagos de una orden, sin enviarla a SAP.
/// Si Valida es false, Errores indica que datos faltan o son invalidos.
///
/// <author>ADG Antonio</author>
/// <created>2026-11-14</created>
/// </summary>
public class LiberacionTramaDto
{
    public string? NumeroOrden { get; set; }
    public string? EstadoOrden { get; set; }
    public bool Valida { get; set; }
    public List<string> Errores { get; set; } = new();

    /// <summary>Request tal como se enviaria a SAP. Nulo si no es valida.</summary>
    public SapLiberaPagoRequestDto? Request { get; set; }
}
