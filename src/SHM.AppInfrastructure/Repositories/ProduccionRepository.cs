using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de registros de produccion medica.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Antonio - 2026-01-20 - Agregado metodo de listado paginado con filtros</modified>
/// <modified>ADG Antonio - 2026-01-24 - Agregados campos de fechas de factura</modified>
/// </summary>
public class ProduccionRepository : IProduccionRepository
{
    private readonly string _connectionString;

    public ProduccionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Columnas de seleccion para consultas de produccion.
    /// </summary>
    private const string SelectColumns = @"
        ID_PRODUCCION as IdProduccion,
        ID_SEDE as IdSede,
        ID_ENTIDAD_MEDICA as IdEntidadMedica,
        CODIGO_PRODUCCION as CodigoProduccion,
        TIPO_PRODUCCION as TipoProduccion,
        TIPO_MEDICO as TipoMedico,
        TIPO_RUBRO as TipoRubro,
        DESCRIPCION as Descripcion,
        PERIODO as Periodo,
        ESTADO_PRODUCCION as EstadoProduccion,
        MTO_CONSUMO as MtoConsumo,
        MTO_DESCUENTO as MtoDescuento,
        MTO_SUBTOTAL as MtoSubtotal,
        MTO_RENTA as MtoRenta,
        MTO_IGV as MtoIgv,
        MTO_TOTAL as MtoTotal,
        TIPO_COMPROBANTE as TipoComprobante,
        SERIE as Serie,
        NUMERO as Numero,
        FECHA_EMISION as FechaEmision,
        GLOSA as Glosa,
        ESTADO_COMPROBANTE as EstadoComprobante,
        CONCEPTO as Concepto,
        FECHA_LIMITE as FechaLimite,
        ESTADO as Estado,
        FACTURA_FECHA_SOLICITUD as FacturaFechaSolicitud,
        FACTURA_FECHA_ENVIO as FacturaFechaEnvio,
        FACTURA_FECHA_ACEPTACION as FacturaFechaAceptacion,
        FACTURA_FECHA_PAGO as FacturaFechaPago,
        GUID_REGISTRO as GuidRegistro,
        ACTIVO as Activo,
        ID_CREADOR as IdCreador,
        FECHA_CREACION as FechaCreacion,
        ID_MODIFICADOR as IdModificador,
        FECHA_MODIFICACION as FechaModificacion";

    /// <summary>
    /// Obtiene todos los registros de produccion.
    /// </summary>
    public async Task<IEnumerable<Produccion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION ORDER BY ID_PRODUCCION DESC";

