using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de permisos de opciones por rol.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class RolOpcionRepository : IRolOpcionRepository
{
    private readonly string _connectionString;

    public RolOpcionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las asignaciones de opciones a roles.
    /// </summary>
    public async Task<IEnumerable<RolOpcion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                ID_OPCION as IdOpcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL_OPCION
            ORDER BY ID_ROL, ID_OPCION";

        return await connection.QueryAsync<RolOpcion>(sql);
    }

    /// <summary>
    /// Obtiene una asignacion de opcion a rol por sus identificadores.
    /// </summary>
    public async Task<RolOpcion?> GetByIdAsync(int idRol, int idOpcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                ID_OPCION as IdOpcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL_OPCION
            WHERE ID_ROL = :IdRol AND ID_OPCION = :IdOpcion";

        return await connection.QueryFirstOrDefaultAsync<RolOpcion>(sql, new { IdRol = idRol, IdOpcion = idOpcion });
    }

    /// <summary>
    /// Obtiene las opciones asignadas a un rol especifico.
    /// </summary>
    public async Task<IEnumerable<RolOpcion>> GetByRolAsync(int idRol)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                ID_OPCION as IdOpcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL_OPCION
            WHERE ID_ROL = :IdRol
            ORDER BY ID_OPCION";

        return await connection.QueryAsync<RolOpcion>(sql, new { IdRol = idRol });
    }

    /// <summary>
    /// Obtiene los roles que tienen asignada una opcion especifica.
    /// </summary>
    public async Task<IEnumerable<RolOpcion>> GetByOpcionAsync(int idOpcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ROL as IdRol,
                ID_OPCION as IdOpcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_ROL_OPCION
            WHERE ID_OPCION = :IdOpcion
            ORDER BY ID_ROL";

        return await connection.QueryAsync<RolOpcion>(sql, new { IdOpcion = idOpcion });
    }

    /// <summary>
    /// Crea una nueva asignacion de opcion a un rol.
    /// </summary>
    public async Task<bool> CreateAsync(RolOpcion rolOpcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_SEG_ROL_OPCION (
                ID_ROL,
                ID_OPCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                :IdRol,
                :IdOpcion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            rolOpcion.IdRol,
            rolOpcion.IdOpcion,
            rolOpcion.IdCreador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Actualiza una asignacion de opcion a rol existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int idRol, int idOpcion, RolOpcion rolOpcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_ROL_OPCION
            SET
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ROL = :IdRol AND ID_OPCION = :IdOpcion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdRol = idRol,
            IdOpcion = idOpcion,
            rolOpcion.Activo,
            rolOpcion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una asignacion de opcion a rol.
    /// </summary>
    public async Task<bool> DeleteAsync(int idRol, int idOpcion, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_ROL_OPCION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ROL = :IdRol AND ID_OPCION = :IdOpcion";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdRol = idRol, IdOpcion = idOpcion, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una asignacion de opcion a rol.
    /// </summary>
    public async Task<bool> ExistsAsync(int idRol, int idOpcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_SEG_ROL_OPCION WHERE ID_ROL = :IdRol AND ID_OPCION = :IdOpcion";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { IdRol = idRol, IdOpcion = idOpcion });

        return count > 0;
    }
}
