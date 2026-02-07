using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.DTOs.Liquidacion;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de liquidaciones.
/// Consulta registros de SHM_PRODUCCION con ESTADO = 'FACTURA_LIQUIDADA'
/// incluyendo datos de cuenta bancaria de la entidad medica.
///
/// <author>ADG Vladimir D</author>
/// <created>2026-02-03</created>
/// </summary>
public class LiquidacionRepository : ILiquidacionRepository
{
    private readonly string _connectionString;

    public LiquidacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene el listado paginado de liquidaciones con datos relacionados y filtro por banco.
    /// Usa paginacion compatible con Oracle 11g (ROWNUM).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    public async Task<(IEnumerable<LiquidacionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        int? idBanco, int? idSede, int pageNumber, int pageSize)
    {
        using var connection = new OracleConnection(_connectionString);

        // Construccion dinamica de WHERE
        var whereClause = "WHERE p.ACTIVO = 1 AND p.ESTADO = 'FACTURA_LIQUIDADA'";

        if (idBanco.HasValue)
        {
            whereClause += " AND cb.ID_BANCO = :IdBanco";
        }
        if (idSede.HasValue && idSede.Value > 0)
        {
            whereClause += " AND p.ID_SEDE = :IdSede";
        }

        // Query para obtener el total de registros
        var countSql = $@"
            SELECT COUNT(1)
            FROM SHM_PRODUCCION p
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN (
                SELECT ecb.ID_BANCO, ecb.CUENTA_CORRIENTE, ecb.CUENTA_CCI, ecb.MONEDA, ecb.ID_ENTIDAD_MEDICA
                FROM SHM_ENTIDAD_CUENTA_BANCO ecb
                WHERE ecb.ACTIVO = 1
                  AND ecb.ID_CUENTA_BANCO = (
                      SELECT MIN(ecb2.ID_CUENTA_BANCO)
                      FROM SHM_ENTIDAD_CUENTA_BANCO ecb2
                      WHERE ecb2.ID_ENTIDAD_MEDICA = ecb.ID_ENTIDAD_MEDICA
                        AND ecb2.ACTIVO = 1
                  )
            ) cb ON cb.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { IdBanco = idBanco, IdSede = idSede });

        // Calcular limites de paginacion
        var minRow = (pageNumber - 1) * pageSize;
        var maxRow = pageNumber * pageSize;

