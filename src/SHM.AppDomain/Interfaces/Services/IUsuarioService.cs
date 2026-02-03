using SHM.AppDomain.DTOs.Opcion;
using SHM.AppDomain.DTOs.Usuario;

namespace SHM.AppDomain.Interfaces.Services;

/// <summary>
/// Interface del servicio para la gestion de usuarios en la capa de aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtiene todos los usuarios registrados en el sistema.
    /// </summary>
    Task<IEnumerable<UsuarioResponseDto>> GetAllUsuariosAsync();

    /// <summary>
    /// Obtiene un usuario por su identificador unico.
    /// </summary>
    Task<UsuarioResponseDto?> GetUsuarioByIdAsync(int id);

    /// <summary>
    /// Obtiene un usuario por su nombre de login.
    /// </summary>
    Task<UsuarioResponseDto?> GetUsuarioByLoginAsync(string login);

    /// <summary>
    /// Valida las credenciales de un usuario para autenticacion.
    /// </summary>
    Task<UsuarioResponseDto?> ValidarCredencialesAsync(string login, string password);

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    Task<UsuarioResponseDto> CreateUsuarioAsync(CreateUsuarioDto createDto, int idCreador);

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    Task<bool> UpdateUsuarioAsync(int id, UpdateUsuarioDto updateDto, int idModificador);

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    Task<bool> DeleteUsuarioAsync(int id);

    /// <summary>
    /// Genera un token para recuperacion de contrasena.
    /// </summary>
    Task<(bool Success, string? Token, string? Email, string? Nombres)> GenerarTokenRecuperacionAsync(string emailOrLogin);

    /// <summary>
    /// Valida si un token de recuperacion es valido y no ha expirado.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> ValidarTokenRecuperacionAsync(string token);

    /// <summary>
    /// Restablece la contrasena de un usuario usando el token de recuperacion.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> RestablecerClaveAsync(string token, string nuevaPassword);

    /// <summary>
    /// Lista las opciones de menu disponibles para un usuario segun su login.
    /// </summary>
    Task<IEnumerable<OpcionMenuDTO>> ListarMenuPorLoginAsync(string login);

    /// <summary>
    /// Actualiza los datos personales del usuario autenticado.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> ActualizarMisDatosAsync(int idUsuario, string? nombres, string? apellidoPaterno, string? apellidoMaterno, string? email, string? numeroDocumento, string? celular);

    /// <summary>
    /// Cambia la contrasena del usuario autenticado.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> CambiarPasswordAsync(int idUsuario, string passwordActual, string passwordNueva);

    /// <summary>
    /// Obtiene un usuario por su GUID de registro.
    /// </summary>
    Task<UsuarioResponseDto?> GetUsuarioByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene usuarios externos de forma paginada con opcion de busqueda.
    /// </summary>
    Task<(IEnumerable<UsuarioResponseDto> Items, int TotalCount)> GetPaginatedExternosAsync(string? searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene usuarios internos de forma paginada con opcion de busqueda.
    /// </summary>
    Task<(IEnumerable<UsuarioResponseDto> Items, int TotalCount)> GetPaginatedInternosAsync(string? searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// Elimina un usuario registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteUsuarioAsync(int id, int idModificador);

    /// <summary>
    /// Crea un nuevo usuario externo con opcion de envio de correo de bienvenida.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> CreateUsuarioExternoAsync(CreateUsuarioDto createDto, int idCreador, bool enviarCorreo);

    /// <summary>
    /// Crea un nuevo usuario interno con opcion de envio de correo de bienvenida.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> CreateUsuarioInternoAsync(CreateUsuarioDto createDto, int idCreador, bool enviarCorreo);

    /// <summary>
    /// Resetea la clave de un usuario y opcionalmente envia notificacion por correo.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> ResetearClaveUsuarioAsync(int idUsuario, int idModificador, bool enviarCorreo);

    /// <summary>
    /// Obtiene los IDs de sedes asignadas a un usuario interno.
    /// </summary>
    Task<IEnumerable<int>> GetSedesUsuarioInternoAsync(int idUsuario);

    /// <summary>
    /// Actualiza las sedes de un usuario interno.
    /// </summary>
    Task<bool> UpdateSedesUsuarioInternoAsync(int idUsuario, List<int>? idsSedesSeleccionadas, int idModificador);
}
