using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de detalles de tablas maestras.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class TablaDetalleRepository : ITablaDetalleRepository
{
    private readonly string _connectionString;

    public TablaDetalleRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los detalles de tablas maestras.
    /// </summary>
    public async Task<IEnumerable<TablaDetalle>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA_DETALLE as IdTablaDetalle,
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ORDEN as Orden,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA_DETALLE
            ORDER BY ID_TABLA, ORDEN, ID_TABLA_DETALLE";

        return await connection.QueryAsync<TablaDetalle>(sql);
    }

    /// <summary>
    /// Obtiene los detalles de una tabla maestra especifica.
    /// </summary>
    public async Task<IEnumerable<TablaDetalle>> GetByTablaIdAsync(int idTabla)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA_DETALLE as IdTablaDetalle,
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ORDEN as Orden,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA_DETALLE
            WHERE ID_TABLA = :IdTabla
            ORDER BY ORDEN, ID_TABLA_DETALLE";

        return await connection.QueryAsync<TablaDetalle>(sql, new { IdTabla = idTabla });
    }

    /// <summary>
    /// Obtiene los detalles activos de una tabla maestra especifica.
    /// </summary>
    public async Task<IEnumerable<TablaDetalle>> GetActivosByTablaIdAsync(int idTabla)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA_DETALLE as IdTablaDetalle,
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ORDEN as Orden,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA_DETALLE
            WHERE ID_TABLA = :IdTabla AND ACTIVO = 1
            ORDER BY ORDEN, ID_TABLA_DETALLE";

        return await connection.QueryAsync<TablaDetalle>(sql, new { IdTabla = idTabla });
    }

    /// <summary>
    /// Obtiene un detalle de tabla maestra por su identificador unico.
    /// </summary>
    public async Task<TablaDetalle?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA_DETALLE as IdTablaDetalle,
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ORDEN as Orden,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA_DETALLE
            WHERE ID_TABLA_DETALLE = :Id";

        return await connection.QueryFirstOrDefaultAsync<TablaDetalle>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un detalle de tabla maestra por su identificador GUID.
    /// </summary>
    public async Task<TablaDetalle?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA_DETALLE as IdTablaDetalle,
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ORDEN as Orden,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA_DETALLE
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<TablaDetalle>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene un detalle de tabla maestra por su codigo dentro de una tabla.
    /// </summary>
    public async Task<TablaDetalle?> GetByCodigoAsync(int idTabla, string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_TABLA_DETALLE as IdTablaDetalle,
                ID_TABLA as IdTabla,
                CODIGO as Codigo,
                DESCRIPCION as Descripcion,
                ORDEN as Orden,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_TABLA_DETALLE
            WHERE ID_TABLA = :IdTabla AND CODIGO = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<TablaDetalle>(sql, new { IdTabla = idTabla, Codigo = codigo });
    }

    /// <summary>
    /// Crea un nuevo detalle de tabla maestra en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(TablaDetalle tablaDetalle)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_TABLA_DETALLE (
                ID_TABLA_DETALLE,
                ID_TABLA,
                CODIGO,
                DESCRIPCION,
                ORDEN,
                ACTIVO,
                GUID_REGISTRO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SGH_SEG_TABLA_DETALLE_SEQ.NEXTVAL,
                :IdTabla,
                :Codigo,
                :Descripcion,
                :Orden,
                1,
                SYS_GUID(),
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_TABLA_DETALLE INTO :IdTablaDetalle";

        var parameters = new DynamicParameters();
        parameters.Add("IdTabla", tablaDetalle.IdTabla);
        parameters.Add("Codigo", tablaDetalle.Codigo);
        parameters.Add("Descripcion", tablaDetalle.Descripcion);
        parameters.Add("Orden", tablaDetalle.Orden);
        parameters.Add("IdCreador", tablaDetalle.IdCreador);
        parameters.Add("IdTablaDetalle", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdTablaDetalle");
    }

    /// <summary>
    /// Actualiza los datos de un detalle de tabla maestra existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, TablaDetalle tablaDetalle)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_TABLA_DETALLE
            SET
                ID_TABLA = :IdTabla,
                CODIGO = :Codigo,
                DESCRIPCION = :Descripcion,
                ORDEN = :Orden,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_TABLA_DETALLE = :IdTablaDetalle";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdTablaDetalle = id,
            tablaDetalle.IdTabla,
            tablaDetalle.Codigo,
            tablaDetalle.Descripcion,
            tablaDetalle.Orden,
            tablaDetalle.Activo,
            tablaDetalle.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un detalle de tabla maestra del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_TABLA_DETALLE
            SET ACTIVO = 0,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_TABLA_DETALLE = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un detalle de tabla maestra con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_TABLA_DETALLE WHERE ID_TABLA_DETALLE = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
