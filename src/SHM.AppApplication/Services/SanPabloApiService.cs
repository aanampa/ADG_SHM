using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SHM.AppDomain.DTOs.SanPabloApi;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para comunicarse con el API externo de San Pablo.
/// Maneja la autenticacion JWT y las consultas de entidades medicas.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-02</created>
/// </summary>
public class SanPabloApiService : ISanPabloApiService
{
    private readonly HttpClient _httpClient;
    private readonly SanPabloApiSettings _settings;
    private readonly ILogger<SanPabloApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private string? _cachedToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public SanPabloApiService(
        HttpClient httpClient,
        IOptions<SanPabloApiSettings> settings,
        ILogger<SanPabloApiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    /// <summary>
    /// Obtiene un token de autenticacion del API de San Pablo.
    /// Implementa cache del token para evitar llamadas innecesarias.
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        // Si tenemos un token en cache y no ha expirado, lo usamos
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.Now < _tokenExpiration)
        {
            _logger.LogDebug("Usando token en cache para API San Pablo");
            return _cachedToken;
        }

        try
        {
            _logger.LogInformation("Obteniendo nuevo token de API San Pablo");

            var loginRequest = new SanPabloLoginRequestDto
            {
                Usuario = _settings.Usuario,
                Password = _settings.Password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/Usuario/Login", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener token de San Pablo. StatusCode: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de login: {Response}", responseContent);

            var loginResponse = JsonSerializer.Deserialize<SanPabloLoginResponseDto>(responseContent, _jsonOptions);

            if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
            {
                _cachedToken = loginResponse.Token;
                // Token valido por 55 minutos (asumiendo expiracion de 1 hora)
                _tokenExpiration = DateTime.Now.AddMinutes(55);
                _logger.LogInformation("Token de San Pablo obtenido exitosamente");
                return _cachedToken;
            }

            _logger.LogWarning("Login a San Pablo fallido: {Message}", loginResponse?.Message);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al obtener token de San Pablo");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al obtener token de San Pablo");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener token de San Pablo");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los datos de una entidad medica desde el API de San Pablo.
    /// </summary>
    public async Task<SanPabloEntidadMedicaDto?> GetEntidadMedicaAsync(string codigoSede, string tipoEntidad, string codigoEntidad)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para consultar entidad medica");
                return null;
            }

            _logger.LogInformation(
                "Consultando entidad medica en San Pablo. CodigoSede: {CodigoSede}, TipoEntidad: {TipoEntidad}, CodigoEntidad: {CodigoEntidad}",
                codigoSede, tipoEntidad, codigoEntidad);

            // Construir URL con parametros
            var url = $"/api/HHMM/v1/ListarObtenerEntidad?Codigo={Uri.EscapeDataString(codigoSede)}&flgCIAMedica={Uri.EscapeDataString(tipoEntidad)}&codigoEntidad={Uri.EscapeDataString(codigoEntidad)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Error al consultar entidad medica en San Pablo. StatusCode: {StatusCode}",
                    response.StatusCode);

                // Si es 401, invalidamos el token cache
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de entidad medica: {Response}", responseContent);

            var entidadResponse = JsonSerializer.Deserialize<SanPabloEntidadMedicaResponseDto>(responseContent, _jsonOptions);

            if (entidadResponse?.Success == true && entidadResponse.Data != null)
            {
                _logger.LogInformation(
                    "Entidad medica encontrada en San Pablo. Codigo: {Codigo}, Nombre: {Nombre}",
                    entidadResponse.Data.Codigo, entidadResponse.Data.Nombre);
                return entidadResponse.Data;
            }

            _logger.LogWarning(
                "Entidad medica no encontrada en San Pablo. CodigoEntidad: {CodigoEntidad}, Message: {Message}",
                codigoEntidad, entidadResponse?.Message);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al consultar entidad medica en San Pablo");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar entidad medica en San Pablo");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar entidad medica en San Pablo");
            return null;
        }
    }
}