        return await connection.QueryAsync<Produccion>(sql);
    }

    /// <summary>
    /// Obtiene un registro de produccion por su identificador unico.
    /// </summary>
    public async Task<Produccion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION WHERE ID_PRODUCCION = :Id";

        return await connection.QueryFirstOrDefaultAsync<Produccion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un registro de produccion por su codigo.
    /// </summary>
    public async Task<Produccion?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION WHERE CODIGO_PRODUCCION = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<Produccion>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Obtiene un registro de produccion por su identificador GUID.
    /// </summary>
    public async Task<Produccion?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Produccion>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene los registros de produccion de una sede especifica.
    /// </summary>
    public async Task<IEnumerable<Produccion>> GetBySedeAsync(int idSede)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION WHERE ID_SEDE = :IdSede ORDER BY ID_PRODUCCION DESC";

        return await connection.QueryAsync<Produccion>(sql, new { IdSede = idSede });
    }

    /// <summary>
    /// Obtiene los registros de produccion de una entidad medica especifica.
    /// </summary>
    public async Task<IEnumerable<Produccion>> GetByEntidadMedicaAsync(int idEntidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica ORDER BY ID_PRODUCCION DESC";

        return await connection.QueryAsync<Produccion>(sql, new { IdEntidadMedica = idEntidadMedica });
    }

    /// <summary>
    /// Obtiene los registros de produccion de un periodo especifico.
    /// </summary>
    public async Task<IEnumerable<Produccion>> GetByPeriodoAsync(string periodo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"SELECT {SelectColumns} FROM SHM_PRODUCCION WHERE PERIODO = :Periodo ORDER BY ID_PRODUCCION DESC";

        return await connection.QueryAsync<Produccion>(sql, new { Periodo = periodo });
    }

    /// <summary>
    /// Crea un nuevo registro de produccion en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Produccion produccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_PRODUCCION (
                ID_PRODUCCION,
                ID_SEDE,
                ID_ENTIDAD_MEDICA,
                CODIGO_PRODUCCION,
                TIPO_PRODUCCION,
                TIPO_MEDICO,
                TIPO_RUBRO,
                DESCRIPCION,
                PERIODO,
                ESTADO_PRODUCCION,
                MTO_CONSUMO,
                MTO_DESCUENTO,
                MTO_SUBTOTAL,
                MTO_RENTA,
                MTO_IGV,
                MTO_TOTAL,
                TIPO_COMPROBANTE,
                SERIE,
                NUMERO,
                FECHA_EMISION,
                GLOSA,
                ESTADO_COMPROBANTE,
                CONCEPTO,
                FECHA_LIMITE,
                ESTADO,
                FACTURA_FECHA_SOLICITUD,
                FACTURA_FECHA_ENVIO,
                FACTURA_FECHA_ACEPTACION,
                FACTURA_FECHA_PAGO,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_PRODUCCION_SEQ.NEXTVAL,
                :IdSede,
                :IdEntidadMedica,
                :CodigoProduccion,
                :TipoProduccion,
                :TipoMedico,
                :TipoRubro,
                :Descripcion,
                :Periodo,
                :EstadoProduccion,
                :MtoConsumo,
                :MtoDescuento,
                :MtoSubtotal,
                :MtoRenta,
                :MtoIgv,
                :MtoTotal,
                :TipoComprobante,
                :Serie,
                :Numero,
                :FechaEmision,
                :Glosa,
                :EstadoComprobante,
                :Concepto,
                :FechaLimite,
                :Estado,
                :FacturaFechaSolicitud,
                :FacturaFechaEnvio,
                :FacturaFechaAceptacion,
                :FacturaFechaPago,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_PRODUCCION INTO :IdProduccion";

        var parameters = new DynamicParameters();
        parameters.Add("IdSede", produccion.IdSede);
        parameters.Add("IdEntidadMedica", produccion.IdEntidadMedica);
        parameters.Add("CodigoProduccion", produccion.CodigoProduccion);
        parameters.Add("TipoProduccion", produccion.TipoProduccion);
        parameters.Add("TipoMedico", produccion.TipoMedico);
        parameters.Add("TipoRubro", produccion.TipoRubro);
        parameters.Add("Descripcion", produccion.Descripcion);
        parameters.Add("Periodo", produccion.Periodo);
        parameters.Add("EstadoProduccion", produccion.EstadoProduccion);
        parameters.Add("MtoConsumo", produccion.MtoConsumo);
        parameters.Add("MtoDescuento", produccion.MtoDescuento);
        parameters.Add("MtoSubtotal", produccion.MtoSubtotal);
        parameters.Add("MtoRenta", produccion.MtoRenta);
        parameters.Add("MtoIgv", produccion.MtoIgv);
        parameters.Add("MtoTotal", produccion.MtoTotal);
        parameters.Add("TipoComprobante", produccion.TipoComprobante);
        parameters.Add("Serie", produccion.Serie);
        parameters.Add("Numero", produccion.Numero);
        parameters.Add("FechaEmision", produccion.FechaEmision);
        parameters.Add("Glosa", produccion.Glosa);
        parameters.Add("EstadoComprobante", produccion.EstadoComprobante);
        parameters.Add("Concepto", produccion.Concepto);
        parameters.Add("FechaLimite", produccion.FechaLimite);
        parameters.Add("Estado", produccion.Estado);
        parameters.Add("FacturaFechaSolicitud", produccion.FacturaFechaSolicitud);
        parameters.Add("FacturaFechaEnvio", produccion.FacturaFechaEnvio);
        parameters.Add("FacturaFechaAceptacion", produccion.FacturaFechaAceptacion);
        parameters.Add("FacturaFechaPago", produccion.FacturaFechaPago);
        parameters.Add("IdCreador", produccion.IdCreador);
        parameters.Add("IdProduccion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdProduccion");
    }

    /// <summary>
    /// Actualiza los datos de un registro de produccion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Produccion produccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PRODUCCION
            SET
                ID_SEDE = :IdSede,
                ID_ENTIDAD_MEDICA = :IdEntidadMedica,
                CODIGO_PRODUCCION = :CodigoProduccion,
                TIPO_PRODUCCION = :TipoProduccion,
                TIPO_MEDICO = :TipoMedico,
                TIPO_RUBRO = :TipoRubro,
                DESCRIPCION = :Descripcion,
                PERIODO = :Periodo,
                ESTADO_PRODUCCION = :EstadoProduccion,
                MTO_CONSUMO = :MtoConsumo,
                MTO_DESCUENTO = :MtoDescuento,
                MTO_SUBTOTAL = :MtoSubtotal,
                MTO_RENTA = :MtoRenta,
                MTO_IGV = :MtoIgv,
                MTO_TOTAL = :MtoTotal,
                TIPO_COMPROBANTE = :TipoComprobante,
                SERIE = :Serie,
                NUMERO = :Numero,
                FECHA_EMISION = :FechaEmision,
                GLOSA = :Glosa,
                ESTADO_COMPROBANTE = :EstadoComprobante,
                CONCEPTO = :Concepto,
                FECHA_LIMITE = :FechaLimite,
                ESTADO = :Estado,
                FACTURA_FECHA_SOLICITUD = :FacturaFechaSolicitud,
                FACTURA_FECHA_ENVIO = :FacturaFechaEnvio,
                FACTURA_FECHA_ACEPTACION = :FacturaFechaAceptacion,
                FACTURA_FECHA_PAGO = :FacturaFechaPago,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_PRODUCCION = :IdProduccion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdProduccion = id,
            produccion.IdSede,
            produccion.IdEntidadMedica,
            produccion.CodigoProduccion,
            produccion.TipoProduccion,
            produccion.TipoMedico,
            produccion.TipoRubro,
            produccion.Descripcion,
            produccion.Periodo,
            produccion.EstadoProduccion,
            produccion.MtoConsumo,
            produccion.MtoDescuento,
            produccion.MtoSubtotal,
            produccion.MtoRenta,
            produccion.MtoIgv,
            produccion.MtoTotal,
            produccion.TipoComprobante,
            produccion.Serie,
            produccion.Numero,
            produccion.FechaEmision,
            produccion.Glosa,
            produccion.EstadoComprobante,
            produccion.Concepto,
            produccion.FechaLimite,
            produccion.Estado,
            produccion.FacturaFechaSolicitud,
            produccion.FacturaFechaEnvio,
            produccion.FacturaFechaAceptacion,
            produccion.FacturaFechaPago,
            produccion.Activo,
            produccion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un registro de produccion del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PRODUCCION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_PRODUCCION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un registro de produccion con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_PRODUCCION WHERE ID_PRODUCCION = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

