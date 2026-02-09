using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de aprobaciones de ordenes de pago.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-03</created>
/// </summary>
public class OrdenPagoAprobacionRepository : IOrdenPagoAprobacionRepository
{
    private readonly string _connectionString;

    private const string SELECT_BASE = @"
        SELECT
            opa.ID_ORDEN_PAGO_APROBACION as IdOrdenPagoAprobacion,
            opa.ID_ORDEN_PAGO as IdOrdenPago,
            opa.ID_PERFIL_APROBACION as IdPerfilAprobacion,
            opa.ESTADO as Estado,
            opa.ID_USUARIO_APROBADOR as IdUsuarioAprobador,
            opa.FECHA_APROBACION as FechaAprobacion,
            opa.ORDEN as Orden,
            opa.GUID_REGISTRO as GuidRegistro,
            opa.ACTIVO as Activo,
            opa.ID_CREADOR as IdCreador,
            opa.FECHA_CREACION as FechaCreacion,
            opa.ID_MODIFICADOR as IdModificador,
            opa.FECHA_MODIFICACION as FechaModificacion,
            op.NUMERO_ORDEN_PAGO as NumeroOrdenPago,
            pa.DESCRIPCION as NombrePerfil,
            ua.NOMBRES || ' ' || ua.APELLIDO_PATERNO as NombreAprobador
        FROM SHM_ORDEN_PAGO_APROBACION opa
        LEFT JOIN SHM_ORDEN_PAGO op ON opa.ID_ORDEN_PAGO = op.ID_ORDEN_PAGO
        LEFT JOIN SHM_PERFIL_APROBACION pa ON opa.ID_PERFIL_APROBACION = pa.ID_PERFIL_APROBACION
        LEFT JOIN SHM_SEG_USUARIO ua ON opa.ID_USUARIO_APROBADOR = ua.ID_USUARIO";