        // Query principal con paginacion - Compatible con Oracle 11g (ROWNUM)
        var sql = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT
                        p.ID_PRODUCCION AS IdProduccion,
                        p.GUID_REGISTRO AS GuidRegistro,
                        p.ID_SEDE AS IdSede,
                        p.ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                        p.CODIGO_PRODUCCION AS CodigoProduccion,
                        p.TIPO_PRODUCCION AS TipoProduccion,
                        tp.DESCRIPCION AS DesTipoProduccion,
                        p.TIPO_MEDICO AS TipoMedico,
                        tm.DESCRIPCION AS DesTipoMedico,
                        p.TIPO_RUBRO AS TipoRubro,
                        tr.DESCRIPCION AS DesTipoRubro,
                        p.ESTADO AS Estado,
                        ep.DESCRIPCION AS DesEstado,
                        p.DESCRIPCION AS Descripcion,
                        p.PERIODO AS Periodo,
                        p.ESTADO_PRODUCCION AS EstadoProduccion,
                        p.MTO_CONSUMO AS MtoConsumo,
                        p.MTO_DESCUENTO AS MtoDescuento,
                        p.MTO_SUBTOTAL AS MtoSubtotal,
                        p.MTO_RENTA AS MtoRenta,
                        p.MTO_IGV AS MtoIgv,
                        p.MTO_TOTAL AS MtoTotal,
                        p.TIPO_COMPROBANTE AS TipoComprobante,
                        p.CONCEPTO AS Concepto,
                        p.FECHA_LIMITE AS FechaLimite,
                        p.SERIE AS Serie,
                        p.NUMERO AS Numero,
                        p.FECHA_EMISION AS FechaEmision,
                        p.GLOSA AS Glosa,
                        p.ESTADO_COMPROBANTE AS EstadoComprobante,
                        p.NUMERO_LIQUIDACION AS NumeroLiquidacion,
                        p.CODIGO_LIQUIDACION AS CodigoLiquidacion,
                        p.PERIODO_LIQUIDACION AS PeriodoLiquidacion,
                        p.ESTADO_LIQUIDACION AS EstadoLiquidacion,
                        p.FECHA_LIQUIDACION AS FechaLiquidacion,
                        p.DESCRIPCION_LIQUIDACION AS DescripcionLiquidacion,
                        cb.ID_BANCO AS IdBanco,
                        b.CODIGO_BANCO AS CodigoBanco,
                        b.NOMBRE_BANCO AS NombreBanco,
                        cb.CUENTA_CORRIENTE AS CuentaCorriente,
                        cb.CUENTA_CCI AS CuentaCci,
                        cb.MONEDA AS Moneda,
                        s.CODIGO AS CodigoSede,
                        s.NOMBRE AS NombreSede,
                        em.RUC AS Ruc,
                        em.RAZON_SOCIAL AS RazonSocial,
                        em.TIPO_ENTIDAD_MEDICA AS TipoEntidadMedica,
                        tem.DESCRIPCION AS DesTipoEntidadMedica,
                        p.ACTIVO AS Activo,
                        p.ID_CREADOR AS IdCreador,
                        p.FECHA_CREACION AS FechaCreacion,
                        p.ID_MODIFICADOR AS IdModificador,
                        p.FECHA_MODIFICACION AS FechaModificacion
                    FROM SHM_PRODUCCION p
                    LEFT JOIN SHM_SEDE s ON s.ID_SEDE = p.ID_SEDE
                    LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
                    LEFT JOIN (
                        SELECT ecb.ID_BANCO, ecb.CUENTA_CORRIENTE, ecb.CUENTA_CCI, ecb.MONEDA, ecb.ID_ENTIDAD_MEDICA
                        FROM SHM_ENTIDAD_CUENTA_BANCO ecb
                        WHERE ecb.ACTIVO = 1
                          AND ecb.ID_CUENTA_BANCO = (
                              SELECT MIN(ecb2.ID_CUENTA_BANCO)
                              FROM SHM_ENTIDAD_CUENTA_BANCO ecb2
                              WHERE ecb2.ID_ENTIDAD_MEDICA = ecb.ID_ENTIDAD_MEDICA
                                AND ecb2.ACTIVO = 1
                          )
                    ) cb ON cb.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
                    LEFT JOIN SHM_BANCO b ON b.ID_BANCO = cb.ID_BANCO
                    LEFT JOIN SHM_TABLA_DETALLE_VW tp ON tp.CODIGO_TABLA = 'TIPO_PRODUCCION' AND tp.CODIGO = p.TIPO_PRODUCCION
                    LEFT JOIN SHM_TABLA_DETALLE_VW tm ON tm.CODIGO_TABLA = 'TIPO_MEDICO' AND tm.CODIGO = p.TIPO_MEDICO
                    LEFT JOIN SHM_TABLA_DETALLE_VW tr ON tr.CODIGO_TABLA = 'TIPO_RUBRO' AND tr.CODIGO = p.TIPO_RUBRO
                    LEFT JOIN SHM_TABLA_DETALLE_VW ep ON ep.CODIGO_TABLA = 'ESTADO_PROCESO' AND ep.CODIGO = p.ESTADO
                    LEFT JOIN SHM_TABLA_DETALLE_VW tem ON tem.CODIGO_TABLA = 'TIPO_ENTIDAD_MEDICA' AND tem.CODIGO = em.TIPO_ENTIDAD_MEDICA
                    {whereClause}
                    ORDER BY p.FECHA_LIQUIDACION DESC, p.CODIGO_PRODUCCION DESC
                ) a
                WHERE ROWNUM <= :MaxRow
            )
            WHERE rnum > :MinRow";

        var items = await connection.QueryAsync<LiquidacionListaResponseDto>(sql, new
        {
            IdBanco = idBanco,
            IdSede = idSede,
            MinRow = minRow,
            MaxRow = maxRow
        });

        return (items, totalCount);
    }

    /// <summary>
    /// Obtiene una liquidacion por su GUID con datos relacionados.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-03</created>
    /// </summary>
    public async Task<LiquidacionListaResponseDto?> GetByGuidWithDetailsAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                p.ID_PRODUCCION AS IdProduccion,
                p.GUID_REGISTRO AS GuidRegistro,
                p.ID_SEDE AS IdSede,
                p.ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                p.CODIGO_PRODUCCION AS CodigoProduccion,
                p.TIPO_PRODUCCION AS TipoProduccion,
                tp.DESCRIPCION AS DesTipoProduccion,
                p.TIPO_MEDICO AS TipoMedico,
                tm.DESCRIPCION AS DesTipoMedico,
                p.TIPO_RUBRO AS TipoRubro,
                tr.DESCRIPCION AS DesTipoRubro,
                p.ESTADO AS Estado,
                ep.DESCRIPCION AS DesEstado,
                p.DESCRIPCION AS Descripcion,
                p.PERIODO AS Periodo,
                p.ESTADO_PRODUCCION AS EstadoProduccion,
                p.MTO_CONSUMO AS MtoConsumo,
                p.MTO_DESCUENTO AS MtoDescuento,
                p.MTO_SUBTOTAL AS MtoSubtotal,
                p.MTO_RENTA AS MtoRenta,
                p.MTO_IGV AS MtoIgv,
                p.MTO_TOTAL AS MtoTotal,
                p.TIPO_COMPROBANTE AS TipoComprobante,
                p.CONCEPTO AS Concepto,
                p.FECHA_LIMITE AS FechaLimite,
                p.SERIE AS Serie,
                p.NUMERO AS Numero,
                p.FECHA_EMISION AS FechaEmision,
                p.GLOSA AS Glosa,
                p.ESTADO_COMPROBANTE AS EstadoComprobante,
                p.NUMERO_LIQUIDACION AS NumeroLiquidacion,
                p.CODIGO_LIQUIDACION AS CodigoLiquidacion,
                p.PERIODO_LIQUIDACION AS PeriodoLiquidacion,
                p.ESTADO_LIQUIDACION AS EstadoLiquidacion,
                p.FECHA_LIQUIDACION AS FechaLiquidacion,
                p.DESCRIPCION_LIQUIDACION AS DescripcionLiquidacion,
                cb.ID_BANCO AS IdBanco,
                b.CODIGO_BANCO AS CodigoBanco,
                b.NOMBRE_BANCO AS NombreBanco,
                cb.CUENTA_CORRIENTE AS CuentaCorriente,
                cb.CUENTA_CCI AS CuentaCci,
                cb.MONEDA AS Moneda,
                s.CODIGO AS CodigoSede,
                s.NOMBRE AS NombreSede,
                em.RUC AS Ruc,
                em.RAZON_SOCIAL AS RazonSocial,
                em.TIPO_ENTIDAD_MEDICA AS TipoEntidadMedica,
                tem.DESCRIPCION AS DesTipoEntidadMedica,
                p.ACTIVO AS Activo,
                p.ID_CREADOR AS IdCreador,
                p.FECHA_CREACION AS FechaCreacion,
                p.ID_MODIFICADOR AS IdModificador,
                p.FECHA_MODIFICACION AS FechaModificacion
            FROM SHM_PRODUCCION p
            LEFT JOIN SHM_SEDE s ON s.ID_SEDE = p.ID_SEDE
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN (
                SELECT ecb.ID_BANCO, ecb.CUENTA_CORRIENTE, ecb.CUENTA_CCI, ecb.MONEDA, ecb.ID_ENTIDAD_MEDICA
                FROM SHM_ENTIDAD_CUENTA_BANCO ecb
                WHERE ecb.ACTIVO = 1
                  AND ecb.ID_CUENTA_BANCO = (
                      SELECT MIN(ecb2.ID_CUENTA_BANCO)
                      FROM SHM_ENTIDAD_CUENTA_BANCO ecb2
                      WHERE ecb2.ID_ENTIDAD_MEDICA = ecb.ID_ENTIDAD_MEDICA
                        AND ecb2.ACTIVO = 1
                  )
            ) cb ON cb.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN SHM_BANCO b ON b.ID_BANCO = cb.ID_BANCO
            LEFT JOIN SHM_TABLA_DETALLE_VW tp ON tp.CODIGO_TABLA = 'TIPO_PRODUCCION' AND tp.CODIGO = p.TIPO_PRODUCCION
            LEFT JOIN SHM_TABLA_DETALLE_VW tm ON tm.CODIGO_TABLA = 'TIPO_MEDICO' AND tm.CODIGO = p.TIPO_MEDICO
            LEFT JOIN SHM_TABLA_DETALLE_VW tr ON tr.CODIGO_TABLA = 'TIPO_RUBRO' AND tr.CODIGO = p.TIPO_RUBRO
            LEFT JOIN SHM_TABLA_DETALLE_VW ep ON ep.CODIGO_TABLA = 'ESTADO_PROCESO' AND ep.CODIGO = p.ESTADO
            LEFT JOIN SHM_TABLA_DETALLE_VW tem ON tem.CODIGO_TABLA = 'TIPO_ENTIDAD_MEDICA' AND tem.CODIGO = em.TIPO_ENTIDAD_MEDICA
            WHERE p.GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<LiquidacionListaResponseDto>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene el listado agrupado de liquidaciones por CODIGO_LIQUIDACION e ID_BANCO.
    /// Para la generacion de ordenes de pago.
    /// </summary>
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    public async Task<IEnumerable<LiquidacionGrupoResponseDto>> GetGruposAsync(int? idBanco, int idSede)
    {
        using var connection = new OracleConnection(_connectionString);

        // Construccion dinamica de WHERE
        var whereClause = "WHERE p.ACTIVO = 1 AND p.ESTADO = 'FACTURA_LIQUIDADA' AND p.ID_SEDE = :IdSede";

        if (idBanco.HasValue)
        {
            whereClause += " AND cb.ID_BANCO = :IdBanco";
        }

        var sql = $@"
            SELECT
                p.ID_SEDE AS IdSede,
                p.CODIGO_LIQUIDACION AS CodigoLiquidacion,
                MAX(p.NUMERO_LIQUIDACION) AS NumeroLiquidacion,
                cb.ID_BANCO AS IdBanco,
                MAX(b.CODIGO_BANCO) AS CodigoBanco,
                MAX(b.NOMBRE_BANCO) AS NombreBanco,
                MAX(p.TIPO_PRODUCCION) AS TipoProduccion,
                MAX(tp.DESCRIPCION) AS DesTipoProduccion,
                MAX(p.TIPO_MEDICO) AS TipoMedico,
                MAX(tm.DESCRIPCION) AS DesTipoMedico,
                MAX(p.TIPO_RUBRO) AS TipoRubro,
                MAX(tr.DESCRIPCION) AS DesTipoRubro,
                MAX(p.DESCRIPCION_LIQUIDACION) AS Descripcion,
                MAX(p.PERIODO) AS Periodo,
                SUM(p.MTO_TOTAL) AS MtoTotal,
                COUNT(p.ID_PRODUCCION) AS CantidadFacturas
            FROM SHM_PRODUCCION p
            LEFT JOIN SHM_SEDE s ON s.ID_SEDE = p.ID_SEDE
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN (
                SELECT ecb.ID_BANCO, ecb.ID_ENTIDAD_MEDICA
                FROM SHM_ENTIDAD_CUENTA_BANCO ecb
                WHERE ecb.ACTIVO = 1
                  AND ecb.ID_CUENTA_BANCO = (
                      SELECT MIN(ecb2.ID_CUENTA_BANCO)
                      FROM SHM_ENTIDAD_CUENTA_BANCO ecb2
                      WHERE ecb2.ID_ENTIDAD_MEDICA = ecb.ID_ENTIDAD_MEDICA
                        AND ecb2.ACTIVO = 1
                  )
            ) cb ON cb.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN SHM_BANCO b ON b.ID_BANCO = cb.ID_BANCO
            LEFT JOIN SHM_TABLA_DETALLE_VW tp ON tp.CODIGO_TABLA = 'TIPO_PRODUCCION' AND tp.CODIGO = p.TIPO_PRODUCCION
            LEFT JOIN SHM_TABLA_DETALLE_VW tm ON tm.CODIGO_TABLA = 'TIPO_MEDICO' AND tm.CODIGO = p.TIPO_MEDICO
            LEFT JOIN SHM_TABLA_DETALLE_VW tr ON tr.CODIGO_TABLA = 'TIPO_RUBRO' AND tr.CODIGO = p.TIPO_RUBRO
            {whereClause}
            GROUP BY p.ID_SEDE, p.CODIGO_LIQUIDACION, cb.ID_BANCO
            ORDER BY p.CODIGO_LIQUIDACION DESC";

        return await connection.QueryAsync<LiquidacionGrupoResponseDto>(sql, new { IdBanco = idBanco, IdSede = idSede });
    }

    /// <summary>
    /// Obtiene las producciones de un codigo de liquidacion especifico.
    /// </summary>
    /// <author>ADG Vladimir D</author>
    /// <created>2026-02-06</created>
    /// <modified>ADG Vladimir D - 2026-02-06 - Agregado filtro por ID_BANCO</modified>
    public async Task<IEnumerable<LiquidacionListaResponseDto>> GetProduccionesByCodigoLiquidacionAsync(
        string codigoLiquidacion, int idSede, int? idBanco = null)
    {
        using var connection = new OracleConnection(_connectionString);

        // Construccion dinamica de WHERE para filtro opcional por banco
        var whereClause = @"WHERE p.ACTIVO = 1
              AND p.ESTADO = 'FACTURA_LIQUIDADA'
              AND p.CODIGO_LIQUIDACION = :CodigoLiquidacion
              AND p.ID_SEDE = :IdSede";

        if (idBanco.HasValue)
        {
            whereClause += " AND cb.ID_BANCO = :IdBanco";
        }

        var sql = $@"
            SELECT
                p.ID_PRODUCCION AS IdProduccion,
                p.GUID_REGISTRO AS GuidRegistro,
                p.ID_SEDE AS IdSede,
                p.ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                p.CODIGO_PRODUCCION AS CodigoProduccion,
                p.TIPO_PRODUCCION AS TipoProduccion,
                tp.DESCRIPCION AS DesTipoProduccion,
                p.DESCRIPCION AS Descripcion,
                p.PERIODO AS Periodo,
                p.MTO_CONSUMO AS MtoConsumo,
                p.MTO_DESCUENTO AS MtoDescuento,
                p.MTO_SUBTOTAL AS MtoSubtotal,
                p.MTO_RENTA AS MtoRenta,
                p.MTO_IGV AS MtoIgv,
                p.MTO_TOTAL AS MtoTotal,
                p.SERIE AS Serie,
                p.NUMERO AS Numero,
                p.FECHA_EMISION AS FechaEmision,
                p.NUMERO_LIQUIDACION AS NumeroLiquidacion,
                p.CODIGO_LIQUIDACION AS CodigoLiquidacion,
                p.PERIODO_LIQUIDACION AS PeriodoLiquidacion,
                p.ESTADO_LIQUIDACION AS EstadoLiquidacion,
                p.FECHA_LIQUIDACION AS FechaLiquidacion,
                cb.ID_BANCO AS IdBanco,
                b.CODIGO_BANCO AS CodigoBanco,
                b.NOMBRE_BANCO AS NombreBanco,
                em.RUC AS Ruc,
                em.RAZON_SOCIAL AS RazonSocial,
                p.ACTIVO AS Activo
            FROM SHM_PRODUCCION p
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN (
                SELECT ecb.ID_BANCO, ecb.ID_ENTIDAD_MEDICA
                FROM SHM_ENTIDAD_CUENTA_BANCO ecb
                WHERE ecb.ACTIVO = 1
                  AND ecb.ID_CUENTA_BANCO = (
                      SELECT MIN(ecb2.ID_CUENTA_BANCO)
                      FROM SHM_ENTIDAD_CUENTA_BANCO ecb2
                      WHERE ecb2.ID_ENTIDAD_MEDICA = ecb.ID_ENTIDAD_MEDICA
                        AND ecb2.ACTIVO = 1
                  )
            ) cb ON cb.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN SHM_BANCO b ON b.ID_BANCO = cb.ID_BANCO
            LEFT JOIN SHM_TABLA_DETALLE_VW tp ON tp.CODIGO_TABLA = 'TIPO_PRODUCCION' AND tp.CODIGO = p.TIPO_PRODUCCION
            {whereClause}
            ORDER BY p.ID_PRODUCCION";

        return await connection.QueryAsync<LiquidacionListaResponseDto>(sql, new
        {
            CodigoLiquidacion = codigoLiquidacion,
            IdSede = idSede,
            IdBanco = idBanco
        });
    }
}
