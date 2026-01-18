using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de entidades medicas (clinicas, hospitales, consultorios).
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class EntidadMedicaRepository : IEntidadMedicaRepository
{
    private readonly string _connectionString;

    public EntidadMedicaRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las entidades medicas registradas en el sistema.
    /// </summary>
    public async Task<IEnumerable<EntidadMedica>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                CODIGO_ENTIDAD as CodigoEntidad,
                RAZON_SOCIAL as RazonSocial,
                RUC as Ruc,
                TIPO_ENTIDAD_MEDICA as TipoEntidadMedica,
                TELEFONO as Telefono,
                CELULAR as Celular,
                CODIGO_ACREEDOR as CodigoAcreedor,
                DIRECCION as Direccion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                FECHA_CREACION as FechaCreacion,
                ID_CREADOR as IdCreador,
                FECHA_MODIFICACION as FechaModificacion,
                ID_MODIFICADOR as IdModificador
            FROM SHM_ENTIDAD_MEDICA
            ORDER BY ID_ENTIDAD_MEDICA";

        return await connection.QueryAsync<EntidadMedica>(sql);
    }

    /// <summary>
    /// Obtiene una entidad medica por su identificador unico.
    /// </summary>
    public async Task<EntidadMedica?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                CODIGO_ENTIDAD as CodigoEntidad,
                RAZON_SOCIAL as RazonSocial,
                RUC as Ruc,
                TIPO_ENTIDAD_MEDICA as TipoEntidadMedica,
                TELEFONO as Telefono,
                CELULAR as Celular,
                CODIGO_ACREEDOR as CodigoAcreedor,
                DIRECCION as Direccion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                FECHA_CREACION as FechaCreacion,
                ID_CREADOR as IdCreador,
                FECHA_MODIFICACION as FechaModificacion,
                ID_MODIFICADOR as IdModificador
            FROM SHM_ENTIDAD_MEDICA
            WHERE ID_ENTIDAD_MEDICA = :Id";

        return await connection.QueryFirstOrDefaultAsync<EntidadMedica>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una entidad medica por su codigo.
    /// </summary>
    public async Task<EntidadMedica?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                CODIGO_ENTIDAD as CodigoEntidad,
                RAZON_SOCIAL as RazonSocial,
                RUC as Ruc,
                TIPO_ENTIDAD_MEDICA as TipoEntidadMedica,
                TELEFONO as Telefono,
                CELULAR as Celular,
                CODIGO_ACREEDOR as CodigoAcreedor,
                DIRECCION as Direccion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                FECHA_CREACION as FechaCreacion,
                ID_CREADOR as IdCreador,
                FECHA_MODIFICACION as FechaModificacion,
                ID_MODIFICADOR as IdModificador
            FROM SHM_ENTIDAD_MEDICA
            WHERE CODIGO_ENTIDAD = :Codigo";

        return await connection.QueryFirstOrDefaultAsync<EntidadMedica>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Obtiene una entidad medica por su numero de RUC.
    /// </summary>
    public async Task<EntidadMedica?> GetByRucAsync(string ruc)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                CODIGO_ENTIDAD as CodigoEntidad,
                RAZON_SOCIAL as RazonSocial,
                RUC as Ruc,
                TIPO_ENTIDAD_MEDICA as TipoEntidadMedica,
                TELEFONO as Telefono,
                CELULAR as Celular,
                CODIGO_ACREEDOR as CodigoAcreedor,
                DIRECCION as Direccion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                FECHA_CREACION as FechaCreacion,
                ID_CREADOR as IdCreador,
                FECHA_MODIFICACION as FechaModificacion,
                ID_MODIFICADOR as IdModificador
            FROM SHM_ENTIDAD_MEDICA
            WHERE RUC = :Ruc";

        return await connection.QueryFirstOrDefaultAsync<EntidadMedica>(sql, new { Ruc = ruc });
    }

    /// <summary>
    /// Crea una nueva entidad medica en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(EntidadMedica entidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ENTIDAD_MEDICA (
                ID_ENTIDAD_MEDICA,
                CODIGO_ENTIDAD,
                RAZON_SOCIAL,
                RUC,
                TIPO_ENTIDAD_MEDICA,
                TELEFONO,
                CELULAR,
                CODIGO_ACREEDOR,
                DIRECCION,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ENTIDAD_MEDICA_SEQ.NEXTVAL,
                :CodigoEntidad,
                :RazonSocial,
                :Ruc,
                :TipoEntidadMedica,
                :Telefono,
                :Celular,
                :CodigoAcreedor,
                :Direccion,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ENTIDAD_MEDICA INTO :IdEntidadMedica";

        var parameters = new DynamicParameters();
        parameters.Add("CodigoEntidad", entidadMedica.CodigoEntidad);
        parameters.Add("RazonSocial", entidadMedica.RazonSocial);
        parameters.Add("Ruc", entidadMedica.Ruc);
        parameters.Add("TipoEntidadMedica", entidadMedica.TipoEntidadMedica);
        parameters.Add("Telefono", entidadMedica.Telefono);
        parameters.Add("Celular", entidadMedica.Celular);
        parameters.Add("CodigoAcreedor", entidadMedica.CodigoAcreedor);
        parameters.Add("Direccion", entidadMedica.Direccion);
        parameters.Add("IdCreador", entidadMedica.IdCreador);
        parameters.Add("IdEntidadMedica", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdEntidadMedica");
    }

    /// <summary>
    /// Actualiza los datos de una entidad medica existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, EntidadMedica entidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ENTIDAD_MEDICA
            SET
                CODIGO_ENTIDAD = :CodigoEntidad,
                RAZON_SOCIAL = :RazonSocial,
                RUC = :Ruc,
                TIPO_ENTIDAD_MEDICA = :TipoEntidadMedica,
                TELEFONO = :Telefono,
                CELULAR = :Celular,
                CODIGO_ACREEDOR = :CodigoAcreedor,
                DIRECCION = :Direccion,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdEntidadMedica = id,
            entidadMedica.CodigoEntidad,
            entidadMedica.RazonSocial,
            entidadMedica.Ruc,
            entidadMedica.TipoEntidadMedica,
            entidadMedica.Telefono,
            entidadMedica.Celular,
            entidadMedica.CodigoAcreedor,
            entidadMedica.Direccion,
            entidadMedica.Activo,
            entidadMedica.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una entidad medica del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ENTIDAD_MEDICA
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ENTIDAD_MEDICA = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una entidad medica con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_ENTIDAD_MEDICA WHERE ID_ENTIDAD_MEDICA = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

    /// <summary>
    /// Obtiene una entidad medica por su identificador GUID.
    /// </summary>
    public async Task<EntidadMedica?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                CODIGO_ENTIDAD as CodigoEntidad,
                RAZON_SOCIAL as RazonSocial,
                RUC as Ruc,
                TIPO_ENTIDAD_MEDICA as TipoEntidadMedica,
                TELEFONO as Telefono,
                CELULAR as Celular,
                CODIGO_ACREEDOR as CodigoAcreedor,
                DIRECCION as Direccion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                FECHA_CREACION as FechaCreacion,
                ID_CREADOR as IdCreador,
                FECHA_MODIFICACION as FechaModificacion,
                ID_MODIFICADOR as IdModificador
            FROM SHM_ENTIDAD_MEDICA
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<EntidadMedica>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene entidades medicas de forma paginada con opcion de busqueda.
    /// </summary>
    public async Task<(IEnumerable<EntidadMedica> Items, int TotalCount)> GetPaginatedAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        using var connection = new OracleConnection(_connectionString);

        var whereClause = "WHERE ACTIVO = 1";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClause += @" AND (
                UPPER(RAZON_SOCIAL) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(RUC) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(CODIGO_ENTIDAD) LIKE '%' || UPPER(:SearchTerm) || '%')";
        }

        var countSql = $"SELECT COUNT(1) FROM SHM_ENTIDAD_MEDICA {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });

        var offset = (pageNumber - 1) * pageSize;
        var dataSql = $@"
            SELECT
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                CODIGO_ENTIDAD as CodigoEntidad,
                RAZON_SOCIAL as RazonSocial,
                RUC as Ruc,
                TIPO_ENTIDAD_MEDICA as TipoEntidadMedica,
                TELEFONO as Telefono,
                CELULAR as Celular,
                CODIGO_ACREEDOR as CodigoAcreedor,
                DIRECCION as Direccion,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                FECHA_CREACION as FechaCreacion,
                ID_CREADOR as IdCreador,
                FECHA_MODIFICACION as FechaModificacion,
                ID_MODIFICADOR as IdModificador
            FROM SHM_ENTIDAD_MEDICA
            {whereClause}
            ORDER BY RAZON_SOCIAL
            OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

        var items = await connection.QueryAsync<EntidadMedica>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }
}
