using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de archivos de comprobantes de produccion.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class ArchivoComprobanteRepository : IArchivoComprobanteRepository
{
    private readonly string _connectionString;

    public ArchivoComprobanteRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los archivos de comprobantes registrados.
    /// </summary>
    public async Task<IEnumerable<ArchivoComprobante>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO_COMPROBANTE as IdArchivoComprobante,
                ID_PRODUCCION as IdProduccion,
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO_COMPROBANTE
            ORDER BY ID_ARCHIVO_COMPROBANTE";

        return await connection.QueryAsync<ArchivoComprobante>(sql);
    }

    /// <summary>
    /// Obtiene un archivo de comprobante por su identificador unico.
    /// </summary>
    public async Task<ArchivoComprobante?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO_COMPROBANTE as IdArchivoComprobante,
                ID_PRODUCCION as IdProduccion,
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO_COMPROBANTE
            WHERE ID_ARCHIVO_COMPROBANTE = :Id";

        return await connection.QueryFirstOrDefaultAsync<ArchivoComprobante>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene los archivos de comprobantes de una produccion especifica.
    /// </summary>
    public async Task<IEnumerable<ArchivoComprobante>> GetByProduccionAsync(int idProduccion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO_COMPROBANTE as IdArchivoComprobante,
                ID_PRODUCCION as IdProduccion,
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO_COMPROBANTE
            WHERE ID_PRODUCCION = :IdProduccion
            ORDER BY ID_ARCHIVO_COMPROBANTE";

        return await connection.QueryAsync<ArchivoComprobante>(sql, new { IdProduccion = idProduccion });
    }

    /// <summary>
    /// Obtiene los archivos de comprobantes asociados a un archivo especifico.
    /// </summary>
    public async Task<IEnumerable<ArchivoComprobante>> GetByArchivoAsync(int idArchivo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO_COMPROBANTE as IdArchivoComprobante,
                ID_PRODUCCION as IdProduccion,
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                DESCRIPCION as Descripcion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO_COMPROBANTE
            WHERE ID_ARCHIVO = :IdArchivo
            ORDER BY ID_ARCHIVO_COMPROBANTE";

        return await connection.QueryAsync<ArchivoComprobante>(sql, new { IdArchivo = idArchivo });
    }

    /// <summary>
    /// Crea un nuevo archivo de comprobante en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(ArchivoComprobante archivoComprobante)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ARCHIVO_COMPROBANTE (
                ID_ARCHIVO_COMPROBANTE,
                ID_PRODUCCION,
                ID_ARCHIVO,
                TIPO_ARCHIVO,
                DESCRIPCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ARCHIVO_COMPROBANTE_SEQ.NEXTVAL,
                :IdProduccion,
                :IdArchivo,
                :TipoArchivo,
                :Descripcion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ARCHIVO_COMPROBANTE INTO :IdArchivoComprobante";

        var parameters = new DynamicParameters();
        parameters.Add("IdProduccion", archivoComprobante.IdProduccion);
        parameters.Add("IdArchivo", archivoComprobante.IdArchivo);
        parameters.Add("TipoArchivo", archivoComprobante.TipoArchivo);
        parameters.Add("Descripcion", archivoComprobante.Descripcion);
        parameters.Add("IdCreador", archivoComprobante.IdCreador);
        parameters.Add("IdArchivoComprobante", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdArchivoComprobante");
    }

    /// <summary>
    /// Actualiza los datos de un archivo de comprobante existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, ArchivoComprobante archivoComprobante)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ARCHIVO_COMPROBANTE
            SET
                ID_PRODUCCION = :IdProduccion,
                ID_ARCHIVO = :IdArchivo,
                TIPO_ARCHIVO = :TipoArchivo,
                DESCRIPCION = :Descripcion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ARCHIVO_COMPROBANTE = :IdArchivoComprobante";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdArchivoComprobante = id,
            archivoComprobante.IdProduccion,
            archivoComprobante.IdArchivo,
            archivoComprobante.TipoArchivo,
            archivoComprobante.Descripcion,
            archivoComprobante.Activo,
            archivoComprobante.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un archivo de comprobante del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ARCHIVO_COMPROBANTE
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ARCHIVO_COMPROBANTE = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un archivo de comprobante con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_ARCHIVO_COMPROBANTE WHERE ID_ARCHIVO_COMPROBANTE = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
