using System.Transactions;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de la relacion usuario-sede.
/// Utiliza Dapper con Oracle Database.
/// Implementa la estrategia DELETE ALL + INSERT para actualizaciones.
///
/// <author>Vladimir</author>
/// <created>2026-02-02</created>
/// </summary>
public class UsuarioSedeRepository : IUsuarioSedeRepository
{
    private readonly string _connectionString;

    public UsuarioSedeRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las sedes asignadas a un usuario.
    /// </summary>
    public async Task<IEnumerable<UsuarioSede>> GetByUsuarioIdAsync(int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                ID_SEDE as IdSede,
                GUID_REGISTRO as GuidRegistro,
                ES_ULTIMA_SEDE as EsUltimaSede,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion
            FROM SHM_SEG_USUARIO_SEDE
            WHERE ID_USUARIO = :IdUsuario
              AND ACTIVO = 1";

        return await connection.QueryAsync<UsuarioSede>(sql, new { IdUsuario = idUsuario });
    }

    /// <summary>
    /// Obtiene los IDs de las sedes asignadas a un usuario.
    /// </summary>
    public async Task<IEnumerable<int>> GetSedeIdsByUsuarioIdAsync(int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT ID_SEDE
            FROM SHM_SEG_USUARIO_SEDE
            WHERE ID_USUARIO = :IdUsuario
              AND ACTIVO = 1";

        return await connection.QueryAsync<int>(sql, new { IdUsuario = idUsuario });
    }

    /// <summary>
    /// Actualiza las sedes de un usuario usando la estrategia DELETE ALL + INSERT.
    /// Elimina todas las asignaciones previas e inserta las nuevas.
    /// Si existe una transaccion ambient (TransactionScope), se enlista automaticamente.
    /// Si no, crea su propia transaccion local.
    /// </summary>
    public async Task<bool> UpdateSedesUsuarioAsync(int idUsuario, IEnumerable<int> idsSedesSeleccionadas, int idCreador)
    {
        // Detectar si hay una transaccion ambient (TransactionScope)
        var hasAmbientTransaction = Transaction.Current != null;

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        // Solo crear transaccion local si NO hay TransactionScope
        OracleTransaction? transaction = null;
        if (!hasAmbientTransaction)
        {
            transaction = connection.BeginTransaction();
        }

        try
        {
            // 1. Eliminar todas las asignaciones previas del usuario
            var deleteSql = @"DELETE FROM SHM_SEG_USUARIO_SEDE WHERE ID_USUARIO = :IdUsuario";
            await connection.ExecuteAsync(deleteSql, new { IdUsuario = idUsuario }, transaction);

            // 2. Insertar las nuevas asignaciones
            var sedesList = idsSedesSeleccionadas.ToList();
            if (sedesList.Count > 0)
            {
                var insertSql = @"
                    INSERT INTO SHM_SEG_USUARIO_SEDE (
                        ID_USUARIO,
                        ID_SEDE,
                        GUID_REGISTRO,
                        ES_ULTIMA_SEDE,
                        ACTIVO,
                        ID_CREADOR,
                        FECHA_CREACION
                    ) VALUES (
                        :IdUsuario,
                        :IdSede,
                        SYS_GUID(),
                        :EsUltimaSede,
                        1,
                        :IdCreador,
                        SYSDATE
                    )";

                foreach (var idSede in sedesList)
                {
                    var parameters = new
                    {
                        IdUsuario = idUsuario,
                        IdSede = idSede,
                        EsUltimaSede = 0, // Se actualiza al ingresar al sistema
                        IdCreador = idCreador
                    };
                    await connection.ExecuteAsync(insertSql, parameters, transaction);
                }
            }

            // Solo hacer commit si usamos transaccion local
            transaction?.Commit();
            return true;
        }
        catch
        {
            // Solo hacer rollback si usamos transaccion local
            transaction?.Rollback();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    /// <summary>
    /// Obtiene la sede seleccionada para el usuario al iniciar sesion.
    /// Prioridad: ES_ULTIMA_SEDE = 1, si no existe toma el primer registro.
    /// Compatible con Oracle 11g (usa ROWNUM en lugar de FETCH FIRST).
    /// </summary>
    public async Task<(int IdSede, string NombreSede)?> GetSedeSeleccionadaLoginAsync(int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT IdSede, NombreSede FROM (
                SELECT
                    us.ID_SEDE as IdSede,
                    s.NOMBRE as NombreSede
                FROM SHM_SEG_USUARIO_SEDE us
                INNER JOIN SHM_SEDE s ON us.ID_SEDE = s.ID_SEDE
                WHERE us.ID_USUARIO = :IdUsuario
                  AND us.ACTIVO = 1
                  AND s.ACTIVO = 1
                ORDER BY us.ES_ULTIMA_SEDE DESC, us.ID_SEDE ASC
            ) WHERE ROWNUM = 1";

        var result = await connection.QueryFirstOrDefaultAsync<(int IdSede, string NombreSede)?>(sql, new { IdUsuario = idUsuario });

        return result;
    }
}
