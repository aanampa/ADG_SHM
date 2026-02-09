using SHM.AppDomain.DTOs.OrdenPagoAprobacion;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la entidad OrdenPagoAprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public interface IOrdenPagoAprobacionService
{
    Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetAllAsync();
    Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetAllActiveAsync();
    Task<OrdenPagoAprobacionResponseDto?> GetByIdAsync(int id);
    Task<OrdenPagoAprobacionResponseDto?> GetByGuidAsync(string guid);
    Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetByOrdenPagoIdAsync(int idOrdenPago);
    Task<IEnumerable<OrdenPagoAprobacionResponseDto>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion);
    Task<(bool success, string message)> AprobarAsync(int idOrdenPago, int idUsuario);
    Task<(bool success, string message)> RechazarAsync(int idOrdenPago, int idUsuario, string? comentario);
    Task<OrdenPagoAprobacionResponseDto> CreateAsync(CreateOrdenPagoAprobacionDto dto, int idCreador);
    Task<OrdenPagoAprobacionResponseDto?> UpdateAsync(UpdateOrdenPagoAprobacionDto dto, int idModificador);
    Task<bool> DeleteAsync(string guid, int idModificador);
    Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador);
}
