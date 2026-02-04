using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de aprobaciones de ordenes de pago.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoAprobacionRepository : IOrdenPagoAprobacionRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            opa.ID_ORDEN_PAGO_APROBACION as IdOrdenPagoAprobacion,
            opa.ID_ORDEN_PAGO as IdOrdenPago,
            opa.ID_ROL as IdRol,
            opa.GUID_REGISTRO as GuidRegistro,
            opa.ACTIVO as Activo,
            opa.ID_CREADOR as IdCreador,
            opa.FECHA_CREACION as FechaCreacion,
            opa.ID_MODIFICADOR as IdModificador,
            opa.FECHA_MODIFICACION as FechaModificacion,
            op.NUMERO_ORDEN_PAGO as NumeroOrdenPago,
            r.NOMBRE as NombreRol,
            u.NOMBRES || ' ' || u.APELLIDO_PATERNO as NombreAprobador
        FROM SHM_ORDEN_PAGO_APROBACION opa
        LEFT JOIN SHM_ORDEN_PAGO op ON opa.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
        LEFT JOIN SHM_SEG_ROL r ON opa.ID_ROL = r.ID_ROL
        LEFT JOIN SHM_SEG_USUARIO u ON opa.ID_CREADOR = u.ID_USUARIO";

    public OrdenPagoAprobacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY opa.ID_ORDEN_PAGO_APROBACION DESC";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql);
    }

    /// <summary>
    /// Obtiene todas las aprobaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetAllActiveAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ACTIVO = 1
            ORDER BY opa.ID_ORDEN_PAGO_APROBACION DESC";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql);
    }

    /// <summary>
    /// Obtiene una aprobacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ORDEN_PAGO_APROBACION = :Id";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una aprobacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ORDEN_PAGO = :IdOrdenPago AND opa.ACTIVO = 1
            ORDER BY opa.FECHA_CREACION";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql, new { IdOrdenPago = idOrdenPago });
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de un rol.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetByRolIdAsync(int idRol)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ROL = :IdRol AND opa.ACTIVO = 1
            ORDER BY opa.FECHA_CREACION DESC";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql, new { IdRol = idRol });
    }

    /// <summary>
    /// Obtiene una aprobacion especifica por orden de pago y rol.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetByOrdenPagoAndRolAsync(int idOrdenPago, int idRol)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ORDEN_PAGO = :IdOrdenPago
              AND opa.ID_ROL = :IdRol
              AND opa.ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql,
            new { IdOrdenPago = idOrdenPago, IdRol = idRol });
    }

    /// <summary>
    /// Crea una nueva aprobacion de orden de pago.
    /// </summary>
    public async Task<int> CreateAsync(OrdenPagoAprobacion ordenPagoAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ORDEN_PAGO_APROBACION (
                ID_ORDEN_PAGO_APROBACION,
                ID_ORDEN_PAGO,
                ID_ROL,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_APROBACION_SEQ.NEXTVAL,
                :IdOrdenPago,
                :IdRol,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ORDEN_PAGO_APROBACION INTO :IdOrdenPagoAprobacion";

        var parameters = new DynamicParameters();
        parameters.Add("IdOrdenPago", ordenPagoAprobacion.IdOrdenPago);
        parameters.Add("IdRol", ordenPagoAprobacion.IdRol);
        parameters.Add("IdCreador", ordenPagoAprobacion.IdCreador);
        parameters.Add("IdOrdenPagoAprobacion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOrdenPagoAprobacion");
    }

    /// <summary>
    /// Actualiza una aprobacion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(OrdenPagoAprobacion ordenPagoAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET
                ID_ORDEN_PAGO = :IdOrdenPago,
                ID_ROL = :IdRol,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_APROBACION = :IdOrdenPagoAprobacion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ordenPagoAprobacion.IdOrdenPagoAprobacion,
            ordenPagoAprobacion.IdOrdenPago,
            ordenPagoAprobacion.IdRol,
            ordenPagoAprobacion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una aprobacion.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_APROBACION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO = :IdOrdenPago AND ACTIVO = 1";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdOrdenPago = idOrdenPago, IdModificador = idModificador });

        return rowsAffected > 0;
    }
}
