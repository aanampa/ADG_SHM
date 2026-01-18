using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de bancos del sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class BancoRepository : IBancoRepository
{
    private readonly string _connectionString;

    public BancoRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los bancos registrados en el sistema.
    /// </summary>
    public async Task<IEnumerable<Banco>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BANCO as IdBanco,
                CODIGO_BANCO as CodigoBanco,
                NOMBRE_BANCO as NombreBanco,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BANCO
            ORDER BY ID_BANCO";

        return await connection.QueryAsync<Banco>(sql);
    }

    /// <summary>
    /// Obtiene un banco por su identificador unico.
    /// </summary>
    public async Task<Banco?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BANCO as IdBanco,
                CODIGO_BANCO as CodigoBanco,
                NOMBRE_BANCO as NombreBanco,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BANCO
            WHERE ID_BANCO = :Id";

        return await connection.QueryFirstOrDefaultAsync<Banco>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un banco por su codigo.
    /// </summary>
    public async Task<Banco?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BANCO as IdBanco,
                CODIGO_BANCO as CodigoBanco,
                NOMBRE_BANCO as NombreBanco,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BANCO
            WHERE CODIGO_BANCO = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<Banco>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Crea un nuevo banco en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Banco banco)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_BANCO (
                ID_BANCO,
                CODIGO_BANCO,
                NOMBRE_BANCO,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_BANCO_SEQ.NEXTVAL,
                :CodigoBanco,
                :NombreBanco,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_BANCO INTO :IdBanco";

        var parameters = new DynamicParameters();
        parameters.Add("CodigoBanco", banco.CodigoBanco);
        parameters.Add("NombreBanco", banco.NombreBanco);
        parameters.Add("IdCreador", banco.IdCreador);
        parameters.Add("IdBanco", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdBanco");
    }

    /// <summary>
    /// Actualiza los datos de un banco existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Banco banco)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_BANCO
            SET
                CODIGO_BANCO = :CodigoBanco,
                NOMBRE_BANCO = :NombreBanco,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_BANCO = :IdBanco";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdBanco = id,
            banco.CodigoBanco,
            banco.NombreBanco,
            banco.Activo,
            banco.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un banco del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_BANCO
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_BANCO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un banco con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_BANCO WHERE ID_BANCO = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

    /// <summary>
    /// Obtiene un banco por su identificador GUID.
    /// </summary>
    public async Task<Banco?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BANCO as IdBanco,
                CODIGO_BANCO as CodigoBanco,
                NOMBRE_BANCO as NombreBanco,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BANCO
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Banco>(sql, new { GuidRegistro = guidRegistro });
    }
}
