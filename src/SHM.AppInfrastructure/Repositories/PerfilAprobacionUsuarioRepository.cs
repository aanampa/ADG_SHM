using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de relaciones perfil de aprobacion - usuario.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionUsuarioRepository : IPerfilAprobacionUsuarioRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            pau.ID_PERFIL_APROBACION as IdPerfilAprobacion,
            pau.ID_USUARIO as IdUsuario,
            pau.ID_SEDE as IdSede,
            u.NOMBRES || ' ' || u.APELLIDO_PATERNO as NombreUsuario,
            pa.DESCRIPCION as NombrePerfil,
            s.NOMBRE as NombreSede
        FROM SHM_PERFIL_APROBACION_USUARIO pau
        LEFT JOIN SHM_SEG_USUARIO u ON pau.ID_USUARIO = u.ID_USUARIO
        LEFT JOIN SHM_PERFIL_APROBACION pa ON pau.ID_PERFIL_APROBACION = pa.ID_PERFIL_APROBACION
        LEFT JOIN SHM_SEDE s ON pau.ID_SEDE = s.ID_SEDE";

    public PerfilAprobacionUsuarioRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las relaciones perfil-usuario.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionUsuario>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY pau.ID_PERFIL_APROBACION, pau.ID_USUARIO";

        return await connection.QueryAsync<PerfilAprobacionUsuario>(sql);
    }

    /// <summary>
    /// Obtiene una relacion por sus identificadores compuestos.
    /// </summary>
    public async Task<PerfilAprobacionUsuario?> GetByIdAsync(int idPerfilAprobacion, int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE pau.ID_PERFIL_APROBACION = :IdPerfilAprobacion
              AND pau.ID_USUARIO = :IdUsuario";

        return await connection.QueryFirstOrDefaultAsync<PerfilAprobacionUsuario>(sql,
            new { IdPerfilAprobacion = idPerfilAprobacion, IdUsuario = idUsuario });
    }

    /// <summary>
    /// Obtiene todos los usuarios de un perfil de aprobacion.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionUsuario>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE pau.ID_PERFIL_APROBACION = :IdPerfilAprobacion
            ORDER BY pau.ID_USUARIO";

        return await connection.QueryAsync<PerfilAprobacionUsuario>(sql,
            new { IdPerfilAprobacion = idPerfilAprobacion });
    }

    /// <summary>
    /// Obtiene todos los perfiles de aprobacion de un usuario.
    /// </summary>
    public async Task<IEnumerable<PerfilAprobacionUsuario>> GetByUsuarioIdAsync(int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE pau.ID_USUARIO = :IdUsuario
            ORDER BY pau.ID_PERFIL_APROBACION";

        return await connection.QueryAsync<PerfilAprobacionUsuario>(sql,
            new { IdUsuario = idUsuario });
    }

    /// <summary>
    /// Crea una nueva relacion perfil-usuario.
    /// </summary>
    public async Task<bool> CreateAsync(PerfilAprobacionUsuario perfilAprobacionUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_PERFIL_APROBACION_USUARIO (
                ID_PERFIL_APROBACION,
                ID_USUARIO,
                ID_SEDE
            ) VALUES (
                :IdPerfilAprobacion,
                :IdUsuario,
                :IdSede
            )";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            perfilAprobacionUsuario.IdPerfilAprobacion,
            perfilAprobacionUsuario.IdUsuario,
            perfilAprobacionUsuario.IdSede
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina una relacion perfil-usuario.
    /// </summary>
    public async Task<bool> DeleteAsync(int idPerfilAprobacion, int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            DELETE FROM SHM_PERFIL_APROBACION_USUARIO
            WHERE ID_PERFIL_APROBACION = :IdPerfilAprobacion
              AND ID_USUARIO = :IdUsuario";

        var rowsAffected = await connection.ExecuteAsync(sql,
            new { IdPerfilAprobacion = idPerfilAprobacion, IdUsuario = idUsuario });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe una relacion perfil-usuario.
    /// </summary>
    public async Task<bool> ExistsAsync(int idPerfilAprobacion, int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT COUNT(1) FROM SHM_PERFIL_APROBACION_USUARIO
            WHERE ID_PERFIL_APROBACION = :IdPerfilAprobacion
              AND ID_USUARIO = :IdUsuario";

        var count = await connection.ExecuteScalarAsync<int>(sql,
            new { IdPerfilAprobacion = idPerfilAprobacion, IdUsuario = idUsuario });

        return count > 0;
    }
}
