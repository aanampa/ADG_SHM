using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SHM.AppDomain.DTOs.SapApi;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para comunicarse con el API OData de SAP.
/// Maneja la autenticacion OAuth2 (client_credentials) y las consultas OData.
///
/// <author>ADG Antonio</author>
/// <created>2026-03-04</created>
/// </summary>
public class SapApiService : ISapApiService
{
    private readonly HttpClient _httpClient;
    private readonly SapApiSettings _settings;
    private readonly ILogger<SapApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private string? _cachedToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public SapApiService(
        HttpClient httpClient,
        IOptions<SapApiSettings> settings,
        ILogger<SapApiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    /// <summary>
    /// Obtiene un token de acceso OAuth2 de SAP usando client_credentials.
    /// Implementa cache del token para evitar llamadas innecesarias.
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        // Si tenemos un token en cache y no ha expirado, lo usamos
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.Now < _tokenExpiration)
        {
            _logger.LogDebug("Usando token en cache para API SAP");
            return _cachedToken;
        }

        try
        {
            _logger.LogInformation("Obteniendo nuevo token de API SAP");

            // Basic Auth: Username:Password en Base64
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Body: application/x-www-form-urlencoded
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", _settings.Scope)
            });
            request.Content = formContent;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al obtener token de SAP. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de token SAP: {Response}", responseContent);

            var tokenResponse = JsonSerializer.Deserialize<SapTokenResponseDto>(responseContent, _jsonOptions);

            if (!string.IsNullOrEmpty(tokenResponse?.AccessToken))
            {
                _cachedToken = tokenResponse.AccessToken;
                // Cache por 55 minutos (token expira en 3600 seg = 1 hora)
                _tokenExpiration = DateTime.Now.AddMinutes(55);
                _logger.LogInformation("Token de SAP obtenido exitosamente. Expira en: {ExpiresIn} segundos",
                    tokenResponse.ExpiresIn);
                return _cachedToken;
            }

            _logger.LogWarning("Login a SAP fallido: No se recibio access_token en la respuesta");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al obtener token de SAP");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al obtener token de SAP");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener token de SAP");
            return null;
        }
    }

    /// <summary>
    /// Verifica la conectividad con el API de SAP intentando obtener un token de acceso.
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
            _logger.LogError(ex, "Error al verificar conexion con API SAP");
            return (false, $"Error de conexion: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la lista de bancos desde SAP (COD_BANCOSet).
    /// </summary>
    public async Task<List<SapBancoDto>> GetBancosAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para consultar bancos en SAP");
                return new List<SapBancoDto>();
            }

            _logger.LogInformation("Consultando bancos en SAP (COD_BANCOSet)");

            using var request = new HttpRequestMessage(HttpMethod.Get, _settings.EndpointBancos);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al consultar bancos en SAP. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                // Si es 401, invalidamos el token cache
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                return new List<SapBancoDto>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de bancos SAP: {Response}", responseContent);

            var odataResponse = JsonSerializer.Deserialize<SapODataResponseDto<SapBancoDto>>(responseContent, _jsonOptions);

            if (odataResponse?.D?.Results != null)
            {
                _logger.LogInformation("Se obtuvieron {Count} bancos desde SAP", odataResponse.D.Results.Count);
                return odataResponse.D.Results;
            }

            _logger.LogWarning("No se obtuvieron bancos desde SAP. Respuesta sin resultados");
            return new List<SapBancoDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al consultar bancos en SAP");
            return new List<SapBancoDto>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar bancos en SAP");
            return new List<SapBancoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar bancos en SAP");
            return new List<SapBancoDto>();
        }
    }

    /// <summary>
    /// Obtiene las cuentas bancarias de un acreedor desde SAP (CTA_ACREEDORSet).
    /// </summary>
    /// <param name="codAcreedor">Codigo del acreedor en SAP.</param>
    public async Task<List<SapAcreedorCuentaBancariaDto>> GetCuentasBancariasByAcreedorAsync(string codAcreedor)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para consultar cuentas bancarias de acreedor en SAP");
                return new List<SapAcreedorCuentaBancariaDto>();
            }

            _logger.LogInformation("Consultando cuentas bancarias del acreedor {CodAcreedor} en SAP", codAcreedor);

            var url = $"{_settings.EndpointCuentasAcreedor}?$filter=CodAcreedor eq '{codAcreedor}'";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al consultar cuentas bancarias en SAP. CodAcreedor: {CodAcreedor}, StatusCode: {StatusCode}, Response: {Response}",
                    codAcreedor, response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                return new List<SapAcreedorCuentaBancariaDto>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de cuentas bancarias SAP: {Response}", responseContent);

            var odataResponse = JsonSerializer.Deserialize<SapODataResponseDto<SapAcreedorCuentaBancariaDto>>(responseContent, _jsonOptions);

            if (odataResponse?.D?.Results != null)
            {
                _logger.LogInformation("Se obtuvieron {Count} cuentas bancarias del acreedor {CodAcreedor} desde SAP",
                    odataResponse.D.Results.Count, codAcreedor);
                return odataResponse.D.Results;
            }

            _logger.LogWarning("No se obtuvieron cuentas bancarias del acreedor {CodAcreedor} desde SAP", codAcreedor);
            return new List<SapAcreedorCuentaBancariaDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al consultar cuentas bancarias del acreedor {CodAcreedor} en SAP", codAcreedor);
            return new List<SapAcreedorCuentaBancariaDto>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar cuentas bancarias del acreedor {CodAcreedor} en SAP", codAcreedor);
            return new List<SapAcreedorCuentaBancariaDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar cuentas bancarias del acreedor {CodAcreedor} en SAP", codAcreedor);
            return new List<SapAcreedorCuentaBancariaDto>();
        }
    }
}
