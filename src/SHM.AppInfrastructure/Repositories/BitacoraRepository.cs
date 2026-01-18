using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de registros de auditoria del sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class BitacoraRepository : IBitacoraRepository
{
    private readonly string _connectionString;

    public BitacoraRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los registros de auditoria.
    /// </summary>
    public async Task<IEnumerable<Bitacora>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BITACORA as IdBitacora,
                ENTIDAD as Entidad,
                ACCION as Accion,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BITACORA
            ORDER BY ID_BITACORA DESC";

        return await connection.QueryAsync<Bitacora>(sql);
    }

    /// <summary>
    /// Obtiene un registro de auditoria por su identificador unico.
    /// </summary>
    public async Task<Bitacora?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BITACORA as IdBitacora,
                ENTIDAD as Entidad,
                ACCION as Accion,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BITACORA
            WHERE ID_BITACORA = :Id";

        return await connection.QueryFirstOrDefaultAsync<Bitacora>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene los registros de auditoria de una entidad especifica.
    /// </summary>
    public async Task<IEnumerable<Bitacora>> GetByEntidadAsync(string entidad)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_BITACORA as IdBitacora,
                ENTIDAD as Entidad,
                ACCION as Accion,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_BITACORA
            WHERE ENTIDAD = :Entidad
            ORDER BY ID_BITACORA DESC";

        return await connection.QueryAsync<Bitacora>(sql, new { Entidad = entidad });
    }

    /// <summary>
    /// Crea un nuevo registro de auditoria en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Bitacora bitacora)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_BITACORA (
                ID_BITACORA,
                ENTIDAD,
                ACCION,
                DESCRIPCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_BITACORA_SEQ.NEXTVAL,
                :Entidad,
                :Accion,
                :Descripcion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_BITACORA INTO :IdBitacora";

        var parameters = new DynamicParameters();
        parameters.Add("Entidad", bitacora.Entidad);
        parameters.Add("Accion", bitacora.Accion);
        parameters.Add("Descripcion", bitacora.Descripcion);
        parameters.Add("IdCreador", bitacora.IdCreador);
        parameters.Add("IdBitacora", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdBitacora");
    }

    /// <summary>
    /// Actualiza los datos de un registro de auditoria existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Bitacora bitacora)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_BITACORA
            SET
                ENTIDAD = :Entidad,
                ACCION = :Accion,
                DESCRIPCION = :Descripcion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_BITACORA = :IdBitacora";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdBitacora = id,
            bitacora.Entidad,
            bitacora.Accion,
            bitacora.Descripcion,
            bitacora.Activo,
            bitacora.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un registro de auditoria del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_BITACORA
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_BITACORA = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un registro de auditoria con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_BITACORA WHERE ID_BITACORA = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
