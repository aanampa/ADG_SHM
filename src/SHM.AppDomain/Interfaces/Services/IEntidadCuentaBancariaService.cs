using SHM.AppDomain.DTOs.EntidadCuentaBancaria;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de cuentas bancarias de entidades en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IEntidadCuentaBancariaService
{
    /// <summary>
    /// Obtiene todas las cuentas bancarias de entidades registradas en el sistema.
    /// </summary>
    Task<IEnumerable<EntidadCuentaBancariaResponseDto>> GetAllEntidadCuentasBancariasAsync();

    /// <summary>
    /// Obtiene una cuenta bancaria por su identificador unico.
    /// </summary>
    Task<EntidadCuentaBancariaResponseDto?> GetEntidadCuentaBancariaByIdAsync(int id);

    /// <summary>
    /// Obtiene todas las cuentas bancarias asociadas a una entidad especifica.
    /// </summary>
    Task<IEnumerable<EntidadCuentaBancariaResponseDto>> GetEntidadCuentasBancariasByEntidadIdAsync(int idEntidad);

    /// <summary>
    /// Crea una nueva cuenta bancaria de entidad en el sistema.
    /// </summary>
    Task<EntidadCuentaBancariaResponseDto> CreateEntidadCuentaBancariaAsync(CreateEntidadCuentaBancariaDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de una cuenta bancaria existente.
    /// </summary>
    Task<bool> UpdateEntidadCuentaBancariaAsync(int id, UpdateEntidadCuentaBancariaDto updateDto, int idModificador);

    /// <summary>
    /// Elimina una cuenta bancaria registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteEntidadCuentaBancariaAsync(int id, int idModificador);

    /// <summary>
    /// Obtiene una cuenta bancaria por su GUID de registro.
    /// </summary>
    Task<EntidadCuentaBancariaResponseDto?> GetEntidadCuentaBancariaByGuidAsync(string guidRegistro);
}
