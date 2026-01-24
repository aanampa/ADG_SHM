using SHM.AppDomain.DTOs.Parametro;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de parametros del sistema en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IParametroService
{
    /// <summary>
    /// Obtiene todos los parametros registrados en el sistema.
    /// </summary>
    Task<IEnumerable<ParametroResponseDto>> GetAllParametrosAsync();

    /// <summary>
    /// Obtiene un parametro por su identificador unico.
    /// </summary>
    Task<ParametroResponseDto?> GetParametroByIdAsync(int id);

    /// <summary>
    /// Obtiene un parametro por su GUID de registro.
    /// </summary>
    Task<ParametroResponseDto?> GetParametroByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene un parametro por su codigo.
    /// </summary>
    Task<ParametroResponseDto?> GetParametroByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene el valor de un parametro por su codigo.
    /// </summary>
    /// <param name="codigo">Codigo del parametro</param>
    /// <returns>Valor del parametro o null si no existe</returns>
    /// <author>ADG Antonio</author>
    /// <created>2026-01-23</created>
    Task<string?> GetValorByCodigoAsync(string codigo);

    /// <summary>
    /// Crea un nuevo parametro en el sistema.
    /// </summary>
    Task<ParametroResponseDto> CreateParametroAsync(CreateParametroDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un parametro existente.
    /// </summary>
    Task<bool> UpdateParametroAsync(int id, UpdateParametroDto updateDto, int idModificador);

    /// <summary>
    /// Elimina un parametro registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteParametroAsync(int id, int idModificador);
}
