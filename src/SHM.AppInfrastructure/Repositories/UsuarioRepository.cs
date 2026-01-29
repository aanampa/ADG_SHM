using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio para la gestion de usuarios del sistema.
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexión de Oracle no está configurada.");
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados en el sistema.
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_USUARIO
            ORDER BY ID_USUARIO";

        return await connection.QueryAsync<Usuario>(sql);
    }

    /// <summary>
    /// Obtiene un usuario por su identificador unico.
    /// </summary>
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_USUARIO
            WHERE ID_USUARIO = :Id";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de login.
    /// </summary>
    public async Task<Usuario?> GetByLoginAsync(string login)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion,
                TOKEN_RECUPERACION as TokenRecuperacion,
                FECHA_EXPIRACION_TOKEN as FechaExpiracionToken
            FROM SHM_SEG_USUARIO
            WHERE UPPER(LOGIN) = UPPER(:Login)";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Login = login });
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    public async Task<int> CreateAsync(Usuario usuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            INSERT INTO SHM_SEG_USUARIO (
                ID_USUARIO,
                TIPO_USUARIO,
                LOGIN,
                PASSWORD,
                EMAIL,
                NUMERO_DOCUMENTO,
                NOMBRES,
                APELLIDO_PATERNO,
                APELLIDO_MATERNO,
                CELULAR,
                TELEFONO,
                CARGO,
                ID_ENTIDAD_MEDICA,
                ID_ROL,
                GUID_REGISTRO,
                ACTIVO,
                ID_CREADOR,
                FECHA_CREACION
            ) VALUES (
                SHM_SEG_USUARIO_SEQ.NEXTVAL,
                :TipoUsuario,
                :Login,
                :Password,
                :Email,
                :NumeroDocumento,
                :Nombres,
                :ApellidoPaterno,
                :ApellidoMaterno,
                :Celular,
                :Telefono,
                :Cargo,
                :IdEntidadMedica,
                :IdRol,
                SYS_GUID(),
                1,
                :IdCreador,
                SYSDATE
            )
            RETURNING ID_USUARIO INTO :IdUsuario";

        var parameters = new DynamicParameters();
        parameters.Add("TipoUsuario", usuario.TipoUsuario);
        parameters.Add("Login", usuario.Login);
        parameters.Add("Password", usuario.Password);
        parameters.Add("Email", usuario.Email);
        parameters.Add("NumeroDocumento", usuario.NumeroDocumento);
        parameters.Add("Nombres", usuario.Nombres);
        parameters.Add("ApellidoPaterno", usuario.ApellidoPaterno);
        parameters.Add("ApellidoMaterno", usuario.ApellidoMaterno);
        parameters.Add("Celular", usuario.Celular);
        parameters.Add("Telefono", usuario.Telefono);
        parameters.Add("Cargo", usuario.Cargo);
        parameters.Add("IdEntidadMedica", usuario.IdEntidadMedica);
        parameters.Add("IdRol", usuario.IdRol);
        parameters.Add("IdCreador", usuario.IdCreador);
        parameters.Add("IdUsuario", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        return parameters.Get<int>("IdUsuario");
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    public async Task<bool> UpdateAsync(int id, Usuario usuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET
                TIPO_USUARIO = :TipoUsuario,
                LOGIN = :Login,
                PASSWORD = :Password,
                EMAIL = :Email,
                NUMERO_DOCUMENTO = :NumeroDocumento,
                NOMBRES = :Nombres,
                APELLIDO_PATERNO = :ApellidoPaterno,
                APELLIDO_MATERNO = :ApellidoMaterno,
                CELULAR = :Celular,
                TELEFONO = :Telefono,
                CARGO = :Cargo,
                ID_ENTIDAD_MEDICA = :IdEntidadMedica,
                ID_ROL = :IdRol,
                ACTIVO = :Activo,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_USUARIO = :IdUsuario";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdUsuario = id,
            usuario.TipoUsuario,
            usuario.Login,
            usuario.Password,
            usuario.Email,
            usuario.NumeroDocumento,
            usuario.Nombres,
            usuario.ApellidoPaterno,
            usuario.ApellidoMaterno,
            usuario.Celular,
            usuario.Telefono,
            usuario.Cargo,
            usuario.IdEntidadMedica,
            usuario.IdRol,
            usuario.Activo,
            usuario.IdModificador
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina logicamente un usuario del sistema.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET ACTIVO = 0,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_USUARIO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Verifica si existe un usuario con el identificador especificado.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = "SELECT COUNT(1) FROM SHM_SEG_USUARIO WHERE ID_USUARIO = :Id";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }

    /// <summary>
    /// Obtiene un usuario por su direccion de correo electronico.
    /// </summary>
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion,
                TOKEN_RECUPERACION as TokenRecuperacion,
                FECHA_EXPIRACION_TOKEN as FechaExpiracionToken
            FROM SHM_SEG_USUARIO
            WHERE UPPER(EMAIL) = UPPER(:Email) AND ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
    }

    /// <summary>
    /// Obtiene un usuario por su token de recuperacion de contrasena.
    /// </summary>
    public async Task<Usuario?> GetByTokenRecuperacionAsync(string token)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion,
                TOKEN_RECUPERACION as TokenRecuperacion,
                FECHA_EXPIRACION_TOKEN as FechaExpiracionToken
            FROM SHM_SEG_USUARIO
            WHERE TOKEN_RECUPERACION = :Token AND ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Token = token });
    }

    /// <summary>
    /// Actualiza el token de recuperacion de contrasena de un usuario.
    /// </summary>
    public async Task<bool> UpdateTokenRecuperacionAsync(int idUsuario, string token, DateTime fechaExpiracion)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET TOKEN_RECUPERACION = :Token,
                FECHA_EXPIRACION_TOKEN = :FechaExpiracion
            WHERE ID_USUARIO = :IdUsuario";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdUsuario = idUsuario,
            Token = token,
            FechaExpiracion = fechaExpiracion
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Actualiza la contrasena de un usuario.
    /// </summary>
    public async Task<bool> UpdatePasswordAsync(int idUsuario, string newPasswordHash)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET PASSWORD = :Password,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_USUARIO = :IdUsuario";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdUsuario = idUsuario,
            Password = newPasswordHash
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Limpia el token de recuperacion de contrasena de un usuario.
    /// </summary>
    public async Task<bool> ClearTokenRecuperacionAsync(int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET TOKEN_RECUPERACION = NULL,
                FECHA_EXPIRACION_TOKEN = NULL
            WHERE ID_USUARIO = :IdUsuario";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdUsuario = idUsuario });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Obtiene las opciones de menu disponibles para un usuario segun su login.
    /// </summary>
    public async Task<IEnumerable<OpcionMenuDTO>> GetMenuByLoginAsync(string login)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT DISTINCT
                sso.ID_OPCION as IdOpcion,
                sso.NOMBRE as Nombre,
                sso.URL as Url,
                sso.ICONO as Icono,
                NVL(sso.ID_OPCION_PADRE, 0) as IdOpcionPadre,
                NVL(sso.ORDEN, 0) as Orden
            FROM SHM_SEG_ROL_OPCION ssro
            INNER JOIN SHM_SEG_OPCION sso ON ssro.ID_OPCION = sso.ID_OPCION
            INNER JOIN SHM_SEG_USUARIO ssu ON ssro.ID_ROL = ssu.ID_ROL
            WHERE UPPER(ssu.LOGIN) = UPPER(:Login)
              AND sso.ACTIVO = 1
              AND ssro.ACTIVO = 1
            UNION
            SELECT DISTINCT
                padre.ID_OPCION as IdOpcion,
                padre.NOMBRE as Nombre,
                padre.URL as Url,
                padre.ICONO as Icono,
                NVL(padre.ID_OPCION_PADRE, 0) as IdOpcionPadre,
                NVL(padre.ORDEN, 0) as Orden
            FROM SHM_SEG_ROL_OPCION ssro
            INNER JOIN SHM_SEG_OPCION sso ON ssro.ID_OPCION = sso.ID_OPCION
            INNER JOIN SHM_SEG_OPCION padre ON sso.ID_OPCION_PADRE = padre.ID_OPCION
            INNER JOIN SHM_SEG_USUARIO ssu ON ssro.ID_ROL = ssu.ID_ROL
            WHERE UPPER(ssu.LOGIN) = UPPER(:Login)
              AND sso.ACTIVO = 1
              AND ssro.ACTIVO = 1
              AND padre.ACTIVO = 1
            ORDER BY Orden, IdOpcion";

        return await connection.QueryAsync<OpcionMenuDTO>(sql, new { Login = login });
    }

    /// <summary>
    /// Actualiza los datos personales de un usuario desde el perfil.
    /// </summary>
    public async Task<bool> UpdateMisDatosAsync(int idUsuario, string? nombres, string? apellidoPaterno, string? apellidoMaterno, string? email, string? numeroDocumento, string? celular)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET NOMBRES = :Nombres,
                APELLIDO_PATERNO = :ApellidoPaterno,
                APELLIDO_MATERNO = :ApellidoMaterno,
                EMAIL = :Email,
                NUMERO_DOCUMENTO = :NumeroDocumento,
                CELULAR = :Celular,
                ID_MODIFICADOR = :IdUsuario,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_USUARIO = :IdUsuario";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            IdUsuario = idUsuario,
            Nombres = nombres,
            ApellidoPaterno = apellidoPaterno,
            ApellidoMaterno = apellidoMaterno,
            Email = email,
            NumeroDocumento = numeroDocumento,
            Celular = celular
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Obtiene un usuario por su identificador GUID.
    /// </summary>
    public async Task<Usuario?> GetByGuidAsync(string guidRegistro)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_USUARIO
            WHERE GUID_REGISTRO = :GuidRegistro";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { GuidRegistro = guidRegistro });
    }

    /// <summary>
    /// Obtiene usuarios externos de forma paginada con opcion de busqueda.
    /// </summary>
    public async Task<(IEnumerable<Usuario> Items, int TotalCount)> GetPaginatedExternosAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        using var connection = new OracleConnection(_connectionString);

        var whereClause = "WHERE u.TIPO_USUARIO = 'E' AND u.ACTIVO = 1";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClause += @" AND (
                UPPER(u.NOMBRES) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(u.APELLIDO_PATERNO) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(u.APELLIDO_MATERNO) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(u.LOGIN) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(u.EMAIL) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(u.NUMERO_DOCUMENTO) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(em.RAZON_SOCIAL) LIKE '%' || UPPER(:SearchTerm) || '%')";
        }

        var countSql = $@"
            SELECT COUNT(1)
            FROM SHM_SEG_USUARIO u
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON u.ID_ENTIDAD_MEDICA = em.ID_ENTIDAD_MEDICA
            {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });

        var offset = (pageNumber - 1) * pageSize;
        var dataSql = $@"
            SELECT
                u.ID_USUARIO as IdUsuario,
                u.TIPO_USUARIO as TipoUsuario,
                u.LOGIN as Login,
                u.PASSWORD as Password,
                u.EMAIL as Email,
                u.NUMERO_DOCUMENTO as NumeroDocumento,
                u.NOMBRES as Nombres,
                u.APELLIDO_PATERNO as ApellidoPaterno,
                u.APELLIDO_MATERNO as ApellidoMaterno,
                u.CELULAR as Celular,
                u.TELEFONO as Telefono,
                u.CARGO as Cargo,
                u.ID_ENTIDAD_MEDICA as IdEntidadMedica,
                u.ID_ROL as IdRol,
                u.GUID_REGISTRO as GuidRegistro,
                u.ACTIVO as Activo,
                u.ID_CREADOR as IdCreador,
                u.FECHA_CREACION as FechaCreacion,
                u.ID_MODIFICADOR as IdModificador,
                u.FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_USUARIO u
            LEFT JOIN SHM_ENTIDAD_MEDICA em ON u.ID_ENTIDAD_MEDICA = em.ID_ENTIDAD_MEDICA
            {whereClause}
            ORDER BY u.APELLIDO_PATERNO, u.NOMBRES
            OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

        var items = await connection.QueryAsync<Usuario>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }

    /// <summary>
    /// Obtiene usuarios internos de forma paginada con opcion de busqueda.
    /// </summary>
    public async Task<(IEnumerable<Usuario> Items, int TotalCount)> GetPaginatedInternosAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        using var connection = new OracleConnection(_connectionString);

        var whereClause = "WHERE TIPO_USUARIO = 'I' AND ACTIVO = 1";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClause += @" AND (
                UPPER(NOMBRES) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(APELLIDO_PATERNO) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(APELLIDO_MATERNO) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(LOGIN) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(EMAIL) LIKE '%' || UPPER(:SearchTerm) || '%' OR
                UPPER(NUMERO_DOCUMENTO) LIKE '%' || UPPER(:SearchTerm) || '%')";
        }

        var countSql = $@"
            SELECT COUNT(1)
            FROM SHM_SEG_USUARIO
            {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });

        var offset = (pageNumber - 1) * pageSize;
        var dataSql = $@"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                PASSWORD as Password,
                EMAIL as Email,
                NUMERO_DOCUMENTO as NumeroDocumento,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                CELULAR as Celular,
                TELEFONO as Telefono,
                CARGO as Cargo,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                ID_ROL as IdRol,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_SEG_USUARIO
            {whereClause}
            ORDER BY APELLIDO_PATERNO, NOMBRES
            OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

        var items = await connection.QueryAsync<Usuario>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }

    /// <summary>
    /// Elimina logicamente un usuario registrando quien realizo la eliminacion.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int idModificador)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            UPDATE SHM_SEG_USUARIO
            SET ACTIVO = 0,
                ID_MODIFICADOR = :IdModificador,
                FECHA_MODIFICACION = SYSDATE
            WHERE ID_USUARIO = :Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IdModificador = idModificador });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Obtiene los usuarios activos asociados a una entidad medica.
    ///
    /// <author>ADG Vladimir D</author>
    /// <created>2026-01-26</created>
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetByIdEntidadMedicaAsync(int idEntidadMedica)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_USUARIO as IdUsuario,
                TIPO_USUARIO as TipoUsuario,
                LOGIN as Login,
                EMAIL as Email,
                NOMBRES as Nombres,
                APELLIDO_PATERNO as ApellidoPaterno,
                APELLIDO_MATERNO as ApellidoMaterno,
                ID_ENTIDAD_MEDICA as IdEntidadMedica,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo
            FROM SHM_SEG_USUARIO
            WHERE ID_ENTIDAD_MEDICA = :IdEntidadMedica
              AND ACTIVO = 1
              AND EMAIL IS NOT NULL
            ORDER BY APELLIDO_PATERNO, NOMBRES";

        return await connection.QueryAsync<Usuario>(sql, new { IdEntidadMedica = idEntidadMedica });
    }
}
