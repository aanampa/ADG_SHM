using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de opciones de menu del sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class OpcionRepository : IOpcionRepository
{
    private readonly string _connectionString;

    public OpcionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las opciones de menu del sistema.
    /// </summary>
    public async Task<IEnumerable<Opcion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_OPCION as IdOpcion,
                NOMBRE as Nombre,
                URL as Url,
                ICONO as Icono,
                ORDEN as Orden,
                ID_OPCION_PADRE as IdOpcionPadre,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_OPCION
            ORDER BY ORDEN, ID_OPCION";

        return await connection.QueryAsync<Opcion>(sql);
    }

    /// <summary>
    /// Obtiene una opcion de menu por su identificador unico.
    /// </summary>
    public async Task<Opcion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_OPCION as IdOpcion,
                NOMBRE as Nombre,
                URL as Url,
                ICONO as Icono,
                ORDEN as Orden,
                ID_OPCION_PADRE as IdOpcionPadre,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_OPCION
            WHERE ID_OPCION = :Id";

        return await connection.QueryFirstOrDefaultAsync<Opcion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una opcion de menu por su identificador GUID.
    /// </summary>
    public async Task<Opcion?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_OPCION as IdOpcion,
                NOMBRE as Nombre,
                URL as Url,
                ICONO as Icono,
                ORDEN as Orden,
                ID_OPCION_PADRE as IdOpcionPadre,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_OPCION
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Opcion>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene las opciones de menu hijas de una opcion padre.
    /// </summary>
    public async Task<IEnumerable<Opcion>> GetByPadreAsync(int? idOpcionPadre)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_OPCION as IdOpcion,
                NOMBRE as Nombre,
                URL as Url,
                ICONO as Icono,
                ORDEN as Orden,
                ID_OPCION_PADRE as IdOpcionPadre,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_OPCION
            WHERE (ID_OPCION_PADRE = :IdOpcionPadre OR (:IdOpcionPadre IS NULL AND ID_OPCION_PADRE IS NULL))
            ORDER BY ORDEN, ID_OPCION";

        return await connection.QueryAsync<Opcion>(sql, new { IdOpcionPadre = idOpcionPadre });
    }

    /// <summary>
    /// Crea una nueva opcion de menu en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Opcion opcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_SEG_OPCION (
                ID_OPCION,
                NOMBRE,
                URL,
                ICONO,
                ORDEN,
                ID_OPCION_PADRE,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_SEG_OPCION_SEQ.NEXTVAL,
                :Nombre,
                :Url,
                :Icono,
                :Orden,
                :IdOpcionPadre,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_OPCION INTO :IdOpcion";

        var parameters = new DynamicParameters();
        parameters.Add("Nombre", opcion.Nombre);
        parameters.Add("Url", opcion.Url);
        parameters.Add("Icono", opcion.Icono);
        parameters.Add("Orden", opcion.Orden);
        parameters.Add("IdOpcionPadre", opcion.IdOpcionPadre);
        parameters.Add("IdCreador", opcion.IdCreador);
        parameters.Add("IdOpcion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOpcion");
    }

    /// <summary>
    /// Actualiza los datos de una opcion de menu existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Opcion opcion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_OPCION
            SET
                NOMBRE = :Nombre,
                URL = :Url,
                ICONO = :Icono,
                ORDEN = :Orden,
                ID_OPCION_PADRE = :IdOpcionPadre,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_OPCION = :IdOpcion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdOpcion = id,
            opcion.Nombre,
            opcion.Url,
            opcion.Icono,
            opcion.Orden,
            opcion.IdOpcionPadre,
            opcion.Activo,
            opcion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una opcion de menu del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_OPCION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_OPCION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una opcion de menu con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_SEG_OPCION WHERE ID_OPCION = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
