using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para operaciones de acceso a datos de la entidad Archivo.
/// Implementa operaciones CRUD usando Dapper con Oracle.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// <modified>ADG Vladimir - 2026-01-29 - Agregado soporte para almacenamiento BLOB</modified>
/// </summary>
public class ArchivoRepository : IArchivoRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor del repositorio.
    /// </summary>
    public ArchivoRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los archivos ordenados por ID.
    /// </summary>
    public async Task<IEnumerable<Archivo>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                NOMBRE_ORIGINAL as NombreOriginal,
                NOMBRE_ARCHIVO as NombreArchivo,
                EXTENSION as Extension,
                TAMANO as Tamano,
                RUTA as Ruta,
                TIPO_ALMACENAMIENTO as TipoAlmacenamiento,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO
            ORDER BY ID_ARCHIVO";

        return await connection.QueryAsync<Archivo>(sql);
    }

    /// <summary>
    /// Obtiene un archivo por su ID.
    /// </summary>
    public async Task<Archivo?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                NOMBRE_ORIGINAL as NombreOriginal,
                NOMBRE_ARCHIVO as NombreArchivo,
                EXTENSION as Extension,
                TAMANO as Tamano,
                RUTA as Ruta,
                TIPO_ALMACENAMIENTO as TipoAlmacenamiento,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO
            WHERE ID_ARCHIVO = :Id";

        return await connection.QueryFirstOrDefaultAsync<Archivo>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un archivo por su GUID.
    /// </summary>
    public async Task<Archivo?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ARCHIVO as IdArchivo,
                TIPO_ARCHIVO as TipoArchivo,
                NOMBRE_ORIGINAL as NombreOriginal,
                NOMBRE_ARCHIVO as NombreArchivo,
                EXTENSION as Extension,
                TAMANO as Tamano,
                RUTA as Ruta,
                TIPO_ALMACENAMIENTO as TipoAlmacenamiento,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ARCHIVO
            WHERE GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<Archivo>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Crea un nuevo archivo en la base de datos (almacenamiento FILE).
    /// </summary>
    public async Task<int> CreateAsync(Archivo archivo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ARCHIVO (
                ID_ARCHIVO,
                TIPO_ARCHIVO,
                NOMBRE_ORIGINAL,
                NOMBRE_ARCHIVO,
                EXTENSION,
                TAMANO,
                RUTA,
                TIPO_ALMACENAMIENTO,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ARCHIVO_SEQ.NEXTVAL,
                :TipoArchivo,
                :NombreOriginal,
                :NombreArchivo,
                :Extension,
                :Tamano,
                :Ruta,
                'FILE',
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ARCHIVO INTO :IdArchivo";

        var parameters = new DynamicParameters();
        parameters.Add("TipoArchivo", archivo.TipoArchivo);
        parameters.Add("NombreOriginal", archivo.NombreOriginal);
        parameters.Add("NombreArchivo", archivo.NombreArchivo);
        parameters.Add("Extension", archivo.Extension);
        parameters.Add("Tamano", archivo.Tamano);
        parameters.Add("Ruta", archivo.Ruta);
        parameters.Add("IdCreador", archivo.IdCreador);
        parameters.Add("IdArchivo", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdArchivo");
    }

    /// <summary>
    /// Crea un nuevo archivo con contenido BLOB en la base de datos.
    /// </summary>
    public async Task<int> CreateWithBlobAsync(Archivo archivo, byte[] contenido)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO SHM_ARCHIVO (
                ID_ARCHIVO,
                TIPO_ARCHIVO,
                NOMBRE_ORIGINAL,
                NOMBRE_ARCHIVO,
                EXTENSION,
                TAMANO,
                RUTA,
                CONTENIDO_ARCHIVO,
                TIPO_ALMACENAMIENTO,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ARCHIVO_SEQ.NEXTVAL,
                :TipoArchivo,
                :NombreOriginal,
                :NombreArchivo,
                :Extension,
                :Tamano,
                NULL,
                :ContenidoArchivo,
                'BLOB',
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ARCHIVO INTO :IdArchivo";

        using var cmd = new OracleCommand(sql, connection);
        cmd.Parameters.Add("TipoArchivo", OracleDbType.Varchar2).Value = archivo.TipoArchivo ?? (object)DBNull.Value;
        cmd.Parameters.Add("NombreOriginal", OracleDbType.Varchar2).Value = archivo.NombreOriginal ?? (object)DBNull.Value;
        cmd.Parameters.Add("NombreArchivo", OracleDbType.Varchar2).Value = archivo.NombreArchivo ?? (object)DBNull.Value;
        cmd.Parameters.Add("Extension", OracleDbType.Varchar2).Value = archivo.Extension ?? (object)DBNull.Value;
        cmd.Parameters.Add("Tamano", OracleDbType.Int32).Value = archivo.Tamano ?? (object)DBNull.Value;
        cmd.Parameters.Add("ContenidoArchivo", OracleDbType.Blob).Value = contenido;
        cmd.Parameters.Add("IdCreador", OracleDbType.Int32).Value = archivo.IdCreador;

        var outputParam = cmd.Parameters.Add("IdArchivo", OracleDbType.Int32);
        outputParam.Direction = System.Data.ParameterDirection.Output;

        await cmd.ExecuteNonQueryAsync();

        return Convert.ToInt32(outputParam.Value.ToString());
    }

    /// <summary>
    /// Obtiene el contenido BLOB de un archivo.
    /// </summary>
    public async Task<byte[]?> GetBlobContentAsync(int idArchivo)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT CONTENIDO_ARCHIVO FROM SHM_ARCHIVO WHERE ID_ARCHIVO = :Id AND TIPO_ALMACENAMIENTO = 'BLOB'";

        using var cmd = new OracleCommand(sql, connection);
        cmd.Parameters.Add("Id", OracleDbType.Int32).Value = idArchivo;

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
            {
                var blob = reader.GetOracleBlob(0);
                var buffer = new byte[blob.Length];
                await blob.ReadAsync(buffer, 0, (int)blob.Length);
                return buffer;
            }
        }
        return null;
    }

    /// <summary>
    /// Actualiza un archivo existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Archivo archivo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ARCHIVO
            SET
                TIPO_ARCHIVO = :TipoArchivo,
                NOMBRE_ORIGINAL = :NombreOriginal,
                NOMBRE_ARCHIVO = :NombreArchivo,
                EXTENSION = :Extension,
                TAMANO = :Tamano,
                RUTA = :Ruta,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ARCHIVO = :IdArchivo";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdArchivo = id,
            archivo.TipoArchivo,
            archivo.NombreOriginal,
            archivo.NombreArchivo,
            archivo.Extension,
            archivo.Tamano,
            archivo.Ruta,
            archivo.Activo,
            archivo.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Realiza soft delete de un archivo (ACTIVO = 0).
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ARCHIVO
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ARCHIVO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un archivo con el ID especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_ARCHIVO WHERE ID_ARCHIVO = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
