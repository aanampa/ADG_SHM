using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de relaciones orden de pago - produccion.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// <modified>ADG Antonio - 2026-02-07 - Renombrado de OrdenPagoLiquidacion a OrdenPagoProduccion</modified>
/// </summary>
public class OrdenPagoProduccionRepository : IOrdenPagoProduccionRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            opp.ID_ORDEN_PAGO_PRODUCCION as IdOrdenPagoProduccion,
            opp.ID_ORDEN_PAGO as IdOrdenPago,
            opp.ID_PRODUCCION as IdProduccion,
            opp.GUID_REGISTRO as GuidRegistro,
            opp.ACTIVO as Activo,
            opp.ID_CREADOR as IdCreador,
            opp.FECHA_CREACION as FechaCreacion,
            opp.ID_MODIFICADOR as IdModificador,
            opp.FECHA_MODIFICACION as FechaModificacion,
            op.NUMERO_ORDEN_PAGO as NumeroOrdenPago,
            p.NUMERO_LIQUIDACION as NumeroLiquidacion
        FROM SHM_ORDEN_PAGO_PRODUCCION opp
        LEFT JOIN SHM_ORDEN_PAGO op ON opp.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
        LEFT JOIN SHM_PRODUCCION p ON opp.ID_PRODUCCION = p.ID_PRODUCCION";

    public OrdenPagoProduccionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las relaciones orden de pago - produccion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY opp.ID_ORDEN_PAGO_PRODUCCION DESC";

        return await connection.QueryAsync<OrdenPagoProduccion>(sql);
    }

    /// <summary>
    /// Obtiene todas las relaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccion>> GetAllActiveAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opp.ACTIVO = 1
            ORDER BY opp.ID_ORDEN_PAGO_PRODUCCION DESC";

        return await connection.QueryAsync<OrdenPagoProduccion>(sql);
    }

    /// <summary>
    /// Obtiene una relacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoProduccion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opp.ID_ORDEN_PAGO_PRODUCCION = :Id";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoProduccion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una relacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoProduccion?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opp.GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoProduccion>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Obtiene todas las producciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccion>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opp.ID_ORDEN_PAGO = :IdOrdenPago AND opp.ACTIVO = 1
            ORDER BY opp.ID_ORDEN_PAGO_PRODUCCION";

        return await connection.QueryAsync<OrdenPagoProduccion>(sql, new { IdOrdenPago = idOrdenPago });
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago de una produccion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoProduccion>> GetByProduccionIdAsync(int idProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opp.ID_PRODUCCION = :IdProduccion AND opp.ACTIVO = 1
            ORDER BY opp.ID_ORDEN_PAGO_PRODUCCION";

        return await connection.QueryAsync<OrdenPagoProduccion>(sql, new { IdProduccion = idProduccion });
    }

    /// <summary>
    /// Obtiene una relacion especifica por orden de pago y produccion.
    /// </summary>
    public async Task<OrdenPagoProduccion?> GetByOrdenPagoAndProduccionAsync(int idOrdenPago, int idProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opp.ID_ORDEN_PAGO = :IdOrdenPago
              AND opp.ID_PRODUCCION = :IdProduccion
              AND opp.ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoProduccion>(sql,
            new { IdOrdenPago = idOrdenPago, IdProduccion = idProduccion });
    }

    /// <summary>
    /// Crea una nueva relacion orden de pago - produccion.
    /// </summary>
    public async Task<int> CreateAsync(OrdenPagoProduccion ordenPagoProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ORDEN_PAGO_PRODUCCION (
                ID_ORDEN_PAGO_PRODUCCION,
                ID_ORDEN_PAGO,
                ID_PRODUCCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_PRODUCCION_SEQ.NEXTVAL,
                :IdOrdenPago,
                :IdProduccion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ORDEN_PAGO_PRODUCCION INTO :IdOrdenPagoProduccion";

        var parameters = new DynamicParameters();
        parameters.Add("IdOrdenPago", ordenPagoProduccion.IdOrdenPago);
        parameters.Add("IdProduccion", ordenPagoProduccion.IdProduccion);
        parameters.Add("IdCreador", ordenPagoProduccion.IdCreador);
        parameters.Add("IdOrdenPagoProduccion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOrdenPagoProduccion");
    }

    /// <summary>
    /// Crea multiples relaciones en una sola operacion.
    /// </summary>
    public async Task<int> CreateBulkAsync(IEnumerable<OrdenPagoProduccion> producciones)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var count = 0;
            foreach (var item in producciones)
            {
                var sql = @"
                    INSERT INTO SHM_ORDEN_PAGO_PRODUCCION (
                        ID_ORDEN_PAGO_PRODUCCION,
                        ID_ORDEN_PAGO,
                        ID_PRODUCCION,
                        GUID_REGISTRO,
                        ACTIVO,
                        ID_CREADOR,
                        FECHA_CREACION
                    ) VALUES (
                        SHM_ORDEN_PAGO_PRODUCCION_SEQ.NEXTVAL,
                        :IdOrdenPago,
                        :IdProduccion,
                        SYS_GUID(),
                        1,
                        :IdCreador,
                        SYSDATE
                    )";

                await connection.ExecuteAsync(sql, new
                {
                    item.IdOrdenPago,
                    item.IdProduccion,
                    item.IdCreador
                }, transaction);

                count++;
            }

            transaction.Commit();
            return count;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Actualiza una relacion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(OrdenPagoProduccion ordenPagoProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_PRODUCCION
            SET
                ID_ORDEN_PAGO = :IdOrdenPago,
                ID_PRODUCCION = :IdProduccion,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_PRODUCCION = :IdOrdenPagoProduccion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ordenPagoProduccion.IdOrdenPagoProduccion,
            ordenPagoProduccion.IdOrdenPago,
            ordenPagoProduccion.IdProduccion,
            ordenPagoProduccion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una relacion.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_PRODUCCION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_PRODUCCION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente todas las producciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_PRODUCCION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO = :IdOrdenPago AND ACTIVO = 1";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdOrdenPago = idOrdenPago, IdModificador = idModificador });

        return rowsAffected > 0;
    }
}
