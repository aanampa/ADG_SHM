using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de tablas maestras del sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaRepository : ITablaRepository
{
    private readonly string _connectionString;

    public TablaRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las tablas maestras registradas en el sistema.
    /// </summary>
    public async Task<IEnumerable<Tabla>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA
            ORDER BY ID_TABLA";

        return await connection.QueryAsync<Tabla>(sql);
    }

    /// <summary>
    /// Obtiene una tabla maestra por su identificador unico.
    /// </summary>
    public async Task<Tabla?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA
            WHERE ID_TABLA = :Id";

        return await connection.QueryFirstOrDefaultAsync<Tabla>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una tabla maestra por su identificador GUID.
    /// </summary>
    public async Task<Tabla?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Tabla>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene una tabla maestra por su codigo.
    /// </summary>
    public async Task<Tabla?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA
            WHERE CODIGO = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<Tabla>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Crea una nueva tabla maestra en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Tabla tabla)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_TABLA (
                ID_TABLA,
                CODIGO,
                DESCRIPCION,
                ACTIVO,
                GUID_REGISTRO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SGH_SEG_TABLA_SEQ.NEXTVAL,
                :Codigo,
                :Descripcion,
                1,
                SYS_GUID(),
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_TABLA INTO :IdTabla";

        var parameters = new DynamicParameters();
        parameters.Add("Codigo", tabla.Codigo);
        parameters.Add("Descripcion", tabla.Descripcion);
        parameters.Add("IdCreador", tabla.IdCreador);
        parameters.Add("IdTabla", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdTabla");
    }

    /// <summary>
    /// Actualiza los datos de una tabla maestra existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Tabla tabla)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_TABLA
            SET
                CODIGO = :Codigo,
                DESCRIPCION = :Descripcion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_TABLA = :IdTabla";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdTabla = id,
            tabla.Codigo,
            tabla.Descripcion,
            tabla.Activo,
            tabla.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una tabla maestra del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_TABLA
            SET ACTIVO = 0,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_TABLA = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una tabla maestra con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_TABLA WHERE ID_TABLA = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
