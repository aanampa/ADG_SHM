using Dapper;
using Oracle.ManagedDataAccess.Client;
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
}
