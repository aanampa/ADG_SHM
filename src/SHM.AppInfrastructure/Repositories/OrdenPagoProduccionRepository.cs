using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.DTOs.OrdenPago;
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
/// <modified>ADG Antonio - 2026-04-15 - Agregado GetComprobantesParaSapByOrdenPagoGuidAsync</modified>
/// <modified>ADG Antonio - 2026-07-18 - Agregado GetComprobantesPendientesPagoAsync</modified>
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

    /// <summary>
    /// Obtiene los datos de comprobante de todas las producciones activas de una orden de pago,
    /// con JOIN a SHM_PRODUCCION y SHM_ENTIDAD_MEDICA para obtener los campos necesarios para SAP.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-04-15</created>
    /// </summary>
    public async Task<IEnumerable<OrdenPagoComprobanteQueryDto>> GetComprobantesParaSapByOrdenPagoGuidAsync(string guidOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                p.ID_PRODUCCION       AS IdProduccion,
                p.GUID_REGISTRO       AS GuidProduccion,
                p.TIPO_COMPROBANTE    AS TipoComprobante,
                p.SERIE               AS Serie,
                p.NUMERO              AS Numero,
                p.FECHA_EMISION       AS FechaEmision,
                em.CODIGO_ACREEDOR    AS CodigoAcreedor,
                em.RAZON_SOCIAL       AS RazonSocial,
                em.RUC                AS Ruc
            FROM SHM_ORDEN_PAGO op
            INNER JOIN SHM_ORDEN_PAGO_PRODUCCION opp ON opp.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
                                                     AND opp.ACTIVO = 1
            INNER JOIN SHM_PRODUCCION p              ON p.ID_PRODUCCION = opp.ID_PRODUCCION
            INNER JOIN SHM_ENTIDAD_MEDICA em         ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            WHERE op.GUID_REGISTRO = :GuidOrdenPago
              AND op.ACTIVO = 1
            ORDER BY opp.ID_ORDEN_PAGO_PRODUCCION";

        return await connection.QueryAsync<OrdenPagoComprobanteQueryDto>(sql, new { GuidOrdenPago = guidOrdenPago });
    }

    /// <summary>
    /// Obtiene los comprobantes pendientes de actualizar estado de pago: producciones
    /// de ordenes de pago con ESTADO = APROBADO cuyo PAGO_ESTADO aun es nulo.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-07-18</created>
    /// </summary>
    public async Task<IEnumerable<OrdenPagoComprobantePendientePagoQueryDto>> GetComprobantesPendientesPagoAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                op.ID_ORDEN_PAGO      AS IdOrdenPago,
                op.GUID_REGISTRO      AS GuidOrdenPago,
                op.NUMERO_ORDEN_PAGO  AS NumeroOrdenPago,
                p.ID_PRODUCCION       AS IdProduccion,
                p.GUID_REGISTRO       AS GuidProduccion,
                p.TIPO_COMPROBANTE    AS TipoComprobante,
                p.SERIE               AS Serie,
                p.NUMERO              AS Numero,
                p.FECHA_EMISION       AS FechaEmision,
                em.CODIGO_ACREEDOR    AS CodigoAcreedor,
                em.RAZON_SOCIAL       AS RazonSocial,
                em.RUC                AS Ruc
            FROM SHM_ORDEN_PAGO op
            INNER JOIN SHM_ORDEN_PAGO_PRODUCCION opp ON opp.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
                                                     AND opp.ACTIVO = 1
            INNER JOIN SHM_PRODUCCION p              ON p.ID_PRODUCCION = opp.ID_PRODUCCION
            INNER JOIN SHM_ENTIDAD_MEDICA em         ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            WHERE op.ACTIVO = 1
              AND op.ESTADO = 'APROBADO'
              AND p.PAGO_ESTADO IS NULL
            ORDER BY op.NUMERO_ORDEN_PAGO, opp.ID_ORDEN_PAGO_PRODUCCION";

        return await connection.QueryAsync<OrdenPagoComprobantePendientePagoQueryDto>(sql);
    }
}
