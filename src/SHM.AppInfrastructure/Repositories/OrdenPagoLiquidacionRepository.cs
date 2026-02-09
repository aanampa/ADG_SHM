using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.DTOs.OrdenPagoLiquidacion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de liquidaciones de ordenes de pago.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-07</created>
/// </summary>
public class OrdenPagoLiquidacionRepository : IOrdenPagoLiquidacionRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            opl.ID_ORDEN_PAGO_LIQUIDACION as IdOrdenPagoLiquidacion,
            opl.ID_ORDEN_PAGO as IdOrdenPago,
            opl.NUMERO_LIQUIDACION as NumeroLiquidacion,
            opl.CODIGO_LIQUIDACION as CodigoLiquidacion,
            opl.MTO_CONSUMO_ACUM as MtoConsumoAcum,
            opl.MTO_DESCUENTO_ACUM as MtoDescuentoAcum,
            opl.MTO_SUBTOTAL_ACUM as MtoSubtotalAcum,
            opl.MTO_RENTA_ACUM as MtoRentaAcum,
            opl.MTO_IGV_ACUM as MtoIgvAcum,
            opl.MTO_TOTAL_ACUM as MtoTotalAcum,
            opl.CANT_COMPROBANTES as CantComprobantes,
            opl.DESCRIPCION_LIQUIDACION as DescripcionLiquidacion,
            opl.PERIODO_LIQUIDACION as PeriodoLiquidacion,
            opl.ID_BANCO as IdBanco,
            opl.TIPO_LIQUIDACION as TipoLiquidacion,
            opl.COMENTARIOS as Comentarios,
            opl.GUID_REGISTRO as GuidRegistro,
            opl.ACTIVO as Activo,
            opl.ID_CREADOR as IdCreador,
            opl.FECHA_CREACION as FechaCreacion,
            opl.ID_MODIFICADOR as IdModificador,
            opl.FECHA_MODIFICACION as FechaModificacion,
            op.NUMERO_ORDEN_PAGO as NumeroOrdenPago,
            b.NOMBRE_BANCO as NombreBanco
        FROM SHM_ORDEN_PAGO_LIQUIDACION opl
        LEFT JOIN SHM_ORDEN_PAGO op ON opl.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
        LEFT JOIN SHM_BANCO b ON opl.ID_BANCO = b.ID_BANCO";

    public OrdenPagoLiquidacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las liquidaciones de ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiquidacion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY opl.ID_ORDEN_PAGO_LIQUIDACION DESC";

        return await connection.QueryAsync<OrdenPagoLiquidacion>(sql);
    }

    /// <summary>
    /// Obtiene todas las liquidaciones activas.
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
    /// Obtiene una liquidacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoLiquidacion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.ID_ORDEN_PAGO_LIQUIDACION = :Id";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoLiquidacion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una liquidacion por su GUID.
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
    /// Obtiene una liquidacion por su numero.
    /// </summary>
    public async Task<OrdenPagoLiquidacion?> GetByNumeroLiquidacionAsync(string numeroLiquidacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.NUMERO_LIQUIDACION = :NumeroLiquidacion AND opl.ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoLiquidacion>(sql, new { NumeroLiquidacion = numeroLiquidacion });
    }

    /// <summary>
    /// Obtiene una liquidacion por su codigo.
    /// </summary>
    public async Task<OrdenPagoLiquidacion?> GetByCodigoLiquidacionAsync(string codigoLiquidacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opl.CODIGO_LIQUIDACION = :CodigoLiquidacion AND opl.ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoLiquidacion>(sql, new { CodigoLiquidacion = codigoLiquidacion });
    }

    /// <summary>
    /// Crea una nueva liquidacion de orden de pago.
    /// </summary>
    public async Task<int> CreateAsync(OrdenPagoLiquidacion ordenPagoLiquidacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ORDEN_PAGO_LIQUIDACION (
                ID_ORDEN_PAGO_LIQUIDACION,
                ID_ORDEN_PAGO,
                NUMERO_LIQUIDACION,
                CODIGO_LIQUIDACION,
                MTO_CONSUMO_ACUM,
                MTO_DESCUENTO_ACUM,
                MTO_SUBTOTAL_ACUM,
                MTO_RENTA_ACUM,
                MTO_IGV_ACUM,
                MTO_TOTAL_ACUM,
                CANT_COMPROBANTES,
                DESCRIPCION_LIQUIDACION,
                PERIODO_LIQUIDACION,
                ID_BANCO,
                TIPO_LIQUIDACION,
                COMENTARIOS,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_LIQUIDACION_SEQ.NEXTVAL,
                :IdOrdenPago,
                :NumeroLiquidacion,
                :CodigoLiquidacion,
                :MtoConsumoAcum,
                :MtoDescuentoAcum,
                :MtoSubtotalAcum,
                :MtoRentaAcum,
                :MtoIgvAcum,
                :MtoTotalAcum,
                :CantComprobantes,
                :DescripcionLiquidacion,
                :PeriodoLiquidacion,
                :IdBanco,
                :TipoLiquidacion,
                :Comentarios,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ORDEN_PAGO_LIQUIDACION INTO :IdOrdenPagoLiquidacion";

        var parameters = new DynamicParameters();
        parameters.Add("IdOrdenPago", ordenPagoLiquidacion.IdOrdenPago);
        parameters.Add("NumeroLiquidacion", ordenPagoLiquidacion.NumeroLiquidacion);
        parameters.Add("CodigoLiquidacion", ordenPagoLiquidacion.CodigoLiquidacion);
        parameters.Add("MtoConsumoAcum", ordenPagoLiquidacion.MtoConsumoAcum);
        parameters.Add("MtoDescuentoAcum", ordenPagoLiquidacion.MtoDescuentoAcum);
        parameters.Add("MtoSubtotalAcum", ordenPagoLiquidacion.MtoSubtotalAcum);
        parameters.Add("MtoRentaAcum", ordenPagoLiquidacion.MtoRentaAcum);
        parameters.Add("MtoIgvAcum", ordenPagoLiquidacion.MtoIgvAcum);
        parameters.Add("MtoTotalAcum", ordenPagoLiquidacion.MtoTotalAcum);
        parameters.Add("CantComprobantes", ordenPagoLiquidacion.CantComprobantes);
        parameters.Add("DescripcionLiquidacion", ordenPagoLiquidacion.DescripcionLiquidacion);
        parameters.Add("PeriodoLiquidacion", ordenPagoLiquidacion.PeriodoLiquidacion);
        parameters.Add("IdBanco", ordenPagoLiquidacion.IdBanco);
        parameters.Add("TipoLiquidacion", ordenPagoLiquidacion.TipoLiquidacion);
        parameters.Add("Comentarios", ordenPagoLiquidacion.Comentarios);
        parameters.Add("IdCreador", ordenPagoLiquidacion.IdCreador);
        parameters.Add("IdOrdenPagoLiquidacion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOrdenPagoLiquidacion");
    }

    /// <summary>
    /// Actualiza una liquidacion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(OrdenPagoLiquidacion ordenPagoLiquidacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_LIQUIDACION
            SET
                ID_ORDEN_PAGO = :IdOrdenPago,
                NUMERO_LIQUIDACION = :NumeroLiquidacion,
                CODIGO_LIQUIDACION = :CodigoLiquidacion,
                MTO_CONSUMO_ACUM = :MtoConsumoAcum,
                MTO_DESCUENTO_ACUM = :MtoDescuentoAcum,
                MTO_SUBTOTAL_ACUM = :MtoSubtotalAcum,
                MTO_RENTA_ACUM = :MtoRentaAcum,
                MTO_IGV_ACUM = :MtoIgvAcum,
                MTO_TOTAL_ACUM = :MtoTotalAcum,
                CANT_COMPROBANTES = :CantComprobantes,
                DESCRIPCION_LIQUIDACION = :DescripcionLiquidacion,
                PERIODO_LIQUIDACION = :PeriodoLiquidacion,
                ID_BANCO = :IdBanco,
                TIPO_LIQUIDACION = :TipoLiquidacion,
                COMENTARIOS = :Comentarios,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_LIQUIDACION = :IdOrdenPagoLiquidacion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ordenPagoLiquidacion.IdOrdenPagoLiquidacion,
            ordenPagoLiquidacion.IdOrdenPago,
            ordenPagoLiquidacion.NumeroLiquidacion,
            ordenPagoLiquidacion.CodigoLiquidacion,
            ordenPagoLiquidacion.MtoConsumoAcum,
            ordenPagoLiquidacion.MtoDescuentoAcum,
            ordenPagoLiquidacion.MtoSubtotalAcum,
            ordenPagoLiquidacion.MtoRentaAcum,
            ordenPagoLiquidacion.MtoIgvAcum,
            ordenPagoLiquidacion.MtoTotalAcum,
            ordenPagoLiquidacion.CantComprobantes,
            ordenPagoLiquidacion.DescripcionLiquidacion,
            ordenPagoLiquidacion.PeriodoLiquidacion,
            ordenPagoLiquidacion.IdBanco,
            ordenPagoLiquidacion.TipoLiquidacion,
            ordenPagoLiquidacion.Comentarios,
            ordenPagoLiquidacion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una liquidacion.
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

    /// <summary>
    /// Obtiene el detalle de producciones por liquidacion para una orden de pago.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-08</created>
    /// </summary>
    public async Task<IEnumerable<DetalleLiquidacionItemDto>> GetDetalleLiquidacionesByOrdenPagoIdAsync(int idOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                T1.CODIGO_LIQUIDACION AS CodigoLiquidacion,
                T1.DESCRIPCION_LIQUIDACION AS DescripcionLiquidacion,
                T1.TIPO_LIQUIDACION AS TipoLiquidacion,
                T1.PERIODO_LIQUIDACION AS PeriodoLiquidacion,
                EM.RUC AS Ruc,
                EM.TIPO_ENTIDAD_MEDICA AS TipoEntidadMedica,
                EM.RAZON_SOCIAL AS RazonSocial,
                B.NOMBRE_BANCO AS NombreBanco,
                T1.TIPO_COMPROBANTE AS TipoComprobante,
                T1.SERIE AS Serie,
                T1.NUMERO AS Numero,
                T1.MTO_SUBTOTAL AS MtoSubtotal,
                T1.MTO_IGV AS MtoIgv,
                T1.MTO_RENTA AS MtoRenta,
                T1.MTO_TOTAL AS MtoTotal
            FROM SHM_PRODUCCION T1
            INNER JOIN SHM_ENTIDAD_MEDICA EM
                ON T1.ID_ENTIDAD_MEDICA = EM.ID_ENTIDAD_MEDICA
            INNER JOIN SHM_ENTIDAD_CUENTA_BANCO ECB
                ON T1.ID_CUENTA_BANCO = ECB.ID_CUENTA_BANCO
            INNER JOIN SHM_ORDEN_PAGO_LIQUIDACION T2
                ON T1.NUMERO_LIQUIDACION = T2.NUMERO_LIQUIDACION
                AND T1.CODIGO_LIQUIDACION = T2.CODIGO_LIQUIDACION
                AND ECB.ID_BANCO = T2.ID_BANCO
            LEFT JOIN SHM_BANCO B
                ON ECB.ID_BANCO = B.ID_BANCO
            WHERE T2.ID_ORDEN_PAGO = :IdOrdenPago
                AND T1.ACTIVO = 1
            ORDER BY T1.CODIGO_LIQUIDACION, T1.CODIGO_PRODUCCION";

        return await connection.QueryAsync<DetalleLiquidacionItemDto>(sql, new { IdOrdenPago = idOrdenPago });
    }
}
