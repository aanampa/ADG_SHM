using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de parametros de configuracion del sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ParametroRepository : IParametroRepository
{
    private readonly string _connectionString;

    public ParametroRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los parametros de configuracion del sistema.
    /// </summary>
    public async Task<IEnumerable<Parametro>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_PARAMETRO as IdParametro,
                CODIGO as Codigo,
                VALOR as Valor,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_PARAMETRO
            ORDER BY ID_PARAMETRO";

        return await connection.QueryAsync<Parametro>(sql);
    }

    /// <summary>
    /// Obtiene un parametro por su identificador unico.
    /// </summary>
    public async Task<Parametro?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_PARAMETRO as IdParametro,
                CODIGO as Codigo,
                VALOR as Valor,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_PARAMETRO
            WHERE ID_PARAMETRO = :Id";

        return await connection.QueryFirstOrDefaultAsync<Parametro>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un parametro por su identificador GUID.
    /// </summary>
    public async Task<Parametro?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_PARAMETRO as IdParametro,
                CODIGO as Codigo,
                VALOR as Valor,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_PARAMETRO
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Parametro>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene un parametro por su codigo.
    /// </summary>
    public async Task<Parametro?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_PARAMETRO as IdParametro,
                CODIGO as Codigo,
                VALOR as Valor,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_PARAMETRO
            WHERE CODIGO = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<Parametro>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Crea un nuevo parametro de configuracion en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Parametro parametro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_PARAMETRO (
                ID_PARAMETRO,
                CODIGO,
                VALOR,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_PARAMETRO_SEQ.NEXTVAL,
                :Codigo,
                :Valor,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_PARAMETRO INTO :IdParametro";

        var parameters = new DynamicParameters();
        parameters.Add("Codigo", parametro.Codigo);
        parameters.Add("Valor", parametro.Valor);
        parameters.Add("IdCreador", parametro.IdCreador);
        parameters.Add("IdParametro", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdParametro");
    }

    /// <summary>
    /// Actualiza los datos de un parametro de configuracion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Parametro parametro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PARAMETRO
            SET
                CODIGO = :Codigo,
                VALOR = :Valor,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_PARAMETRO = :IdParametro";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdParametro = id,
            parametro.Codigo,
            parametro.Valor,
            parametro.Activo,
            parametro.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un parametro de configuracion del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PARAMETRO
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_PARAMETRO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un parametro con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_PARAMETRO WHERE ID_PARAMETRO = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