/// <summary>
    /// Verifica si existe un registro de produccion con la llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion).
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-19</created>
    /// </summary>
    public async Task<bool> ExistsByKeyAsync(int idSede, int idEntidadMedica, string codigoProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"SELECT COUNT(1) FROM SHM_PRODUCCION
                    WHERE ID_SEDE = :IdSede
                    AND ID_ENTIDAD_MEDICA = :IdEntidadMedica
                    AND CODIGO_PRODUCCION = :CodigoProduccion";

        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            IdSede = idSede,
            IdEntidadMedica = idEntidadMedica,
            CodigoProduccion = codigoProduccion
        });

        return count > 0;
    }
    
    /// <summary>
    /// Obtiene el listado paginado de producciones con datos relacionados y filtro por estado.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-20</created>
    /// </summary>
    public async Task<(IEnumerable<ProduccionListaResponseDto> Items, int TotalCount)> GetPaginatedListAsync(
        string? estado, int pageNumber, int pageSize)
    {
        using var connection = new OracleConnection(_connectionString);

        var whereClause = "WHERE p.ACTIVO = 1";
        if (!string.IsNullOrEmpty(estado))
        {
            whereClause += " AND p.ESTADO = :Estado";
        }

        // Query para obtener el total de registros
        var countSql = $@"
            SELECT COUNT(1)
            FROM SHM_PRODUCCION p
            {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Estado = estado });

        // Query principal con paginacion
        var offset = (pageNumber - 1) * pageSize;
        var sql = $@"
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
                p.FACTURA_FECHA_SOLICITUD AS FacturaFechaSolicitud,
                p.FACTURA_FECHA_ENVIO AS FacturaFechaEnvio,
                p.FACTURA_FECHA_ACEPTACION AS FacturaFechaAceptacion,
                p.FACTURA_FECHA_PAGO AS FacturaFechaPago,
                p.ACTIVO AS Activo,
                p.ID_CREADOR AS IdCreador,
                p.FECHA_CREACION AS FechaCreacion,
                p.ID_MODIFICADOR AS IdModificador,
                p.FECHA_MODIFICACION AS FechaModificacion,
                s.CODIGO AS CodigoSede,
                s.NOMBRE AS NombreSede,
                em.RUC AS Ruc,
                em.RAZON_SOCIAL AS RazonSocial,
                em.TIPO_ENTIDAD_MEDICA AS TipoEntidadMedica,
                tem.DESCRIPCION AS DesTipoEntidadMedica
            FROM SHM_PRODUCCION p
            LEFT JOIN SHM_SEDE s ON s.ID_SEDE = p.ID_SEDE
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN SHM_TABLA_DETALLE_VW tp ON tp.CODIGO_TABLA = 'TIPO_PRODUCCION' AND tp.CODIGO = p.TIPO_PRODUCCION
            LEFT JOIN SHM_TABLA_DETALLE_VW tm ON tm.CODIGO_TABLA = 'TIPO_MEDICO' AND tm.CODIGO = p.TIPO_MEDICO
            LEFT JOIN SHM_TABLA_DETALLE_VW tr ON tr.CODIGO_TABLA = 'TIPO_RUBRO' AND tr.CODIGO = p.TIPO_RUBRO
            LEFT JOIN SHM_TABLA_DETALLE_VW ep ON ep.CODIGO_TABLA = 'ESTADO_PROCESO' AND ep.CODIGO = p.ESTADO
            LEFT JOIN SHM_TABLA_DETALLE_VW tem ON tem.CODIGO_TABLA = 'TIPO_ENTIDAD_MEDICA' AND tem.CODIGO = em.TIPO_ENTIDAD_MEDICA
            {whereClause}
            ORDER BY p.CODIGO_PRODUCCION DESC
            OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

        var items = await connection.QueryAsync<ProduccionListaResponseDto>(sql, new
        {
            Estado = estado,
            Offset = offset,
            PageSize = pageSize
        });

        return (items, totalCount);
    }

    /// <summary>
    /// Obtiene una produccion por su GUID con datos relacionados (sede, entidad medica, descripciones).
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-21</created>
    /// </summary>
    public async Task<ProduccionListaResponseDto?> GetByGuidWithDetailsAsync(string guidRegistro)
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
                p.FACTURA_FECHA_SOLICITUD AS FacturaFechaSolicitud,
                p.FACTURA_FECHA_ENVIO AS FacturaFechaEnvio,
                p.FACTURA_FECHA_ACEPTACION AS FacturaFechaAceptacion,
                p.FACTURA_FECHA_PAGO AS FacturaFechaPago,
                p.ACTIVO AS Activo,
                p.ID_CREADOR AS IdCreador,
                p.FECHA_CREACION AS FechaCreacion,
                p.ID_MODIFICADOR AS IdModificador,
                p.FECHA_MODIFICACION AS FechaModificacion,
                s.CODIGO AS CodigoSede,
                s.NOMBRE AS NombreSede,
                em.RUC AS Ruc,
                em.RAZON_SOCIAL AS RazonSocial,
                em.TIPO_ENTIDAD_MEDICA AS TipoEntidadMedica,
                tem.DESCRIPCION AS DesTipoEntidadMedica
            FROM SHM_PRODUCCION p
            LEFT JOIN SHM_SEDE s ON s.ID_SEDE = p.ID_SEDE
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON em.ID_ENTIDAD_MEDICA = p.ID_ENTIDAD_MEDICA
            LEFT JOIN SHM_TABLA_DETALLE_VW tp ON tp.CODIGO_TABLA = 'TIPO_PRODUCCION' AND tp.CODIGO = p.TIPO_PRODUCCION
            LEFT JOIN SHM_TABLA_DETALLE_VW tm ON tm.CODIGO_TABLA = 'TIPO_MEDICO' AND tm.CODIGO = p.TIPO_MEDICO
            LEFT JOIN SHM_TABLA_DETALLE_VW tr ON tr.CODIGO_TABLA = 'TIPO_RUBRO' AND tr.CODIGO = p.TIPO_RUBRO
            LEFT JOIN SHM_TABLA_DETALLE_VW ep ON ep.CODIGO_TABLA = 'ESTADO_PROCESO' AND ep.CODIGO = p.ESTADO
            LEFT JOIN SHM_TABLA_DETALLE_VW tem ON tem.CODIGO_TABLA = 'TIPO_ENTIDAD_MEDICA' AND tem.CODIGO = em.TIPO_ENTIDAD_MEDICA
            WHERE p.GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<ProduccionListaResponseDto>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Actualiza la fecha limite y estado de una produccion para solicitud de factura.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-21</created>
    /// </summary>
    public async Task<bool> UpdateFechaLimiteEstadoAsync(string guidRegistro, DateTime fechaLimite, string estado, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PRODUCCION
            SET FECHA_LIMITE = :FechaLimite,
                ESTADO = :Estado,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE GUID_REGISTRO = :GuidRegistro";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            GuidRegistro = guidRegistro,
            FechaLimite = fechaLimite,
            Estado = estado,
            IdModificador = idModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Actualiza solo el estado de una produccion.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2025-01-22</created>
    /// </summary>
    public async Task<bool> UpdateEstadoAsync(string guidRegistro, string estado, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PRODUCCION
            SET ESTADO = :Estado,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE GUID_REGISTRO = :GuidRegistro";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            GuidRegistro = guidRegistro,
            Estado = estado,
            IdModificador = idModificador
        });

        return rowsAffected > 0;
    }
    
    /// Obtiene estadisticas del dashboard para una entidad medica.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-24</created>
    /// </summary>
    public async Task<(decimal TotalPorFacturar, int Pendientes, int Enviadas, int EnviadasHHMM, int Pagadas)> GetDashboardStatsAsync(int idEntidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                NVL(SUM(CASE WHEN ESTADO = 'FACTURA_SOLICITADA' THEN MTO_TOTAL ELSE 0 END), 0) AS TotalPorFacturar,
                COUNT(CASE WHEN ESTADO = 'FACTURA_SOLICITADA' THEN 1 END) AS Pendientes,
                COUNT(CASE WHEN ESTADO = 'FACTURA_ENVIADA' THEN 1 END) AS Enviadas,
                COUNT(CASE WHEN ESTADO = 'FACTURA_ENVIADA_HHMM' THEN 1 END) AS EnviadasHHMM,
                COUNT(CASE WHEN ESTADO = 'FACTURA_PAGADA' THEN 1 END) AS Pagadas
            FROM SHM_PRODUCCION
            WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica
            AND ACTIVO = 1";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { IdEntidadMedica = idEntidadMedica });

        return (
            result?.TOTALPORFACTURAR ?? 0m,
            (int)(result?.PENDIENTES ?? 0),
            (int)(result?.ENVIADAS ?? 0),
            (int)(result?.ENVIADASHHMM ?? 0),
            (int)(result?.PAGADAS ?? 0)
        );
    }


    /// <summary>
    /// Obtiene el conteo de facturas enviadas en el mes actual para una entidad medica.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-24</created>
    /// </summary>
    public async Task<int> GetFacturasEnviadasMesActualAsync(int idEntidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT COUNT(1)
            FROM SHM_PRODUCCION
            WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica
            AND ACTIVO = 1
            AND ESTADO IN ('FACTURA_ENVIADA', 'FACTURA_ENVIADA_HHMM', 'FACTURA_PAGADA')
            AND TRUNC(FECHA_EMISION, 'MM') = TRUNC(SYSDATE, 'MM')";

        return await connection.ExecuteScalarAsync<int>(sql, new { IdEntidadMedica = idEntidadMedica });
    }

    /// <summary>
    /// Obtiene datos de facturas por mes para los ultimos 6 meses.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-24</created>
    /// </summary>
    public async Task<IEnumerable<(int Anio, int Mes, int Enviadas, int Pendientes)>> GetFacturasPorMesAsync(int idEntidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            WITH MESES AS (
                SELECT ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -LEVEL + 1) AS MES_INICIO
                FROM DUAL
                CONNECT BY LEVEL <= 6
            )
            SELECT
                EXTRACT(YEAR FROM m.MES_INICIO) AS Anio,
                EXTRACT(MONTH FROM m.MES_INICIO) AS Mes,
                COUNT(CASE WHEN p.ESTADO IN ('FACTURA_ENVIADA', 'FACTURA_ENVIADA_HHMM', 'FACTURA_PAGADA') THEN 1 END) AS Enviadas,
                COUNT(CASE WHEN p.ESTADO = 'FACTURA_SOLICITADA' THEN 1 END) AS Pendientes
            FROM MESES m
            LEFT JOIN SHM_PRODUCCION p ON TRUNC(p.FECHA_CREACION, 'MM') = m.MES_INICIO
                AND p.ID_ENTIDAD_MEDICA = :IdEntidadMedica
                AND p.ACTIVO = 1
            GROUP BY m.MES_INICIO
            ORDER BY m.MES_INICIO ASC";

        var results = await connection.QueryAsync<dynamic>(sql, new { IdEntidadMedica = idEntidadMedica });

        return results.Select(r => (
            Anio: (int)r.ANIO,
            Mes: (int)r.MES,
            Enviadas: (int)r.ENVIADAS,
            Pendientes: (int)r.PENDIENTES
        ));
    }
}
