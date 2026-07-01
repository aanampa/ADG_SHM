using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.DTOs.Bitacora;
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
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos IdEntidad y FechaAccion</modified>
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
                ID_ENTIDAD as IdEntidad,
                ENTIDAD as Entidad,
                ACCION as Accion,
                DESCRIPCION as Descripcion,
                FECHA_ACCION as FechaAccion,
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
                ID_ENTIDAD as IdEntidad,
                ENTIDAD as Entidad,
                ACCION as Accion,
                DESCRIPCION as Descripcion,
                FECHA_ACCION as FechaAccion,
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
                ID_ENTIDAD as IdEntidad,
                ENTIDAD as Entidad,
                ACCION as Accion,
                DESCRIPCION as Descripcion,
                FECHA_ACCION as FechaAccion,
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
                ID_ENTIDAD,
                ENTIDAD,
                ACCION,
                DESCRIPCION,
                FECHA_ACCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_BITACORA_SEQ.NEXTVAL,
                :IdEntidad,
                :Entidad,
                :Accion,
                :Descripcion,
                :FechaAccion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_BITACORA INTO :IdBitacora";

        var parameters = new DynamicParameters();
        parameters.Add("IdEntidad", bitacora.IdEntidad);
        parameters.Add("Entidad", bitacora.Entidad);
        parameters.Add("Accion", bitacora.Accion);
        parameters.Add("Descripcion", bitacora.Descripcion);
        parameters.Add("FechaAccion", bitacora.FechaAccion);
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
                ID_ENTIDAD = :IdEntidad,
                ENTIDAD = :Entidad,
                ACCION = :Accion,
                DESCRIPCION = :Descripcion,
                FECHA_ACCION = :FechaAccion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_BITACORA = :IdBitacora";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdBitacora = id,
            bitacora.IdEntidad,
            bitacora.Entidad,
            bitacora.Accion,
            bitacora.Descripcion,
            bitacora.FechaAccion,
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

    /// <summary>
    /// Obtiene las bitacoras de una entidad especifica por su ID, incluyendo datos del usuario que realizo la accion.
    /// </summary>
    /// <param name="entidad">Nombre de la entidad (ej: SHM_PRODUCCION)</param>
    /// <param name="idEntidad">ID de la entidad</param>
    /// <returns>Lista de bitacoras con datos del usuario</returns>
    public async Task<IEnumerable<BitacoraConUsuarioDto>> GetByEntidadYIdConUsuarioAsync(string entidad, int idEntidad)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                sb.ID_BITACORA as IdBitacora,
                sb.ID_ENTIDAD as IdEntidad,
                sb.ENTIDAD as Entidad,
                sb.ACCION as Accion,
                sb.DESCRIPCION as Descripcion,
                sb.FECHA_ACCION as FechaAccion,
                sb.ID_CREADOR as IdCreador,
                ssu.APELLIDO_PATERNO as ApellidoPaterno,
                ssu.APELLIDO_MATERNO as ApellidoMaterno,
                ssu.NOMBRES as Nombres
            FROM SHM_BITACORA sb
            LEFT JOIN SHM_SEG_USUARIO ssu ON sb.ID_CREADOR = ssu.ID_USUARIO
            WHERE sb.ID_ENTIDAD = :IdEntidad
              AND sb.ENTIDAD = :Entidad
              AND sb.ACTIVO = 1
            ORDER BY sb.FECHA_CREACION ASC";

        return await connection.QueryAsync<BitacoraConUsuarioDto>(sql, new { Entidad = entidad, IdEntidad = idEntidad });
    }
}
