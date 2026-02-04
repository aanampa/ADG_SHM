using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de relaciones orden de pago - liquidacion.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoLiquidacionRepository : IOrdenPagoLiquidacionRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            opl.ID_ORDEN_PAGO_LIQUIDACION as IdOrdenPagoLiquidacion,
            opl.ID_ORDEN_PAGO as IdOrdenPago,
            opl.ID_PRODUCCION as IdProduccion,
            opl.GUID_REGISTRO as GuidRegistro,
            opl.ACTIVO as Activo,
            opl.ID_CREADOR as IdCreador,
            opl.FECHA_CREACION as FechaCreacion,
            opl.ID_MODIFICADOR as IdModificador,
            opl.FECHA_MODIFICACION as FechaModificacion,
            op.NUMERO_ORDEN_PAGO as NumeroOrdenPago,
            p.NUMERO_LIQUIDACION as NumeroLiquidacion
        FROM SHM_ORDEN_PAGO_LIQUIDACION opl
        LEFT JOIN SHM_ORDEN_PAGO op ON opl.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
        LEFT JOIN SHM_PRODUCCION p ON opl.ID_PRODUCCION = p.ID_PRODUCCION";

    public OrdenPagoLiquidacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las relaciones orden de pago - liquidacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY opl.ID_ORDEN_PAGO_LIQUIDACION DESC";

        return await connection.QueryAsync<OrdenPagoLiquidacion>(sql);
    }

    /// <summary>
    /// Obtiene todas las relaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacion>> GetAllActiveAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.ACTIVO = 1
            ORDER BY opl.ID_ORDEN_PAGO_LIQUIDACION DESC";

        return await connection.QueryAsync<OrdenPagoLiquidacion>(sql);
    }

    /// <summary>
    /// Obtiene una relacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoLiquidacion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.ID_ORDEN_PAGO_LIQUIDACION = :Id";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoLiquidacion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una relacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoLiquidacion?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoLiquidacion>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Obtiene todas las liquidaciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacion>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.ID_ORDEN_PAGO = :IdOrdenPago AND opl.ACTIVO = 1
            ORDER BY opl.ID_ORDEN_PAGO_LIQUIDACION";

        return await connection.QueryAsync<OrdenPagoLiquidacion>(sql, new { IdOrdenPago = idOrdenPago });
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago de una produccion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacion>> GetByProduccionIdAsync(int idProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.ID_PRODUCCION = :IdProduccion AND opl.ACTIVO = 1
            ORDER BY opl.ID_ORDEN_PAGO_LIQUIDACION";

        return await connection.QueryAsync<OrdenPagoLiquidacion>(sql, new { IdProduccion = idProduccion });
    }

    /// <summary>
    /// Obtiene una relacion especifica por orden de pago y produccion.
    /// </summary>
    public async Task<OrdenPagoLiquidacion?> GetByOrdenPagoAndProduccionAsync(int idOrdenPago, int idProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.ID_ORDEN_PAGO = :IdOrdenPago
              AND opl.ID_PRODUCCION = :IdProduccion
              AND opl.ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoLiquidacion>(sql,
            new { IdOrdenPago = idOrdenPago, IdProduccion = idProduccion });
    }

    /// <summary>
    /// Crea una nueva relacion orden de pago - liquidacion.
    /// </summary>
    public async Task<int> CreateAsync(OrdenPagoLiquidacion ordenPagoLiquidacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ORDEN_PAGO_LIQUIDACION (
                ID_ORDEN_PAGO_LIQUIDACION,
                ID_ORDEN_PAGO,
                ID_PRODUCCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_LIQUIDACION_SEQ.NEXTVAL,
                :IdOrdenPago,
                :IdProduccion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ORDEN_PAGO_LIQUIDACION INTO :IdOrdenPagoLiquidacion";

        var parameters = new DynamicParameters();
        parameters.Add("IdOrdenPago", ordenPagoLiquidacion.IdOrdenPago);
        parameters.Add("IdProduccion", ordenPagoLiquidacion.IdProduccion);
        parameters.Add("IdCreador", ordenPagoLiquidacion.IdCreador);
        parameters.Add("IdOrdenPagoLiquidacion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOrdenPagoLiquidacion");
    }

    /// <summary>
    /// Crea multiples relaciones en una sola operacion.
    /// </summary>
    public async Task<int> CreateBulkAsync(IEnumerable<OrdenPagoLiquidacion> liquidaciones)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var count = 0;
            foreach (var item in liquidaciones)
            {
                var sql = @"
                    INSERT INTO SHM_ORDEN_PAGO_LIQUIDACION (
                        ID_ORDEN_PAGO_LIQUIDACION,
                        ID_ORDEN_PAGO,
                        ID_PRODUCCION,
                        GUID_REGISTRO,
                        ACTIVO,
                        ID_CREADOR,
                        FECHA_CREACION
                    ) VALUES (
                        SHM_ORDEN_PAGO_LIQUIDACION_SEQ.NEXTVAL,
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
    public async Task<bool> UpdateAsync(OrdenPagoLiquidacion ordenPagoLiquidacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_LIQUIDACION
            SET
                ID_ORDEN_PAGO = :IdOrdenPago,
                ID_PRODUCCION = :IdProduccion,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_LIQUIDACION = :IdOrdenPagoLiquidacion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ordenPagoLiquidacion.IdOrdenPagoLiquidacion,
            ordenPagoLiquidacion.IdOrdenPago,
            ordenPagoLiquidacion.IdProduccion,
            ordenPagoLiquidacion.IdModificador
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
            UPDATE SHM_ORDEN_PAGO_LIQUIDACION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_LIQUIDACION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente todas las liquidaciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_LIQUIDACION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO = :IdOrdenPago AND ACTIVO = 1";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdOrdenPago = idOrdenPago, IdModificador = idModificador });

        return rowsAffected > 0;
    }
}