    public OrdenPagoAprobacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de ordenes de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            ORDER BY opa.ID_ORDEN_PAGO_APROBACION DESC";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql);
    }

    /// <summary>
    /// Obtiene todas las aprobaciones activas.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetAllActiveAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ACTIVO = 1
            ORDER BY opa.ID_ORDEN_PAGO_APROBACION DESC";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql);
    }

    /// <summary>
    /// Obtiene una aprobacion por su identificador.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ORDEN_PAGO_APROBACION = :Id";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene una aprobacion por su GUID.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetByGuidAsync(string guid)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.GUID_REGISTRO = :Guid";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql, new { Guid = guid });
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ORDEN_PAGO = :IdOrdenPago AND opa.ACTIVO = 1
            ORDER BY opa.ORDEN";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql, new { IdOrdenPago = idOrdenPago });
    }

    /// <summary>
    /// Obtiene todas las aprobaciones de un perfil de aprobacion.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoAprobacion>> GetByPerfilAprobacionIdAsync(int idPerfilAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_PERFIL_APROBACION = :IdPerfilAprobacion AND opa.ACTIVO = 1
            ORDER BY opa.FECHA_CREACION DESC";

        return await connection.QueryAsync<OrdenPagoAprobacion>(sql, new { IdPerfilAprobacion = idPerfilAprobacion });
    }

    /// <summary>
    /// Obtiene una aprobacion especifica por orden de pago y perfil de aprobacion.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetByOrdenPagoAndPerfilAprobacionAsync(int idOrdenPago, int idPerfilAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            WHERE opa.ID_ORDEN_PAGO = :IdOrdenPago
              AND opa.ID_PERFIL_APROBACION = :IdPerfilAprobacion
              AND opa.ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql,
            new { IdOrdenPago = idOrdenPago, IdPerfilAprobacion = idPerfilAprobacion });
    }

    /// <summary>
    /// Obtiene la aprobacion pendiente de una orden de pago que corresponde al usuario.
    /// Valida perfil del usuario, coincidencia de sede y orden secuencial.
    /// </summary>
    public async Task<OrdenPagoAprobacion?> GetPendingByOrdenPagoForUserAsync(int idOrdenPago, int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = $@"{SELECT_BASE}
            INNER JOIN SHM_PERFIL_APROBACION_USUARIO pau ON opa.ID_PERFIL_APROBACION = pau.ID_PERFIL_APROBACION
                AND pau.ID_USUARIO = :IdUsuario
            INNER JOIN SHM_ORDEN_PAGO op2 ON opa.ID_ORDEN_PAGO = op2.ID_ORDEN_PAGO
            WHERE opa.ID_ORDEN_PAGO = :IdOrdenPago
              AND opa.ESTADO = 'PENDIENTE'
              AND opa.ACTIVO = 1
              AND (pau.ID_SEDE IS NULL OR pau.ID_SEDE = op2.ID_SEDE)
              AND NOT EXISTS (
                  SELECT 1 FROM SHM_ORDEN_PAGO_APROBACION opa2
                  WHERE opa2.ID_ORDEN_PAGO = opa.ID_ORDEN_PAGO
                    AND opa2.ACTIVO = 1
                    AND opa2.ORDEN < opa.ORDEN
                    AND opa2.ESTADO != 'APROBADO'
              )";

        return await connection.QueryFirstOrDefaultAsync<OrdenPagoAprobacion>(sql,
            new { IdOrdenPago = idOrdenPago, IdUsuario = idUsuario });
    }

    /// <summary>
    /// Aprueba una aprobacion: registra usuario aprobador y fecha.
    /// </summary>
    public async Task<bool> AprobarAsync(int idOrdenPagoAprobacion, int idUsuarioAprobador, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET ESTADO = 'APROBADO',
                ID_USUARIO_APROBADOR = :IdUsuarioAprobador,
                FECHA_APROBACION = SYSDATE,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_APROBACION = :IdOrdenPagoAprobacion
              AND ESTADO = 'PENDIENTE'";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdOrdenPagoAprobacion = idOrdenPagoAprobacion,
            IdUsuarioAprobador = idUsuarioAprobador,
            IdModificador = idModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Rechaza una aprobacion: registra usuario y cambia estado.
    /// </summary>
    public async Task<bool> RechazarAsync(int idOrdenPagoAprobacion, int idUsuarioAprobador, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET ESTADO = 'RECHAZADO',
                ID_USUARIO_APROBADOR = :IdUsuarioAprobador,
                FECHA_APROBACION = SYSDATE,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_APROBACION = :IdOrdenPagoAprobacion
              AND ESTADO = 'PENDIENTE'";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdOrdenPagoAprobacion = idOrdenPagoAprobacion,
            IdUsuarioAprobador = idUsuarioAprobador,
            IdModificador = idModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Crea una nueva aprobacion de orden de pago.
    /// </summary>
    public async Task<int> CreateAsync(OrdenPagoAprobacion ordenPagoAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_ORDEN_PAGO_APROBACION (
                ID_ORDEN_PAGO_APROBACION,
                ID_ORDEN_PAGO,
                ID_PERFIL_APROBACION,
                ESTADO,
                ORDEN,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_APROBACION_SEQ.NEXTVAL,
                :IdOrdenPago,
                :IdPerfilAprobacion,
                :Estado,
                :Orden,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_ORDEN_PAGO_APROBACION INTO :IdOrdenPagoAprobacion";

        var parameters = new DynamicParameters();
        parameters.Add("IdOrdenPago", ordenPagoAprobacion.IdOrdenPago);
        parameters.Add("IdPerfilAprobacion", ordenPagoAprobacion.IdPerfilAprobacion);
        parameters.Add("Orden", ordenPagoAprobacion.Orden);
        parameters.Add("Estado", ordenPagoAprobacion.Estado);
        parameters.Add("IdCreador", ordenPagoAprobacion.IdCreador);
        parameters.Add("IdOrdenPagoAprobacion", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdOrdenPagoAprobacion");
    }

    /// <summary>
    /// Actualiza una aprobacion existente.
    /// </summary>
    public async Task<bool> UpdateAsync(OrdenPagoAprobacion ordenPagoAprobacion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET
                ID_ORDEN_PAGO = :IdOrdenPago,
                ID_PERFIL_APROBACION = :IdPerfilAprobacion,
                ESTADO = :Estado,
                ID_USUARIO_APROBADOR = :IdUsuarioAprobador,
                FECHA_APROBACION = :FechaAprobacion,
                ORDEN = :Orden,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_APROBACION = :IdOrdenPagoAprobacion";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ordenPagoAprobacion.IdOrdenPagoAprobacion,
            ordenPagoAprobacion.IdOrdenPago,
            ordenPagoAprobacion.IdPerfilAprobacion,
            ordenPagoAprobacion.Estado,
            ordenPagoAprobacion.IdUsuarioAprobador,
            ordenPagoAprobacion.FechaAprobacion,
            ordenPagoAprobacion.Orden,
            ordenPagoAprobacion.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente una aprobacion.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO_APROBACION = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente todas las aprobaciones de una orden de pago.
    /// </summary>
    public async Task<bool> DeleteByOrdenPagoIdAsync(int idOrdenPago, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_ORDEN_PAGO_APROBACION
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_ORDEN_PAGO = :IdOrdenPago AND ACTIVO = 1";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdOrdenPago = idOrdenPago, IdModificador = idModificador });

        return rowsAffected > 0;
    }
}
