using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de ordenes de pago.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoRepository : IOrdenPagoRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            op.ID_ORDEN_PAGO as IdOrdenPago,
            op.ID_SEDE as IdSede,
            op.ID_BANCO as IdBanco,
            op.NUMERO_ORDEN_PAGO as NumeroOrdenPago,
            op.FECHA_GENERACION as FechaGeneracion,
            op.ESTADO as Estado,
            op.MTO_CONSUMO_ACUM as MtoConsumoAcum,
            op.MTO_DESCUENTO_ACUM as MtoDescuentoAcum,
            op.MTO_SUBTOTAL_ACUM as MtoSubtotalAcum,
            op.MTO_RENTA_ACUM as MtoRentaAcum,
            op.MTO_IGV_ACUM as MtoIgvAcum,
            op.MTO_TOTAL_ACUM as MtoTotalAcum,
            op.CANT_COMPROBANTES as CantComprobantes,
            op.CANT_LIQUIDACIONES as CantLiquidaciones,
            op.COMENTARIOS as Comentarios,
            op.GUID_REGISTRO as GuidRegistro,
            op.ACTIVO as Activo,
            op.ID_CREADOR as IdCreador,
            op.FECHA_CREACION as FechaCreacion,
            op.ID_MODIFICADOR as IdModificador,
            op.FECHA_MODIFICACION as FechaModificacion,
            b.NOMBRE_BANCO as NombreBanco
        FROM SHM_ORDEN_PAGO op
        LEFT JOIN SHM_BANCO b ON op.ID_BANCO = b.ID_BANCO";

    public OrdenPagoRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago registradas.
    /// </summary>
    public async Task<IEnumerable<OrdenPago>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY op.ID_ORDEN_PAGO DESC";

        return await connection.QueryAsync<OrdenPago>(sql);
    }

    /// <summary>
    /// Obtiene todas las ordenes de pago activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPago>> GetAllActiveAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.ACTIVO = 1
            ORDER BY op.ID_ORDEN_PAGO DESC";

        return await connection.QueryAsync<OrdenPago>(sql);
    }

    /// <summary>
    /// Obtiene una orden de pago por su identificador.
    /// </summary>
    public async Task<OrdenPago?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.ID_ORDEN_PAGO = :Id";

        return await connection.QueryFirstOrDefaultAsync<OrdenPago>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una orden de pago por su GUID.
    /// </summary>
    public async Task<OrdenPago?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<OrdenPago>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Obtiene una orden de pago por su numero.
    /// </summary>
    public async Task<OrdenPago?> GetByNumeroOrdenPagoAsync(string numeroOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.NUMERO_ORDEN_PAGO = :NumeroOrdenPago";

        return await connection.QueryFirstOrDefaultAsync<OrdenPago>(sql, new { NumeroOrdenPago = numeroOrdenPago });
    }

    /// <summary>
    /// Obtiene ordenes de pago por banco.
    /// </summary>
    public async Task<IEnumerable<OrdenPago>> GetByBancoAsync(int idBanco)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.ID_BANCO = :IdBanco AND op.ACTIVO = 1
            ORDER BY op.ID_ORDEN_PAGO DESC";

        return await connection.QueryAsync<OrdenPago>(sql, new { IdBanco = idBanco });
    }

    /// <summary>
    /// Obtiene ordenes de pago por estado.
    /// </summary>
    public async Task<IEnumerable<OrdenPago>> GetByEstadoAsync(string estado)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.ESTADO = :Estado AND op.ACTIVO = 1
            ORDER BY op.ID_ORDEN_PAGO DESC";

        return await connection.QueryAsync<OrdenPago>(sql, new { Estado = estado });
    }

    /// <summary>
    /// Obtiene ordenes de pago por rango de fecha de generacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPago>> GetByFechaGeneracionAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE op.FECHA_GENERACION >= :FechaInicio
              AND op.FECHA_GENERACION <= :FechaFin
              AND op.ACTIVO = 1
            ORDER BY op.FECHA_GENERACION DESC";

        return await connection.QueryAsync<OrdenPago>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    /// <summary>
    /// Crea una nueva orden de pago.
    /// </summary>
    public async Task<int> CreateAsync(OrdenPago ordenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ORDEN_PAGO (
                ID_ORDEN_PAGO,
                ID_SEDE,
                ID_BANCO,
                NUMERO_ORDEN_PAGO,
                FECHA_GENERACION,
                ESTADO,
                MTO_CONSUMO_ACUM,
                MTO_DESCUENTO_ACUM,
                MTO_SUBTOTAL_ACUM,
                MTO_RENTA_ACUM,
                MTO_IGV_ACUM,
                MTO_TOTAL_ACUM,
                CANT_COMPROBANTES,
                CANT_LIQUIDACIONES,
                COMENTARIOS,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_SEQ.NEXTVAL,
                :IdSede,
                :IdBanco,
                :NumeroOrdenPago,
                :FechaGeneracion,
                :Estado,
                :MtoConsumoAcum,
                :MtoDescuentoAcum,
                :MtoSubtotalAcum,
                :MtoRentaAcum,
                :MtoIgvAcum,
                :MtoTotalAcum,
                :CantComprobantes,
                :CantLiquidaciones,
                :Comentarios,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ORDEN_PAGO INTO :IdOrdenPago";

        var parameters = new DynamicParameters();
        parameters.Add("IdSede", ordenPago.IdSede);
        parameters.Add("IdBanco", ordenPago.IdBanco);
        parameters.Add("NumeroOrdenPago", ordenPago.NumeroOrdenPago);
        parameters.Add("FechaGeneracion", ordenPago.FechaGeneracion);
        parameters.Add("Estado", ordenPago.Estado);
        parameters.Add("MtoConsumoAcum", ordenPago.MtoConsumoAcum);
        parameters.Add("MtoDescuentoAcum", ordenPago.MtoDescuentoAcum);
        parameters.Add("MtoSubtotalAcum", ordenPago.MtoSubtotalAcum);
        parameters.Add("MtoRentaAcum", ordenPago.MtoRentaAcum);
        parameters.Add("MtoIgvAcum", ordenPago.MtoIgvAcum);
        parameters.Add("MtoTotalAcum", ordenPago.MtoTotalAcum);
        parameters.Add("CantComprobantes", ordenPago.CantComprobantes);
        parameters.Add("CantLiquidaciones", ordenPago.CantLiquidaciones);
        parameters.Add("Comentarios", ordenPago.Comentarios);
        parameters.Add("IdCreador", ordenPago.IdCreador);
        parameters.Add("IdOrdenPago", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOrdenPago");
    }

    /// <summary>
    /// Actualiza una orden de pago existente.
    /// </summary>
    public async Task<bool> UpdateAsync(OrdenPago ordenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO
            SET
                ID_BANCO = :IdBanco,
                NUMERO_ORDEN_PAGO = :NumeroOrdenPago,
                FECHA_GENERACION = :FechaGeneracion,
                ESTADO = :Estado,
                MTO_CONSUMO_ACUM = :MtoConsumoAcum,
                MTO_DESCUENTO_ACUM = :MtoDescuentoAcum,
                MTO_SUBTOTAL_ACUM = :MtoSubtotalAcum,
                MTO_RENTA_ACUM = :MtoRentaAcum,
                MTO_IGV_ACUM = :MtoIgvAcum,
                MTO_TOTAL_ACUM = :MtoTotalAcum,
                CANT_COMPROBANTES = :CantComprobantes,
                CANT_LIQUIDACIONES = :CantLiquidaciones,
                COMENTARIOS = :Comentarios,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO = :IdOrdenPago";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ordenPago.IdOrdenPago,
            ordenPago.IdBanco,
            ordenPago.NumeroOrdenPago,
            ordenPago.FechaGeneracion,
            ordenPago.Estado,
            ordenPago.MtoConsumoAcum,
            ordenPago.MtoDescuentoAcum,
            ordenPago.MtoSubtotalAcum,
            ordenPago.MtoRentaAcum,
            ordenPago.MtoIgvAcum,
            ordenPago.MtoTotalAcum,
            ordenPago.CantComprobantes,
            ordenPago.CantLiquidaciones,
            ordenPago.Comentarios,
            ordenPago.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una orden de pago.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }
}
