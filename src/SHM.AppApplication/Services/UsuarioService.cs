using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.DTOs.Usuario;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la gestion de usuarios del sistema, incluyendo autenticacion, recuperacion de clave y administracion de datos de usuario
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema
    /// </summary>
    public async Task<IEnumerable<UsuarioResponseDto>> GetAllUsuariosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return usuarios.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un usuario por su identificador unico
    /// </summary>
    public async Task<UsuarioResponseDto?> GetUsuarioByIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        return usuario != null ? MapToResponseDto(usuario) : null;
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de login
    /// </summary>
    public async Task<UsuarioResponseDto?> GetUsuarioByLoginAsync(string login)
    {
        var usuario = await _usuarioRepository.GetByLoginAsync(login);
        return usuario != null ? MapToResponseDto(usuario) : null;
    }

    /// <summary>
    /// Valida las credenciales de un usuario para autenticacion
    /// </summary>
    public async Task<UsuarioResponseDto?> ValidarCredencialesAsync(string login, string password)
    {
        var usuario = await _usuarioRepository.GetByLoginAsync(login);

        if (usuario == null)
            return null;

        // Verificar que el usuario esté activo
        if (usuario.Activo != 1)
            return null;

        // Verificar la contraseña con BCrypt
        // TODO: Descomentar cuando las claves estén cifradas en BD
        // if (!BCrypt.Net.BCrypt.Verify(password, usuario.Password))
        //     return null;

        // Validación temporal para desarrollo (clave sin cifrar)
        if (password != usuario.Password)
            return null;

        return MapToResponseDto(usuario);
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema
    /// </summary>
    public async Task<UsuarioResponseDto> CreateUsuarioAsync(CreateUsuarioDto createDto, int idCreador)
    {
        var usuario = new Usuario
        {
            TipoUsuario = createDto.TipoUsuario,
            Login = createDto.Login,
            Password = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
            Email = createDto.Email,
            NumeroDocumento = createDto.NumeroDocumento,
            Nombres = createDto.Nombres,
            ApellidoPaterno = createDto.ApellidoPaterno,
            ApellidoMaterno = createDto.ApellidoMaterno,
            Celular = createDto.Celular,
            Telefono = createDto.Telefono,
            Cargo = createDto.Cargo,
            IdEntidadMedica = createDto.IdEntidadMedica,
            IdRol = createDto.IdRol,
            IdCreador = idCreador,
            Activo = 1
        };

        var idUsuario = await _usuarioRepository.CreateAsync(usuario);
        var createdUsuario = await _usuarioRepository.GetByIdAsync(idUsuario);

        return MapToResponseDto(createdUsuario!);
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente
    /// </summary>
    public async Task<bool> UpdateUsuarioAsync(int id, UpdateUsuarioDto updateDto, int idModificador)
    {
        var usuarioExistente = await _usuarioRepository.GetByIdAsync(id);
        if (usuarioExistente == null)
            return false;

        if (!string.IsNullOrEmpty(updateDto.TipoUsuario))
            usuarioExistente.TipoUsuario = updateDto.TipoUsuario;

        if (!string.IsNullOrEmpty(updateDto.Login))
            usuarioExistente.Login = updateDto.Login;

        if (!string.IsNullOrEmpty(updateDto.Password))
            usuarioExistente.Password = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);

        if (updateDto.Email != null)
            usuarioExistente.Email = updateDto.Email;

        if (updateDto.NumeroDocumento != null)
            usuarioExistente.NumeroDocumento = updateDto.NumeroDocumento;

        if (updateDto.Nombres != null)
            usuarioExistente.Nombres = updateDto.Nombres;

        if (updateDto.ApellidoPaterno != null)
            usuarioExistente.ApellidoPaterno = updateDto.ApellidoPaterno;

        if (updateDto.ApellidoMaterno != null)
            usuarioExistente.ApellidoMaterno = updateDto.ApellidoMaterno;

        if (updateDto.Celular != null)
            usuarioExistente.Celular = updateDto.Celular;

        if (updateDto.Telefono != null)
            usuarioExistente.Telefono = updateDto.Telefono;

        if (updateDto.Cargo != null)
            usuarioExistente.Cargo = updateDto.Cargo;

        if (updateDto.IdEntidadMedica.HasValue)
            usuarioExistente.IdEntidadMedica = updateDto.IdEntidadMedica;

        if (updateDto.IdRol.HasValue)
            usuarioExistente.IdRol = updateDto.IdRol;

        if (updateDto.Activo.HasValue)
            usuarioExistente.Activo = updateDto.Activo.Value;

        usuarioExistente.IdModificador = idModificador;

        return await _usuarioRepository.UpdateAsync(id, usuarioExistente);
    }

    /// <summary>
    /// Elimina un usuario por su identificador
    /// </summary>
    public async Task<bool> DeleteUsuarioAsync(int id)
    {
        var exists = await _usuarioRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _usuarioRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Genera un token de recuperacion de clave para un usuario
    /// </summary>
    public async Task<(bool Success, string? Token, string? Email, string? Nombres)> GenerarTokenRecuperacionAsync(string emailOrLogin)
    {
        // Buscar usuario por login o email
        var usuario = await _usuarioRepository.GetByLoginAsync(emailOrLogin);
        if (usuario == null)
        {
            usuario = await _usuarioRepository.GetByEmailAsync(emailOrLogin);
        }

        if (usuario == null || usuario.Activo != 1)
        {
            return (false, null, null, null);
        }

        if (string.IsNullOrEmpty(usuario.Email))
        {
            return (false, null, null, null);
        }

        // Generar token único
        var token = Guid.NewGuid().ToString("N");
        var fechaExpiracion = DateTime.Now.AddHours(1); // Token válido por 1 hora

        // Guardar token en la base de datos
        var updated = await _usuarioRepository.UpdateTokenRecuperacionAsync(usuario.IdUsuario, token, fechaExpiracion);

        if (!updated)
        {
            return (false, null, null, null);
        }

        var nombreCompleto = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();

        return (true, token, usuario.Email, nombreCompleto);
    }

    /// <summary>
    /// Valida un token de recuperacion de clave
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> ValidarTokenRecuperacionAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return (false, "Token inválido");
        }

        var usuario = await _usuarioRepository.GetByTokenRecuperacionAsync(token);

        if (usuario == null)
        {
            return (false, "El enlace de recuperación no es válido o ya fue utilizado");
        }

        if (usuario.FechaExpiracionToken < DateTime.Now)
        {
            // Limpiar token expirado
            await _usuarioRepository.ClearTokenRecuperacionAsync(usuario.IdUsuario);
            return (false, "El enlace de recuperación ha expirado. Por favor solicita uno nuevo");
        }

        return (true, null);
    }

    /// <summary>
    /// Restablece la clave de un usuario utilizando un token de recuperacion
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> RestablecerClaveAsync(string token, string nuevaPassword)
    {
        // Validar token
        var (isValid, errorMessage) = await ValidarTokenRecuperacionAsync(token);
        if (!isValid)
        {
            return (false, errorMessage);
        }

        var usuario = await _usuarioRepository.GetByTokenRecuperacionAsync(token);
        if (usuario == null)
        {
            return (false, "Token inválido");
        }

        // Hashear nueva contraseña
        // TODO: Descomentar cuando se use BCrypt en producción
        // var passwordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
        var passwordHash = nuevaPassword; // Temporal para desarrollo

        // Actualizar contraseña
        var updated = await _usuarioRepository.UpdatePasswordAsync(usuario.IdUsuario, passwordHash);
        if (!updated)
        {
            return (false, "Error al actualizar la contraseña");
        }

        // Limpiar token
        await _usuarioRepository.ClearTokenRecuperacionAsync(usuario.IdUsuario);

        return (true, null);
    }

    /// <summary>
    /// Lista las opciones del menu disponibles para un usuario por su login
    /// </summary>
    public async Task<IEnumerable<OpcionMenuDTO>> ListarMenuPorLoginAsync(string login)
    {
        return await _usuarioRepository.GetMenuByLoginAsync(login);
    }

    /// <summary>
    /// Actualiza los datos personales del usuario actual
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> ActualizarMisDatosAsync(int idUsuario, string? nombres, string? apellidoPaterno, string? apellidoMaterno, string? email, string? numeroDocumento, string? celular)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        // Validar que el email no esté en uso por otro usuario
        if (!string.IsNullOrEmpty(email) && email != usuario.Email)
        {
            var usuarioConEmail = await _usuarioRepository.GetByEmailAsync(email);
            if (usuarioConEmail != null && usuarioConEmail.IdUsuario != idUsuario)
            {
                return (false, "El correo electrónico ya está en uso por otro usuario");
            }
        }

        var updated = await _usuarioRepository.UpdateMisDatosAsync(idUsuario, nombres, apellidoPaterno, apellidoMaterno, email, numeroDocumento, celular);

        if (!updated)
        {
            return (false, "Error al actualizar los datos");
        }

        return (true, null);
    }

    /// <summary>
    /// Cambia la contrasena del usuario actual validando la contrasena actual
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> CambiarPasswordAsync(int idUsuario, string passwordActual, string passwordNueva)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        // Verificar contraseña actual
        // TODO: Descomentar cuando las claves estén cifradas en BD
        // if (!BCrypt.Net.BCrypt.Verify(passwordActual, usuario.Password))
        //     return (false, "La contraseña actual es incorrecta");

        // Validación temporal para desarrollo (clave sin cifrar)
        if (passwordActual != usuario.Password)
        {
            return (false, "La contraseña actual es incorrecta");
        }

        // Hashear nueva contraseña
        // TODO: Descomentar cuando se use BCrypt en producción
        // var passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
        var passwordHash = passwordNueva; // Temporal para desarrollo

        var updated = await _usuarioRepository.UpdatePasswordAsync(idUsuario, passwordHash);
        if (!updated)
        {
            return (false, "Error al actualizar la contraseña");
        }

        return (true, null);
    }

    /// <summary>
    /// Obtiene un usuario por su GUID de registro
    /// </summary>
    public async Task<UsuarioResponseDto?> GetUsuarioByGuidAsync(string guidRegistro)
    {
        var usuario = await _usuarioRepository.GetByGuidAsync(guidRegistro);
        return usuario != null ? MapToResponseDto(usuario) : null;
    }

    /// <summary>
    /// Obtiene usuarios externos paginados con opcion de busqueda
    /// </summary>
    public async Task<(IEnumerable<UsuarioResponseDto> Items, int TotalCount)> GetPaginatedExternosAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _usuarioRepository.GetPaginatedExternosAsync(searchTerm, pageNumber, pageSize);
        var dtos = items.Select(MapToResponseDto);
        return (dtos, totalCount);
    }

    /// <summary>
    /// Elimina un usuario por su identificador registrando el modificador
    /// </summary>
    public async Task<bool> DeleteUsuarioAsync(int id, int idModificador)
    {
        var exists = await _usuarioRepository.ExistsAsync(id);
        if (!exists)
            return false;

        return await _usuarioRepository.DeleteAsync(id, idModificador);
    }

    /// <summary>
    /// Crea un usuario externo con generacion automatica de clave
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> CreateUsuarioExternoAsync(CreateUsuarioDto createDto, int idCreador, bool enviarCorreo)
    {
        // Verificar si el login ya existe
        var existeLogin = await _usuarioRepository.GetByLoginAsync(createDto.Login);
        if (existeLogin != null)
        {
            return (false, "Ya existe un usuario con el mismo login", null);
        }

        // Verificar si el email ya existe
        if (!string.IsNullOrEmpty(createDto.Email))
        {
            var existeEmail = await _usuarioRepository.GetByEmailAsync(createDto.Email);
            if (existeEmail != null)
            {
                return (false, "Ya existe un usuario con el mismo correo electronico", null);
            }
        }

        // Generar clave aleatoria si no se proporciona
        var generatedPassword = string.IsNullOrEmpty(createDto.Password)
            ? GenerarClaveAleatoria()
            : createDto.Password;

        var usuario = new Usuario
        {
            TipoUsuario = "E", // Siempre externo
            Login = createDto.Login,
            Password = generatedPassword, // TODO: BCrypt.Net.BCrypt.HashPassword(generatedPassword)
            Email = createDto.Email,
            NumeroDocumento = createDto.NumeroDocumento,
            Nombres = createDto.Nombres,
            ApellidoPaterno = createDto.ApellidoPaterno,
            ApellidoMaterno = createDto.ApellidoMaterno,
            Celular = createDto.Celular,
            Telefono = createDto.Telefono,
            Cargo = createDto.Cargo,
            IdEntidadMedica = createDto.IdEntidadMedica,
            IdRol = createDto.IdRol,
            IdCreador = idCreador,
            Activo = 1
        };

        var idUsuario = await _usuarioRepository.CreateAsync(usuario);

        // TODO: Si enviarCorreo es true, enviar email con credenciales

        return (true, null, generatedPassword);
    }

    /// <summary>
    /// Obtiene usuarios internos paginados con opcion de busqueda
    /// </summary>
    public async Task<(IEnumerable<UsuarioResponseDto> Items, int TotalCount)> GetPaginatedInternosAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _usuarioRepository.GetPaginatedInternosAsync(searchTerm, pageNumber, pageSize);
        var dtos = items.Select(MapToResponseDto);
        return (dtos, totalCount);
    }

    /// <summary>
    /// Crea un usuario interno con generacion automatica de clave
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> CreateUsuarioInternoAsync(CreateUsuarioDto createDto, int idCreador, bool enviarCorreo)
    {
        // Verificar si el login ya existe
        var existeLogin = await _usuarioRepository.GetByLoginAsync(createDto.Login);
        if (existeLogin != null)
        {
            return (false, "Ya existe un usuario con el mismo login", null);
        }

        // Verificar si el email ya existe
        if (!string.IsNullOrEmpty(createDto.Email))
        {
            var existeEmail = await _usuarioRepository.GetByEmailAsync(createDto.Email);
            if (existeEmail != null)
            {
                return (false, "Ya existe un usuario con el mismo correo electronico", null);
            }
        }

        // Generar clave aleatoria si no se proporciona
        var generatedPassword = string.IsNullOrEmpty(createDto.Password)
            ? GenerarClaveAleatoria()
            : createDto.Password;

        var usuario = new Usuario
        {
            TipoUsuario = "I", // Siempre interno
            Login = createDto.Login,
            Password = generatedPassword, // TODO: BCrypt.Net.BCrypt.HashPassword(generatedPassword)
            Email = createDto.Email,
            NumeroDocumento = createDto.NumeroDocumento,
            Nombres = createDto.Nombres,
            ApellidoPaterno = createDto.ApellidoPaterno,
            ApellidoMaterno = createDto.ApellidoMaterno,
            Celular = createDto.Celular,
            Telefono = createDto.Telefono,
            Cargo = createDto.Cargo,
            IdEntidadMedica = null, // Usuario interno no tiene entidad medica
            IdRol = createDto.IdRol,
            IdCreador = idCreador,
            Activo = 1
        };

        var idUsuario = await _usuarioRepository.CreateAsync(usuario);

        // TODO: Si enviarCorreo es true, enviar email con credenciales

        return (true, null, generatedPassword);
    }

    /// <summary>
    /// Resetea la clave de un usuario generando una nueva clave aleatoria
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> ResetearClaveUsuarioAsync(int idUsuario, int idModificador, bool enviarCorreo)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado", null);
        }

        // Generar nueva clave
        var nuevaClave = GenerarClaveAleatoria();

        // Actualizar clave
        // TODO: BCrypt.Net.BCrypt.HashPassword(nuevaClave)
        var updated = await _usuarioRepository.UpdatePasswordAsync(idUsuario, nuevaClave);
        if (!updated)
        {
            return (false, "Error al actualizar la clave", null);
        }

        // TODO: Si enviarCorreo es true, enviar email con nueva clave

        return (true, null, nuevaClave);
    }

    private static string GenerarClaveAleatoria()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static UsuarioResponseDto MapToResponseDto(Usuario usuario)
    {
        return new UsuarioResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            TipoUsuario = usuario.TipoUsuario,
            Login = usuario.Login,
            Email = usuario.Email,
            NumeroDocumento = usuario.NumeroDocumento,
            Nombres = usuario.Nombres,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            Celular = usuario.Celular,
            Telefono = usuario.Telefono,
            Cargo = usuario.Cargo,
            IdEntidadMedica = usuario.IdEntidadMedica,
            IdRol = usuario.IdRol,
            GuidRegistro = usuario.GuidRegistro,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            FechaModificacion = usuario.FechaModificacion
        };
    }
}
