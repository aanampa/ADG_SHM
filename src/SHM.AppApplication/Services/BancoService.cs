using SHM.AppDomain.DTOs.Banco;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de bancos del sistema, incluyendo operaciones CRUD y consultas por codigo
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class BancoService : IBancoService
{
    private readonly IBancoRepository _bancoRepository;

    public BancoService(IBancoRepository bancoRepository)
    {
        _bancoRepository = bancoRepository;
    }

    /// <summary>
    /// Obtiene todos los bancos del sistema
    /// </summary>
    public async Task<IEnumerable<BancoResponseDto>> GetAllBancosAsync()
    {
        var bancos = await _bancoRepository.GetAllAsync();
        return bancos.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un banco por su identificador unico
    /// </summary>
    public async Task<BancoResponseDto?> GetBancoByIdAsync(int id)
    {
        var banco = await _bancoRepository.GetByIdAsync(id);
        return banco != null ? MapToResponseDto(banco) : null;
    }

    /// <summary>
    /// Obtiene un banco por su codigo
    /// </summary>
    public async Task<BancoResponseDto?> GetBancoByCodigoAsync(string codigo)
    {
        var banco = await _bancoRepository.GetByCodigoAsync(codigo);
        return banco != null ? MapToResponseDto(banco) : null;
    }

    /// <summary>
    /// Obtiene un banco por su GUID de registro
    /// </summary>
    public async Task<BancoResponseDto?> GetBancoByGuidAsync(string guidRegistro)
    {
        var banco = await _bancoRepository.GetByGuidAsync(guidRegistro);
        return banco != null ? MapToResponseDto(banco) : null;
    }

    /// <summary>
    /// Crea un nuevo banco en el sistema
    /// </summary>
    public async Task<BancoResponseDto> CreateBancoAsync(CreateBancoDto createDto, int idCreador)
    {
        var banco = new Banco
        {
            CodigoBanco = createDto.CodigoBanco,
            NombreBanco = createDto.NombreBanco,
            IdCreador = idCreador,
            Activo = 1
        };

        var idBanco = await _bancoRepository.CreateAsync(banco);
        var createdBanco = await _bancoRepository.GetByIdAsync(idBanco);

        return MapToResponseDto(createdBanco!);
    }

    /// <summary>
    /// Actualiza los datos de un banco existente
    /// </summary>
    public async Task<bool> UpdateBancoAsync(int id, UpdateBancoDto updateDto, int idModificador)
    {
        var bancoExistente = await _bancoRepository.GetByIdAsync(id);
        if (bancoExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.CodigoBanco))
            bancoExistente.CodigoBanco = updateDto.CodigoBanco;

        if (!string.IsNullOrEmpty(updateDto.NombreBanco))
            bancoExistente.NombreBanco = updateDto.NombreBanco;

        if (updateDto.Activo.HasValue)
            bancoExistente.Activo = updateDto.Activo.Value;

        bancoExistente.IdModificador = idModificador;

        return await _bancoRepository.UpdateAsync(id, bancoExistente);
    }

    /// <summary>
    /// Elimina un banco por su identificador
    /// </summary>
    public async Task<bool> DeleteBancoAsync(int id, int idModificador)
    {
        var exists = await _bancoRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _bancoRepository.DeleteAsync(id, idModificador);
    }

    private static BancoResponseDto MapToResponseDto(Banco banco)
    {
        return new BancoResponseDto
        {
            IdBanco = banco.IdBanco,
            CodigoBanco = banco.CodigoBanco,
            NombreBanco = banco.NombreBanco,
            Activo = banco.Activo,
            GuidRegistro = banco.GuidRegistro,
            FechaCreacion = banco.FechaCreacion,
            FechaModificacion = banco.FechaModificacion
        };
    }
}
