using SHM.AppDomain.DTOs.Parametro;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de parametros de configuracion del sistema, incluyendo operaciones CRUD y consultas por codigo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ParametroService : IParametroService
{
    private readonly IParametroRepository _parametroRepository;

    public ParametroService(IParametroRepository parametroRepository)
    {
        _parametroRepository = parametroRepository;
    }

    /// <summary>
    /// Obtiene todos los parametros de configuracion del sistema
    /// </summary>
    public async Task<IEnumerable<ParametroResponseDto>> GetAllParametrosAsync()
    {
        var parametros = await _parametroRepository.GetAllAsync();
        return parametros.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un parametro por su identificador unico
    /// </summary>
    public async Task<ParametroResponseDto?> GetParametroByIdAsync(int id)
    {
        var parametro = await _parametroRepository.GetByIdAsync(id);
        return parametro != null ? MapToResponseDto(parametro) : null;
    }

    /// <summary>
    /// Obtiene un parametro por su GUID de registro
    /// </summary>
    public async Task<ParametroResponseDto?> GetParametroByGuidAsync(string guidRegistro)
    {
        var parametro = await _parametroRepository.GetByGuidAsync(guidRegistro);
        return parametro != null ? MapToResponseDto(parametro) : null;
    }

    /// <summary>
    /// Obtiene un parametro por su codigo
    /// </summary>
    public async Task<ParametroResponseDto?> GetParametroByCodigoAsync(string codigo)
    {
        var parametro = await _parametroRepository.GetByCodigoAsync(codigo);
        return parametro != null ? MapToResponseDto(parametro) : null;
    }

    /// <summary>
    /// Crea un nuevo parametro de configuracion en el sistema
    /// </summary>
    public async Task<ParametroResponseDto> CreateParametroAsync(CreateParametroDto createDto, int idCreador)
    {
        var parametro = new Parametro
        {
            Codigo = createDto.Codigo,
            Valor = createDto.Valor,
            IdCreador = idCreador,
            Activo = 1
        };

        var idParametro = await _parametroRepository.CreateAsync(parametro);
        var createdParametro = await _parametroRepository.GetByIdAsync(idParametro);

        return MapToResponseDto(createdParametro!);
    }

    /// <summary>
    /// Actualiza un parametro existente
    /// </summary>
    public async Task<bool> UpdateParametroAsync(int id, UpdateParametroDto updateDto, int idModificador)
    {
        var parametroExistente = await _parametroRepository.GetByIdAsync(id);
        if (parametroExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.Codigo))
            parametroExistente.Codigo = updateDto.Codigo;

        if (updateDto.Valor != null)
            parametroExistente.Valor = updateDto.Valor;

        if (updateDto.Activo.HasValue)
            parametroExistente.Activo = updateDto.Activo.Value;

        parametroExistente.IdModificador = idModificador;

        return await _parametroRepository.UpdateAsync(id, parametroExistente);
    }

    /// <summary>
    /// Elimina un parametro por su identificador
    /// </summary>
    public async Task<bool> DeleteParametroAsync(int id, int idModificador)
    {
        var exists = await _parametroRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _parametroRepository.DeleteAsync(id, idModificador);
    }

    private static ParametroResponseDto MapToResponseDto(Parametro parametro)
    {
        return new ParametroResponseDto
        {
            IdParametro = parametro.IdParametro,
            Codigo = parametro.Codigo,
            Valor = parametro.Valor,
            Activo = parametro.Activo,
            GuidRegistro = parametro.GuidRegistro,
            FechaCreacion = parametro.FechaCreacion,
            FechaModificacion = parametro.FechaModificacion
        };
    }
}
