using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de perfiles de aprobacion.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionRepository : IPerfilAprobacionRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            ID_PERFIL_APROBACION as IdPerfilAprobacion,
            GRUPO_FLUJO_TRABAJO as GrupoFlujoTrabajo,
            CODIGO as Codigo,
            DESCRIPCION as Descripcion,
            NIVEL as Nivel,
            ORDEN as Orden,
            GUID_REGISTRO as GuidRegistro,
            ACTIVO as Activo,
            ID_USUARIO_CREADOR as IdUsuarioCreador,
            FECHA_CREACION as FechaCreacion
        FROM SHM_PERFIL_APROBACION";

    public PerfilAprobacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todos los perfiles de aprobacion.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY ID_PERFIL_APROBACION DESC";

        return await connection.QueryAsync<PerfilAprobacion>(sql);
    }

    /// <summary>
    /// Obtiene todos los perfiles de aprobacion activos.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacion>> GetAllActiveAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE ACTIVO = 1
            ORDER BY ID_PERFIL_APROBACION DESC";

        return await connection.QueryAsync<PerfilAprobacion>(sql);
    }

    /// <summary>
    /// Obtiene un perfil de aprobacion por su identificador.
    /// </summary>
    public async Task<PerfilAprobacion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE ID_PERFIL_APROBACION = :Id";

        return await connection.QueryFirstOrDefaultAsync<PerfilAprobacion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un perfil de aprobacion por su GUID.
    /// </summary>
    public async Task<PerfilAprobacion?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<PerfilAprobacion>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Obtiene un perfil de aprobacion por su codigo.
    /// </summary>
    public async Task<PerfilAprobacion?> GetByCodigoAsync(string codigo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE CODIGO = :Codigo AND ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<PerfilAprobacion>(sql, new { Codigo = codigo });
    }

    /// <summary>
    /// Crea un nuevo perfil de aprobacion.
    /// </summary>
    public async Task<int> CreateAsync(PerfilAprobacion perfilAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_PERFIL_APROBACION (
                ID_PERFIL_APROBACION,
                GRUPO_FLUJO_TRABAJO,
                CODIGO,
                DESCRIPCION,
                NIVEL,
                ORDEN,
                GUID_REGISTRO,
                ACTIVO,
                ID_USUARIO_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_PERFIL_APROBACION_SEQ.NEXTVAL,
                :GrupoFlujoTrabajo,
                :Codigo,
                :Descripcion,
                :Nivel,
                :Orden,
                SYS_GUID(),
                1,
                :IdUsuarioCreador,
                SYSDATE
            )
            RETURNING ID_PERFIL_APROBACION INTO :IdPerfilAprobacion";

        var parameters = new DynamicParameters();
        parameters.Add("GrupoFlujoTrabajo", perfilAprobacion.GrupoFlujoTrabajo);
        parameters.Add("Codigo", perfilAprobacion.Codigo);
        parameters.Add("Descripcion", perfilAprobacion.Descripcion);
        parameters.Add("Nivel", perfilAprobacion.Nivel);
        parameters.Add("Orden", perfilAprobacion.Orden);
        parameters.Add("IdUsuarioCreador", perfilAprobacion.IdUsuarioCreador);
        parameters.Add("IdPerfilAprobacion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdPerfilAprobacion");
    }

    /// <summary>
    /// Actualiza un perfil de aprobacion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(PerfilAprobacion perfilAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PERFIL_APROBACION
            SET
                GRUPO_FLUJO_TRABAJO = :GrupoFlujoTrabajo,
                CODIGO = :Codigo,
                DESCRIPCION = :Descripcion,
                NIVEL = :Nivel,
                ORDEN = :Orden
            WHERE ID_PERFIL_APROBACION = :IdPerfilAprobacion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            perfilAprobacion.IdPerfilAprobacion,
            perfilAprobacion.GrupoFlujoTrabajo,
            perfilAprobacion.Codigo,
            perfilAprobacion.Descripcion,
            perfilAprobacion.Nivel,
            perfilAprobacion.Orden
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un perfil de aprobacion.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_PERFIL_APROBACION
            SET ACTIVO = 0
            WHERE ID_PERFIL_APROBACION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Obtiene los perfiles de aprobacion por grupo de flujo de trabajo.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacion>> GetByGrupoFlujoTrabajoAsync(string grupoFlujoTrabajo)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE GRUPO_FLUJO_TRABAJO = :GrupoFlujoTrabajo AND ACTIVO = 1
            ORDER BY ORDEN";

        return await connection.QueryAsync<PerfilAprobacion>(sql, new { GrupoFlujoTrabajo = grupoFlujoTrabajo });
    }
}
