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

            _logger.LogInformation("- URL: {Url}", _settings.BaseUrl);
            _logger.LogInformation("- Timeout (s): {Timeout}", _settings.TimeoutSeconds);
            _logger.LogInformation("- Usuario: {Usuario}", _settings.Usuario);
            _logger.LogInformation("- Password: {Password}",  _settings.Password.Length);

            var loginRequest = new SanPabloLoginRequestDto
            {
                Usuario = _settings.Usuario,
                Password = _settings.Password
            };

            _logger.LogInformation("- paso 1");

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_settings.EndpointLogin, content);

            _logger.LogInformation("- paso 2");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener token de San Pablo. StatusCode: {StatusCode}", response.StatusCode);
                return null;
            }

            _logger.LogInformation("- paso 3");

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de login: {Response}", responseContent);
            _logger.LogInformation("Respuesta de login: {Response}", responseContent);

            _logger.LogInformation("- paso 4");

            var loginResponse = JsonSerializer.Deserialize<SanPabloLoginResponseDto>(responseContent, _jsonOptions);

            if (!string.IsNullOrEmpty(loginResponse?.AccessToken))
            {
                _cachedToken = loginResponse.AccessToken;
                // Token valido por 55 minutos (asumiendo expiracion de 1 hora)
                _tokenExpiration = DateTime.Now.AddMinutes(55);
                _logger.LogInformation("Token de San Pablo obtenido exitosamente. Usuario: {Usuario}",
                    loginResponse.UserInfo?.Usuario ?? "N/A");
                return _cachedToken;
            }

            _logger.LogWarning("Login a San Pablo fallido: No se recibio access_token en la respuesta");
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
    /// Verifica la conectividad con el API de San Pablo intentando obtener un token de acceso.
    /// </summary>
    public async Task<(bool Ok, string Mensaje)> CheckConnectionAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
                return (true, "Conexion exitosa");

            return (false, "No se pudo obtener token de acceso. Verifique credenciales.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar conexion con API San Pablo");
            return (false, $"Error de conexion: {ex.Message}");
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
            var url = $"{_settings.EndpointObtenerEntidad}?Codigo={Uri.EscapeDataString(codigoSede)}&flgCIAMedica={Uri.EscapeDataString(tipoEntidad)}&codigoEntidad={Uri.EscapeDataString(codigoEntidad)}";

            _logger.LogDebug("GetEntidadMedicaAsync url: {url}", url);


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

            if (entidadResponse?.IsSuccess == true && entidadResponse.Data != null && entidadResponse.Data.Count > 0)
            {
                // Buscar la entidad por codigo en el array de resultados
                var entidad = entidadResponse.Data.FirstOrDefault(e =>
                    e.CODIGO?.Equals(codigoEntidad, StringComparison.OrdinalIgnoreCase) == true);

                if (entidad != null)
                {
                    _logger.LogInformation(
                        "Entidad medica encontrada en San Pablo. Codigo: {Codigo}, Nombre: {Nombre}",
                        entidad.CODIGO, entidad.NOMBRE);
                    return entidad;
                }

                // Si no se encuentra por codigo exacto, tomar el primer resultado
                entidad = entidadResponse.Data.First();
                _logger.LogInformation(
                    "Entidad medica encontrada en San Pablo (primer resultado). Codigo: {Codigo}, Nombre: {Nombre}",
                    entidad.CODIGO, entidad.NOMBRE);
                return entidad;
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

    /// <summary>
    /// Obtiene todas las sedes desde el API de San Pablo usando Codigo=X.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-24</created>
    /// </summary>
    public async Task<List<SanPabloSedeDto>> GetAllSedesAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para consultar todas las sedes");
                return new List<SanPabloSedeDto>();
            }

            _logger.LogInformation("Consultando todas las sedes en San Pablo (Codigo=X)");

            var url = $"{_settings.EndpointObtenerSede}?Codigo=X";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar todas las sedes en San Pablo. StatusCode: {StatusCode}", response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                return new List<SanPabloSedeDto>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de todas las sedes: {Response}", responseContent);

            var sedeResponse = JsonSerializer.Deserialize<SanPabloSedeResponseDto>(responseContent, _jsonOptions);

            if (sedeResponse?.IsSuccess == true && sedeResponse.Data != null)
            {
                _logger.LogInformation("Se obtuvieron {Count} sedes desde API San Pablo", sedeResponse.Data.Count);
                return sedeResponse.Data;
            }

            _logger.LogWarning("No se obtuvieron sedes desde API San Pablo. Message: {Message}", sedeResponse?.Message);
            return new List<SanPabloSedeDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al consultar todas las sedes en San Pablo");
            return new List<SanPabloSedeDto>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar todas las sedes en San Pablo");
            return new List<SanPabloSedeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar todas las sedes en San Pablo");
            return new List<SanPabloSedeDto>();
        }
    }

    /// <summary>
    /// Obtiene los datos de una sede desde el API de San Pablo.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-24</created>
    /// </summary>
    public async Task<SanPabloSedeDto?> GetSedeAsync(string codigo)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para consultar sede");
                return null;
            }

            _logger.LogInformation("Consultando sede en San Pablo. Codigo: {Codigo}", codigo);

            var url = $"{_settings.EndpointObtenerSede}?Codigo={Uri.EscapeDataString(codigo)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar sede en San Pablo. StatusCode: {StatusCode}", response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de sede: {Response}", responseContent);

            var sedeResponse = JsonSerializer.Deserialize<SanPabloSedeResponseDto>(responseContent, _jsonOptions);

            if (sedeResponse?.IsSuccess == true && sedeResponse.Data != null && sedeResponse.Data.Count > 0)
            {
                var sede = sedeResponse.Data.FirstOrDefault(s =>
                    s.CODIGO?.Equals(codigo, StringComparison.OrdinalIgnoreCase) == true);

                if (sede != null)
                {
                    _logger.LogInformation("Sede encontrada en San Pablo. Codigo: {Codigo}, Descripcion: {Descripcion}",
                        sede.CODIGO, sede.DESCRIPCION);
                    return sede;
                }

                sede = sedeResponse.Data.First();
                _logger.LogInformation("Sede encontrada en San Pablo (primer resultado). Codigo: {Codigo}, Descripcion: {Descripcion}",
                    sede.CODIGO, sede.DESCRIPCION);
                return sede;
            }

            _logger.LogWarning("Sede no encontrada en San Pablo. Codigo: {Codigo}, Message: {Message}",
                codigo, sedeResponse?.Message);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al consultar sede en San Pablo");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar sede en San Pablo");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar sede en San Pablo");
            return null;
        }
    }

    /// <summary>
    /// Registra un comprobante en el API de San Pablo.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-25</created>
    /// </summary>
    public async Task<SanPabloComprobanteResponseDto> RegistrarComprobanteAsync(SanPabloComprobanteRequestDto request)
    {
        var errorResponse = new SanPabloComprobanteResponseDto { IsSuccess = false };

        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para registrar comprobante");
                errorResponse.Message = "No se pudo obtener token de autenticacion";
                return errorResponse;
            }

            _logger.LogInformation(
                "Registrando comprobante en San Pablo. Sede: {Sede}, Entidad: {Entidad}, Produccion: {Produccion}, Serie: {Serie}, Numero: {Numero}",
                request.COD_SEDE, request.COD_ENTIDAD, request.COD_PROD, request.CPM_SERIE, request.CPM_NUMERO);


            string json = JsonSerializer.Serialize(request, _jsonOptions);
            
            _logger.LogInformation(json);


           var jsonContent = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointRegistrarComprobante);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = jsonContent;

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al registrar comprobante en San Pablo. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                errorResponse.Message = $"Error HTTP {(int)response.StatusCode}: {errorContent}";
                return errorResponse;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de registrar comprobante: {Response}", responseContent);

            var comprobanteResponse = JsonSerializer.Deserialize<SanPabloComprobanteResponseDto>(responseContent, _jsonOptions);

            if (comprobanteResponse != null)
            {
                _logger.LogInformation(
                    "Respuesta de registrar comprobante. IsSuccess: {IsSuccess}, Title: {Title}, Message: {Message}",
                    comprobanteResponse.IsSuccess, comprobanteResponse.Title, comprobanteResponse.Message);
                return comprobanteResponse;
            }

            errorResponse.Message = "No se pudo deserializar la respuesta del API";
            return errorResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al registrar comprobante en San Pablo");
            errorResponse.Message = $"Error de conexion: {ex.Message}";
            return errorResponse;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al registrar comprobante en San Pablo");
            errorResponse.Message = "Timeout al conectar con API San Pablo";
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar comprobante en San Pablo");
            errorResponse.Message = $"Error inesperado: {ex.Message}";
            return errorResponse;
        }
    }
}
