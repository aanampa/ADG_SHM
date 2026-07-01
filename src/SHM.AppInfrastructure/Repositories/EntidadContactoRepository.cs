using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de contactos de notificacion de entidades medicas.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-05-16</created>
/// </summary>
public class EntidadContactoRepository : IEntidadContactoRepository
{
    private readonly string _connectionString;

    public EntidadContactoRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    private const string SelectColumns = @"
        ID_ENTIDAD_CONTACTO AS IdEntidadContacto,
        ID_ENTIDAD_MEDICA   AS IdEntidadMedica,
        APELLIDO_PATERNO    AS ApellidoPaterno,
        APELLIDO_MATERNO    AS ApellidoMaterno,
        NOMBRES             AS Nombres,
        CELULAR             AS Celular,
        EMAIL               AS Email,
        CARGO               AS Cargo,
        GUID_REGISTRO       AS GuidRegistro,
        ACTIVO              AS Activo,
        ID_CREADOR          AS IdCreador,
        FECHA_CREACION      AS FechaCreacion,
        ID_MODIFICADOR      AS IdModificador,
        FECHA_MODIFICACION  AS FechaModificacion";

    public async Task<IEnumerable<EntidadContacto>> GetByEntidadMedicaAsync(int idEntidadMedica, bool soloActivos = true)
    {
        using var connection = new OracleConnection(_connectionString);

        var where = soloActivos
            ? "WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica AND ACTIVO = 1"
            : "WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica";

        var sql = $@"
            SELECT {SelectColumns}
            FROM SHM_ENTIDAD_CONTACTO
            {where}
            ORDER BY ACTIVO DESC, ID_ENTIDAD_CONTACTO";

        return await connection.QueryAsync<EntidadContacto>(sql, new { IdEntidadMedica = idEntidadMedica });
    }

    public async Task<EntidadContacto?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"
            SELECT {SelectColumns}
            FROM SHM_ENTIDAD_CONTACTO
            WHERE ID_ENTIDAD_CONTACTO = :Id";

        return await connection.QueryFirstOrDefaultAsync<EntidadContacto>(sql, new { Id = id });
    }

    public async Task<EntidadContacto?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"
            SELECT {SelectColumns}
            FROM SHM_ENTIDAD_CONTACTO
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<EntidadContacto>(sql, new { GuidRegistro = guidRegistro });
    }

    public async Task<int> CreateAsync(EntidadContacto contacto)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ENTIDAD_CONTACTO (
                ID_ENTIDAD_CONTACTO,
                ID_ENTIDAD_MEDICA,
                APELLIDO_PATERNO,
                APELLIDO_MATERNO,
                NOMBRES,
                CELULAR,
                EMAIL,
                CARGO,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ENTIDAD_CONTACTO_SEQ.NEXTVAL,
                :IdEntidadMedica,
                :ApellidoPaterno,
                :ApellidoMaterno,
                :Nombres,
                :Celular,
                :Email,
                :Cargo,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ENTIDAD_CONTACTO INTO :IdEntidadContacto";

        var parameters = new DynamicParameters();
        parameters.Add("IdEntidadMedica", contacto.IdEntidadMedica);
        parameters.Add("ApellidoPaterno", contacto.ApellidoPaterno);
        parameters.Add("ApellidoMaterno", contacto.ApellidoMaterno);
        parameters.Add("Nombres",         contacto.Nombres);
        parameters.Add("Celular",         contacto.Celular);
        parameters.Add("Email",           contacto.Email);
        parameters.Add("Cargo",           contacto.Cargo);
        parameters.Add("IdCreador",       contacto.IdCreador);
        parameters.Add("IdEntidadContacto", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdEntidadContacto");
    }

    public async Task<bool> UpdateAsync(int id, EntidadContacto contacto)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ENTIDAD_CONTACTO
            SET
                APELLIDO_PATERNO   = :ApellidoPaterno,
                APELLIDO_MATERNO   = :ApellidoMaterno,
                NOMBRES            = :Nombres,
                CELULAR            = :Celular,
                EMAIL              = :Email,
                CARGO              = :Cargo,
                ACTIVO             = :Activo,
                ID_MODIFICADOR     = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ENTIDAD_CONTACTO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id               = id,
            contacto.ApellidoPaterno,
            contacto.ApellidoMaterno,
            contacto.Nombres,
            contacto.Celular,
            contacto.Email,
            contacto.Cargo,
            contacto.Activo,
            contacto.IdModificador
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ENTIDAD_CONTACTO
            SET ACTIVO             = 0,
                ID_MODIFICADOR     = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ENTIDAD_CONTACTO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_ENTIDAD_CONTACTO WHERE ID_ENTIDAD_CONTACTO = :Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

    public async Task<bool> ExisteEmailEnEntidadAsync(int idEntidadMedica, string email, int? excludeId = null)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT COUNT(1) FROM SHM_ENTIDAD_CONTACTO
            WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica
              AND UPPER(EMAIL) = UPPER(:Email)
              AND ACTIVO = 1";

        if (excludeId.HasValue)
            sql += " AND ID_ENTIDAD_CONTACTO <> :ExcludeId";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { IdEntidadMedica = idEntidadMedica, Email = email, ExcludeId = excludeId });

        return count > 0;
    }
}
