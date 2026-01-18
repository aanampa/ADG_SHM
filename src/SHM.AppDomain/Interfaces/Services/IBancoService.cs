using SHM.AppDomain.DTOs.Banco;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de bancos en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IBancoService
{
    /// <summary>
    /// Obtiene todos los bancos registrados en el sistema.
    /// </summary>
    Task<IEnumerable<BancoResponseDto>> GetAllBancosAsync();

    /// <summary>
    /// Obtiene un banco por su identificador unico.
    /// </summary>
    Task<BancoResponseDto?> GetBancoByIdAsync(int id);

    /// <summary>
    /// Obtiene un banco por su codigo.
    /// </summary>
    Task<BancoResponseDto?> GetBancoByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene un banco por su GUID de registro.
    /// </summary>
    Task<BancoResponseDto?> GetBancoByGuidAsync(string guidRegistro);

    /// <summary>
    /// Crea un nuevo banco en el sistema.
    /// </summary>
    Task<BancoResponseDto> CreateBancoAsync(CreateBancoDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un banco existente.
    /// </summary>
    Task<bool> UpdateBancoAsync(int id, UpdateBancoDto updateDto, int idModificador);

    /// <summary>
    /// Elimina un banco registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteBancoAsync(int id, int idModificador);
}
