using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de roles del sistema de seguridad.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolRepository : IRolRepository
{
    private readonly string _connectionString;

    public RolRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los roles registrados en el sistema.
    /// </summary>
    public async Task<IEnumerable<Rol>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL
            ORDER BY ID_ROL";

        return await connection.QueryAsync<Rol>(sql);
    }

    /// <summary>
    /// Obtiene un rol por su identificador unico.
    /// </summary>
    public async Task<Rol?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL
            WHERE ID_ROL = :Id";

        return await connection.QueryFirstOrDefaultAsync<Rol>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un rol por su identificador GUID.
    /// </summary>
    public async Task<Rol?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Rol>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene un rol por su codigo.
    /// </summary>
    public async Task<Rol?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL
            WHERE CODIGO = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<Rol>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Crea un nuevo rol en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Rol rol)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_SEG_ROL (
                ID_ROL,
                CODIGO,
                DESCRIPCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_SEG_ROL_SEQ.NEXTVAL,
                :Codigo,
                :Descripcion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ROL INTO :IdRol";

        var parameters = new DynamicParameters();
        parameters.Add("Codigo", rol.Codigo);
        parameters.Add("Descripcion", rol.Descripcion);
        parameters.Add("IdCreador", rol.IdCreador);
        parameters.Add("IdRol", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdRol");
    }

    /// <summary>
    /// Actualiza los datos de un rol existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Rol rol)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_ROL
            SET
                CODIGO = :Codigo,
                DESCRIPCION = :Descripcion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ROL = :IdRol";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdRol = id,
            rol.Codigo,
            rol.Descripcion,
            rol.Activo,
            rol.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un rol del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_ROL
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ROL = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un rol con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_SEG_ROL WHERE ID_ROL = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
