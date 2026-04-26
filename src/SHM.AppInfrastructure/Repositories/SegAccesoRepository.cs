using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para el log de accesos al sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-25</created>
/// </summary>
public class SegAccesoRepository : ISegAccesoRepository
{
    private readonly string _connectionString;

    public SegAccesoRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Registra un evento de acceso en la tabla SHM_SEG_ACCESO.
    /// </summary>
    public async Task<int> RegistrarAsync(SegAcceso acceso)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_SEG_ACCESO (
                ID_ACCESO,
                ID_USUARIO,
                LOGIN_INTENTO,
                TIPO_EVENTO,
                IP_ACCESO,
                USER_AGENT,
                DETALLE,
                GUID_REGISTRO,
                FECHA_ACCESO
            ) VALUES (
                SHM_SEG_ACCESO_SEQ.NEXTVAL,
                :IdUsuario,
                :LoginIntento,
                :TipoEvento,
                :IpAcceso,
                :UserAgent,
                :Detalle,
                :GuidRegistro,
                SYSDATE
            )";

        return await connection.ExecuteAsync(sql, new
        {
            acceso.IdUsuario,
            acceso.LoginIntento,
            acceso.TipoEvento,
            acceso.IpAcceso,
            UserAgent    = acceso.UserAgent?[..Math.Min(acceso.UserAgent.Length, 500)],
            Detalle      = acceso.Detalle?[..Math.Min(acceso.Detalle.Length, 500)],
            acceso.GuidRegistro
        });
    }

    /// <summary>
    /// Retorna la fecha del ultimo LOGIN_OK para cada usuario solicitado.
    /// </summary>
    public async Task<Dictionary<int, DateTime>> GetUltimosAccesosAsync(IEnumerable<int> idUsuarios)
    {
        var ids = idUsuarios.ToList();
        if (!ids.Any()) return new Dictionary<int, DateTime>();

        using var connection = new OracleConnection(_connectionString);

        // IDs son enteros internos — interpolacion segura
        var inClause = string.Join(",", ids);
        var sql = $@"
            SELECT ID_USUARIO   AS IdUsuario,
                   MAX(FECHA_ACCESO) AS FechaAcceso
            FROM   SHM_SEG_ACCESO
            WHERE  TIPO_EVENTO = 'LOGIN_OK'
              AND  ID_USUARIO IN ({inClause})
            GROUP BY ID_USUARIO";

        var rows = await connection.QueryAsync<UltimoAccesoRow>(sql);
        return rows.ToDictionary(r => r.IdUsuario, r => r.FechaAcceso);
    }

    private class UltimoAccesoRow
    {
        public int      IdUsuario   { get; set; }
        public DateTime FechaAcceso { get; set; }
    }
}
