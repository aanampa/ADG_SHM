using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de logs de email.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-25</created>
/// </summary>
public class EmailLogRepository : IEmailLogRepository
{
    private readonly string _connectionString;

    public EmailLogRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();
    }

    /// <summary>
    /// Crea un nuevo registro de log de email.
    /// </summary>
    public async Task<int> CreateAsync(EmailLog emailLog)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_EMAIL_LOG (
                ID_EMAIL_LOG,
                GUID_REGISTRO,
                EMAIL_DESTINO,
                NOMBRE_DESTINO,
                ASUNTO,
                TIPO_EMAIL,
                CONTENIDO,
                ES_HTML,
                ESTADO,
                MENSAJE_ERROR,
                ID_USUARIO,
                ID_ENTIDAD_MEDICA,
                ENTIDAD_REFERENCIA,
                ID_REFERENCIA,
                SERVIDOR_SMTP,
                IP_ORIGEN,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_EMAIL_LOG_SEQ.NEXTVAL,
                :GuidRegistro,
                :EmailDestino,
                :NombreDestino,
                :Asunto,
                :TipoEmail,
                :Contenido,
                :EsHtml,
                :Estado,
                :MensajeError,
                :IdUsuario,
                :IdEntidadMedica,
                :EntidadReferencia,
                :IdReferencia,
                :ServidorSmtp,
                :IpOrigen,
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_EMAIL_LOG INTO :IdEmailLog";

        var parameters = new DynamicParameters();
        parameters.Add("GuidRegistro", emailLog.GuidRegistro);
        parameters.Add("EmailDestino", emailLog.EmailDestino);
        parameters.Add("NombreDestino", emailLog.NombreDestino);
        parameters.Add("Asunto", emailLog.Asunto);
        parameters.Add("TipoEmail", emailLog.TipoEmail);
        parameters.Add("Contenido", emailLog.Contenido);
        parameters.Add("EsHtml", emailLog.EsHtml);
        parameters.Add("Estado", emailLog.Estado);
        parameters.Add("MensajeError", emailLog.MensajeError);
        parameters.Add("IdUsuario", emailLog.IdUsuario);
        parameters.Add("IdEntidadMedica", emailLog.IdEntidadMedica);
        parameters.Add("EntidadReferencia", emailLog.EntidadReferencia);
        parameters.Add("IdReferencia", emailLog.IdReferencia);
        parameters.Add("ServidorSmtp", emailLog.ServidorSmtp);
        parameters.Add("IpOrigen", emailLog.IpOrigen);
        parameters.Add("IdCreador", emailLog.IdCreador);
        parameters.Add("IdEmailLog", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdEmailLog");
    }

    /// <summary>
    /// Obtiene un log de email por su ID.
    /// </summary>
    public async Task<EmailLog?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_EMAIL_LOG AS IdEmailLog,
                GUID_REGISTRO AS GuidRegistro,
                EMAIL_DESTINO AS EmailDestino,
                NOMBRE_DESTINO AS NombreDestino,
                ASUNTO AS Asunto,
                TIPO_EMAIL AS TipoEmail,
                CONTENIDO AS Contenido,
                ES_HTML AS EsHtml,
                ESTADO AS Estado,
                MENSAJE_ERROR AS MensajeError,
                ID_USUARIO AS IdUsuario,
                ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                ENTIDAD_REFERENCIA AS EntidadReferencia,
                ID_REFERENCIA AS IdReferencia,
                SERVIDOR_SMTP AS ServidorSmtp,
                IP_ORIGEN AS IpOrigen,
                ACTIVO AS Activo,
                ID_CREADOR AS IdCreador,
                FECHA_CREACION AS FechaCreacion,
                ID_MODIFICADOR AS IdModificador,
                FECHA_MODIFICACION AS FechaModificacion
            FROM SHM_EMAIL_LOG
            WHERE ID_EMAIL_LOG = :Id";

        return await connection.QueryFirstOrDefaultAsync<EmailLog>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene todos los logs de email.
    /// </summary>
    public async Task<IEnumerable<EmailLog>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_EMAIL_LOG AS IdEmailLog,
                GUID_REGISTRO AS GuidRegistro,
                EMAIL_DESTINO AS EmailDestino,
                NOMBRE_DESTINO AS NombreDestino,
                ASUNTO AS Asunto,
                TIPO_EMAIL AS TipoEmail,
                CONTENIDO AS Contenido,
                ES_HTML AS EsHtml,
                ESTADO AS Estado,
                MENSAJE_ERROR AS MensajeError,
                ID_USUARIO AS IdUsuario,
                ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                ENTIDAD_REFERENCIA AS EntidadReferencia,
                ID_REFERENCIA AS IdReferencia,
                SERVIDOR_SMTP AS ServidorSmtp,
                IP_ORIGEN AS IpOrigen,
                ACTIVO AS Activo,
                ID_CREADOR AS IdCreador,
                FECHA_CREACION AS FechaCreacion,
                ID_MODIFICADOR AS IdModificador,
                FECHA_MODIFICACION AS FechaModificacion
            FROM SHM_EMAIL_LOG
            WHERE ACTIVO = 1
            ORDER BY FECHA_CREACION DESC";

        return await connection.QueryAsync<EmailLog>(sql);
    }

    /// <summary>
    /// Obtiene logs de email por tipo.
    /// </summary>
    public async Task<IEnumerable<EmailLog>> GetByTipoAsync(string tipoEmail)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_EMAIL_LOG AS IdEmailLog,
                GUID_REGISTRO AS GuidRegistro,
                EMAIL_DESTINO AS EmailDestino,
                NOMBRE_DESTINO AS NombreDestino,
                ASUNTO AS Asunto,
                TIPO_EMAIL AS TipoEmail,
                CONTENIDO AS Contenido,
                ES_HTML AS EsHtml,
                ESTADO AS Estado,
                MENSAJE_ERROR AS MensajeError,
                ID_USUARIO AS IdUsuario,
                ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                ENTIDAD_REFERENCIA AS EntidadReferencia,
                ID_REFERENCIA AS IdReferencia,
                SERVIDOR_SMTP AS ServidorSmtp,
                IP_ORIGEN AS IpOrigen,
                ACTIVO AS Activo,
                ID_CREADOR AS IdCreador,
                FECHA_CREACION AS FechaCreacion,
                ID_MODIFICADOR AS IdModificador,
                FECHA_MODIFICACION AS FechaModificacion
            FROM SHM_EMAIL_LOG
            WHERE ACTIVO = 1 AND TIPO_EMAIL = :TipoEmail
            ORDER BY FECHA_CREACION DESC";

        return await connection.QueryAsync<EmailLog>(sql, new { TipoEmail = tipoEmail });
    }

    /// <summary>
    /// Obtiene logs de email por estado.
    /// </summary>
    public async Task<IEnumerable<EmailLog>> GetByEstadoAsync(string estado)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_EMAIL_LOG AS IdEmailLog,
                GUID_REGISTRO AS GuidRegistro,
                EMAIL_DESTINO AS EmailDestino,
                NOMBRE_DESTINO AS NombreDestino,
                ASUNTO AS Asunto,
                TIPO_EMAIL AS TipoEmail,
                CONTENIDO AS Contenido,
                ES_HTML AS EsHtml,
                ESTADO AS Estado,
                MENSAJE_ERROR AS MensajeError,
                ID_USUARIO AS IdUsuario,
                ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                ENTIDAD_REFERENCIA AS EntidadReferencia,
                ID_REFERENCIA AS IdReferencia,
                SERVIDOR_SMTP AS ServidorSmtp,
                IP_ORIGEN AS IpOrigen,
                ACTIVO AS Activo,
                ID_CREADOR AS IdCreador,
                FECHA_CREACION AS FechaCreacion,
                ID_MODIFICADOR AS IdModificador,
                FECHA_MODIFICACION AS FechaModificacion
            FROM SHM_EMAIL_LOG
            WHERE ACTIVO = 1 AND ESTADO = :Estado
            ORDER BY FECHA_CREACION DESC";

        return await connection.QueryAsync<EmailLog>(sql, new { Estado = estado });
    }

    /// <summary>
    /// Obtiene el listado paginado de logs de email con filtros opcionales.
    /// Compatible con Oracle 11g (ROWNUM).
    /// </summary>
    public async Task<(IEnumerable<EmailLog> Items, int TotalCount)> GetPaginatedListAsync(
        string? tipoEmail, string? estado, string? emailDestino, int pageNumber, int pageSize)
    {
        using var connection = new OracleConnection(_connectionString);

        var whereClause = "WHERE ACTIVO = 1";
        if (!string.IsNullOrEmpty(tipoEmail))
            whereClause += " AND TIPO_EMAIL = :TipoEmail";
        if (!string.IsNullOrEmpty(estado))
            whereClause += " AND ESTADO = :Estado";
        if (!string.IsNullOrEmpty(emailDestino))
            whereClause += " AND UPPER(EMAIL_DESTINO) LIKE UPPER(:EmailDestino)";

        var countSql = $"SELECT COUNT(1) FROM SHM_EMAIL_LOG {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql,
            new { TipoEmail = tipoEmail, Estado = estado, EmailDestino = $"%{emailDestino}%" });

        var minRow = (pageNumber - 1) * pageSize;
        var maxRow = pageNumber * pageSize;

        var sql = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT
                        ID_EMAIL_LOG AS IdEmailLog,
                        GUID_REGISTRO AS GuidRegistro,
                        EMAIL_DESTINO AS EmailDestino,
                        NOMBRE_DESTINO AS NombreDestino,
                        ASUNTO AS Asunto,
                        TIPO_EMAIL AS TipoEmail,
                        ES_HTML AS EsHtml,
                        ESTADO AS Estado,
                        MENSAJE_ERROR AS MensajeError,
                        ID_USUARIO AS IdUsuario,
                        ENTIDAD_REFERENCIA AS EntidadReferencia,
                        ID_REFERENCIA AS IdReferencia,
                        SERVIDOR_SMTP AS ServidorSmtp,
                        ACTIVO AS Activo,
                        FECHA_CREACION AS FechaCreacion
                    FROM SHM_EMAIL_LOG
                    {whereClause}
                    ORDER BY ID_EMAIL_LOG DESC
                ) a WHERE ROWNUM <= :MaxRow
            ) WHERE rnum > :MinRow";

        var items = await connection.QueryAsync<EmailLog>(sql,
            new { TipoEmail = tipoEmail, Estado = estado, EmailDestino = $"%{emailDestino}%", MaxRow = maxRow, MinRow = minRow });

        return (items, totalCount);
    }

    /// <summary>
    /// Obtiene logs de email por destinatario.
    /// </summary>
    public async Task<IEnumerable<EmailLog>> GetByEmailDestinoAsync(string emailDestino)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_EMAIL_LOG AS IdEmailLog,
                GUID_REGISTRO AS GuidRegistro,
                EMAIL_DESTINO AS EmailDestino,
                NOMBRE_DESTINO AS NombreDestino,
                ASUNTO AS Asunto,
                TIPO_EMAIL AS TipoEmail,
                CONTENIDO AS Contenido,
                ES_HTML AS EsHtml,
                ESTADO AS Estado,
                MENSAJE_ERROR AS MensajeError,
                ID_USUARIO AS IdUsuario,
                ID_ENTIDAD_MEDICA AS IdEntidadMedica,
                ENTIDAD_REFERENCIA AS EntidadReferencia,
                ID_REFERENCIA AS IdReferencia,
                SERVIDOR_SMTP AS ServidorSmtp,
                IP_ORIGEN AS IpOrigen,
                ACTIVO AS Activo,
                ID_CREADOR AS IdCreador,
                FECHA_CREACION AS FechaCreacion,
                ID_MODIFICADOR AS IdModificador,
                FECHA_MODIFICACION AS FechaModificacion
            FROM SHM_EMAIL_LOG
            WHERE ACTIVO = 1 AND EMAIL_DESTINO = :EmailDestino
            ORDER BY FECHA_CREACION DESC";

        return await connection.QueryAsync<EmailLog>(sql, new { EmailDestino = emailDestino });
    }
}
