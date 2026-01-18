using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de sedes de la organizacion.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class SedeRepository : ISedeRepository
{
    private readonly string _connectionString;

    public SedeRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las sedes registradas en el sistema.
    /// </summary>
    public async Task<IEnumerable<Sede>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_SEDE as IdSede,
                ID_CORPORACION as IdCorporacion,
                CODIGO as Codigo,
                NOMBRE as Nombre,
                RUC as Ruc,
                DIRECCION as Direccion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEDE
            ORDER BY ID_SEDE";

        return await connection.QueryAsync<Sede>(sql);
    }

    /// <summary>
    /// Obtiene una sede por su identificador unico.
    /// </summary>
    public async Task<Sede?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_SEDE as IdSede,
                ID_CORPORACION as IdCorporacion,
                CODIGO as Codigo,
                NOMBRE as Nombre,
                RUC as Ruc,
                DIRECCION as Direccion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEDE
            WHERE ID_SEDE = :Id";

        return await connection.QueryFirstOrDefaultAsync<Sede>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una sede por su codigo.
    /// </summary>
    public async Task<Sede?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_SEDE as IdSede,
                ID_CORPORACION as IdCorporacion,
                CODIGO as Codigo,
                NOMBRE as Nombre,
                RUC as Ruc,
                DIRECCION as Direccion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEDE
            WHERE CODIGO = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<Sede>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Crea una nueva sede en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Sede sede)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_SEDE (
                ID_SEDE,
                ID_CORPORACION,
                CODIGO,
                NOMBRE,
                RUC,
                DIRECCION,
                ACTIVO,
                GUID_REGISTRO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_SEDE_SEQ.NEXTVAL,
                :IdCorporacion,
                :Codigo,
                :Nombre,
                :Ruc,
                :Direccion,
                1,
                SYS_GUID(),
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_SEDE INTO :IdSede";

        var parameters = new DynamicParameters();
        parameters.Add("IdCorporacion", sede.IdCorporacion);
        parameters.Add("Codigo", sede.Codigo);
        parameters.Add("Nombre", sede.Nombre);
        parameters.Add("Ruc", sede.Ruc);
        parameters.Add("Direccion", sede.Direccion);
        parameters.Add("IdCreador", sede.IdCreador);
        parameters.Add("IdSede", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdSede");
    }

    /// <summary>
    /// Actualiza los datos de una sede existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Sede sede)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEDE
            SET
                ID_CORPORACION = :IdCorporacion,
                CODIGO = :Codigo,
                NOMBRE = :Nombre,
                RUC = :Ruc,
                DIRECCION = :Direccion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_SEDE = :IdSede";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdSede = id,
            sede.IdCorporacion,
            sede.Codigo,
            sede.Nombre,
            sede.Ruc,
            sede.Direccion,
            sede.Activo,
            sede.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una sede del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEDE
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_SEDE = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una sede con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_SEDE WHERE ID_SEDE = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

    /// <summary>
    /// Obtiene una sede por su identificador GUID.
    /// </summary>
    public async Task<Sede?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_SEDE as IdSede,
                ID_CORPORACION as IdCorporacion,
                CODIGO as Codigo,
                NOMBRE as Nombre,
                RUC as Ruc,
                DIRECCION as Direccion,
                ACTIVO as Activo,
                GUID_REGISTRO as GuidRegistro,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEDE
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Sede>(sql, new { GuidRegistro = guidRegistro });
    }
}
