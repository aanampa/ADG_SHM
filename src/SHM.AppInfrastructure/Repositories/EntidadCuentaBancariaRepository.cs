using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de cuentas bancarias de entidades medicas.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadCuentaBancariaRepository : IEntidadCuentaBancariaRepository
{
    private readonly string _connectionString;

    public EntidadCuentaBancariaRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las cuentas bancarias de entidades medicas.
    /// </summary>
    public async Task<IEnumerable<EntidadCuentaBancaria>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_CUENTA_BANCO as IdCuentaBancaria,
                ID_ENTIDAD_MEDICA as IdEntidad,
                ID_BANCO as IdBanco,
                CUENTA_CORRIENTE as CuentaCorriente,
                CUENTA_CCI as CuentaCci,
                MONEDA as Moneda,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ENTIDAD_CUENTA_BANCO
            ORDER BY ID_CUENTA_BANCO";

        return await connection.QueryAsync<EntidadCuentaBancaria>(sql);
    }

    /// <summary>
    /// Obtiene una cuenta bancaria por su identificador unico.
    /// </summary>
    public async Task<EntidadCuentaBancaria?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_CUENTA_BANCO as IdCuentaBancaria,
                ID_ENTIDAD_MEDICA as IdEntidad,
                ID_BANCO as IdBanco,
                CUENTA_CORRIENTE as CuentaCorriente,
                CUENTA_CCI as CuentaCci,
                MONEDA as Moneda,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ENTIDAD_CUENTA_BANCO
            WHERE ID_CUENTA_BANCO = :Id";

        return await connection.QueryFirstOrDefaultAsync<EntidadCuentaBancaria>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene las cuentas bancarias de una entidad medica especifica.
    /// </summary>
    public async Task<IEnumerable<EntidadCuentaBancaria>> GetByEntidadIdAsync(int idEntidad)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_CUENTA_BANCO as IdCuentaBancaria,
                ID_ENTIDAD_MEDICA as IdEntidad,
                ID_BANCO as IdBanco,
                CUENTA_CORRIENTE as CuentaCorriente,
                CUENTA_CCI as CuentaCci,
                MONEDA as Moneda,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ENTIDAD_CUENTA_BANCO
            WHERE ID_ENTIDAD_MEDICA = :IdEntidad
            ORDER BY ID_CUENTA_BANCO";

        return await connection.QueryAsync<EntidadCuentaBancaria>(sql, new { IdEntidad = idEntidad });
    }

    /// <summary>
    /// Crea una nueva cuenta bancaria para una entidad medica.
    /// </summary>
    public async Task<int> CreateAsync(EntidadCuentaBancaria entidadCuentaBancaria)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ENTIDAD_CUENTA_BANCO (
                ID_CUENTA_BANCO,
                ID_ENTIDAD_MEDICA,
                ID_BANCO,
                CUENTA_CORRIENTE,
                CUENTA_CCI,
                MONEDA,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ENTIDAD_CUENTA_BANCO_SEQ.NEXTVAL,
                :IdEntidad,
                :IdBanco,
                :CuentaCorriente,
                :CuentaCci,
                :Moneda,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_CUENTA_BANCO INTO :IdCuentaBanco";

        var parameters = new DynamicParameters();
        parameters.Add("IdEntidad", entidadCuentaBancaria.IdEntidad);
        parameters.Add("IdBanco", entidadCuentaBancaria.IdBanco);
        parameters.Add("CuentaCorriente", entidadCuentaBancaria.CuentaCorriente);
        parameters.Add("CuentaCci", entidadCuentaBancaria.CuentaCci);
        parameters.Add("Moneda", entidadCuentaBancaria.Moneda);
        parameters.Add("IdCreador", entidadCuentaBancaria.IdCreador);
        parameters.Add("IdCuentaBanco", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdCuentaBanco");
    }

    /// <summary>
    /// Actualiza los datos de una cuenta bancaria existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, EntidadCuentaBancaria entidadCuentaBancaria)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ENTIDAD_CUENTA_BANCO
            SET
                ID_ENTIDAD_MEDICA = :IdEntidad,
                ID_BANCO = :IdBanco,
                CUENTA_CORRIENTE = :CuentaCorriente,
                CUENTA_CCI = :CuentaCci,
                MONEDA = :Moneda,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_CUENTA_BANCO = :IdCuentaBanco";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdCuentaBanco = id,
            entidadCuentaBancaria.IdEntidad,
            entidadCuentaBancaria.IdBanco,
            entidadCuentaBancaria.CuentaCorriente,
            entidadCuentaBancaria.CuentaCci,
            entidadCuentaBancaria.Moneda,
            entidadCuentaBancaria.Activo,
            entidadCuentaBancaria.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una cuenta bancaria del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ENTIDAD_CUENTA_BANCO
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_CUENTA_BANCO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una cuenta bancaria con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_ENTIDAD_CUENTA_BANCO WHERE ID_CUENTA_BANCO = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

    /// <summary>
    /// Obtiene una cuenta bancaria por su identificador GUID.
    /// </summary>
    public async Task<EntidadCuentaBancaria?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_CUENTA_BANCO as IdCuentaBancaria,
                ID_ENTIDAD_MEDICA as IdEntidad,
                ID_BANCO as IdBanco,
                CUENTA_CORRIENTE as CuentaCorriente,
                CUENTA_CCI as CuentaCci,
                MONEDA as Moneda,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ENTIDAD_CUENTA_BANCO
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<EntidadCuentaBancaria>(sql, new { GuidRegistro = guidRegistro });
    }
}
