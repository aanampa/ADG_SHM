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

    /// <summary>
    /// Obtiene el estado de pago de un comprobante desde SAP (DatosFacturaSet).
    /// Transforma los parametros del sistema interno al formato esperado por SAP:
    /// - TipoComprobante: "1"→"H1", "22"→"H2"
    /// - Serie: prefijo "0" a la izquierda
    /// - NumeroComprobante: completado con ceros a la izquierda (7 caracteres)
    /// - Anio: obtenido de fechaEmision
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-04-15</created>
    /// </summary>
    public async Task<SapDatosFacturaDto?> GetDatosFacturaAsync(
        string codigoAcreedor,
        string tipoComprobante,
        string serie,
        string numeroComprobante,
        DateTime fechaEmision)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se pudo obtener token para consultar datos de factura en SAP");
                return null;
            }

            // Transformaciones a formato SAP
            var tipoSap = tipoComprobante switch
            {
                "1"  => "H1",
                "22" => "H2",
                _    => tipoComprobante
            };
            var serieSap  = "0" + serie;
            var numeroSap = numeroComprobante.TrimStart('0').PadLeft(7, '0');
            var anioSap   = fechaEmision.Year.ToString();

            var url = $"{_settings.EndpointDatosFactura}(CodigoAcreedor='{codigoAcreedor}',TipoComprobante='{tipoSap}',Serie='{serieSap}',NumeroComprobante='{numeroSap}',Anio='{anioSap}')";

            _logger.LogInformation(
                "Consultando datos de factura en SAP. Acreedor: {Acreedor}, Tipo: {Tipo}, Serie: {Serie}, Numero: {Numero}, Anio: {Anio}",
                codigoAcreedor, tipoSap, serieSap, numeroSap, anioSap);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Error al consultar datos de factura en SAP. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de datos de factura SAP: {Response}", responseContent);

            var odataResponse = JsonSerializer.Deserialize<SapODataSingleResponseDto<SapDatosFacturaDto>>(responseContent, _jsonOptions);

            if (odataResponse?.D != null)
            {
                _logger.LogInformation(
                    "Datos de factura obtenidos desde SAP. Acreedor: {Acreedor}, EstadoPago: {EstadoPago}",
                    codigoAcreedor, odataResponse.D.EstadoPago);
                return odataResponse.D;
            }

            _logger.LogWarning("No se obtuvieron datos de factura desde SAP. Respuesta sin datos");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al consultar datos de factura en SAP. Acreedor: {Acreedor}", codigoAcreedor);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar datos de factura en SAP. Acreedor: {Acreedor}", codigoAcreedor);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar datos de factura en SAP. Acreedor: {Acreedor}", codigoAcreedor);
            return null;
        }
    }

    /// <summary>
    /// Libera pagos en SAP (LIBERA_PAGO_CABSet).
    /// Pasos: token OAuth2, token CSRF (GET $metadata con x-csrf-token: fetch) y POST de la orden.
    /// El token CSRF esta ligado a la cookie de sesion del mismo GET; el HttpClientHandler
    /// conserva las cookies, por lo que el POST las reenvia. Si SAP responde 403 se obtiene
    /// otro token y se reintenta una vez.
    /// El exito se determina por HTTP 2xx y d.Status = "OK" (el 201 solo indica recepcion).
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-11-11</created>
    /// </summary>
    public async Task<SapLiberaPagoResultadoDto> LiberarPagoAsync(SapLiberaPagoRequestDto request)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new SapLiberaPagoResultadoDto { Mensaje = "No se pudo obtener el token de acceso de SAP" };

            var json = JsonSerializer.Serialize(request);
            _logger.LogInformation("Liberando pago en SAP. IdOrden: {IdOrden}, Documentos: {Cantidad}",
                request.IdOrden, request.Documentos.Count);
            _logger.LogDebug("Request de liberacion SAP: {Json}", json);

            for (var intento = 1; intento <= 2; intento++)
            {
                var csrf = await ObtenerTokenCsrfAsync(token);
                if (string.IsNullOrEmpty(csrf))
                    return new SapLiberaPagoResultadoDto { Mensaje = "No se pudo obtener el token CSRF de SAP" };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointLiberaPago);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequest.Headers.Add("x-csrf-token", csrf);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta de liberacion SAP. StatusCode: {StatusCode}, Body: {Body}",
                    response.StatusCode, body);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && intento == 1)
                {
                    _logger.LogWarning("SAP rechazo el token CSRF (403). Se obtiene otro y se reintenta");
                    continue;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null;
                    _tokenExpiration = DateTime.MinValue;
                }

                var resultado = InterpretarRespuestaLiberaPago(response.StatusCode, body);
                if (resultado.Exito)
                    _logger.LogInformation(
                        "Liberacion SAP exitosa. IdOrden: {IdOrden}, HTTP: {Http}, Status: {Status}, Mensaje: {Mensaje}",
                        request.IdOrden, resultado.HttpStatus, resultado.Status, resultado.Mensaje);
                else
                    _logger.LogWarning(
                        "Liberacion SAP no confirmada. IdOrden: {IdOrden}, HTTP: {Http}, Status: {Status}, Mensaje: {Mensaje}, Respuesta: {Respuesta}",
                        request.IdOrden, resultado.HttpStatus, resultado.Status, resultado.Mensaje,
                        body.Length > 2000 ? body[..2000] : body);
                return resultado;
            }

            return new SapLiberaPagoResultadoDto { Mensaje = "SAP rechazo el token CSRF" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexion al liberar pago en SAP. IdOrden: {IdOrden}", request.IdOrden);
            return new SapLiberaPagoResultadoDto { Mensaje = "Error de conexion con SAP: " + ex.Message };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al liberar pago en SAP. IdOrden: {IdOrden}", request.IdOrden);
            return new SapLiberaPagoResultadoDto { Mensaje = "Tiempo de espera agotado al llamar a SAP" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al liberar pago en SAP. IdOrden: {IdOrden}", request.IdOrden);
            return new SapLiberaPagoResultadoDto { Mensaje = "Error inesperado al llamar a SAP: " + ex.Message };
        }
    }

    /// <summary>
    /// Obtiene el token CSRF de SAP consultando $metadata con el header x-csrf-token: fetch.
    /// </summary>
    private async Task<string?> ObtenerTokenCsrfAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _settings.EndpointMetadata);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("x-csrf-token", "fetch");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Error al obtener el token CSRF de SAP. StatusCode: {StatusCode}", response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _cachedToken = null;
                _tokenExpiration = DateTime.MinValue;
            }
            return null;
        }

        return response.Headers.TryGetValues("x-csrf-token", out var values) ? values.FirstOrDefault() : null;
    }

    /// <summary>
    /// Interpreta la respuesta de la liberacion. Exito solo si HTTP 2xx y d.Status = "OK".
    /// </summary>
    private static SapLiberaPagoResultadoDto InterpretarRespuestaLiberaPago(System.Net.HttpStatusCode statusCode, string body)
    {
        var codigo = (int)statusCode;
        var resultado = new SapLiberaPagoResultadoDto { HttpStatus = codigo, RespuestaRaw = body };
        var esHttpOk = codigo >= 200 && codigo <= 299;

        static string? Texto(JsonElement padre, string propiedad) =>
            padre.ValueKind == JsonValueKind.Object &&
            padre.TryGetProperty(propiedad, out var v) &&
            v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var raiz = doc.RootElement;

            if (!esHttpOk)
            {
                // Error OData: { "error": { "message": { "value": "..." } } }
                string? detalle = null;
                if (raiz.ValueKind == JsonValueKind.Object && raiz.TryGetProperty("error", out var error) &&
                    error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var message))
                {
                    detalle = message.ValueKind == JsonValueKind.Object ? Texto(message, "value") : Texto(error, "message");
                }
                resultado.Mensaje = $"SAP respondio HTTP {codigo}" + (string.IsNullOrEmpty(detalle) ? "" : ": " + detalle);
                return resultado;
            }

            if (raiz.ValueKind == JsonValueKind.Object && raiz.TryGetProperty("d", out var d))
            {
                resultado.Status = Texto(d, "Status");
                resultado.Mensaje = Texto(d, "Mensaje");
            }

            resultado.Exito = string.Equals(resultado.Status, "OK", StringComparison.OrdinalIgnoreCase);
            if (!resultado.Exito && string.IsNullOrEmpty(resultado.Mensaje))
            {
                resultado.Mensaje = string.IsNullOrEmpty(resultado.Status)
                    ? "La respuesta de SAP no incluye Status"
                    : $"SAP respondio con Status '{resultado.Status}'";
            }
        }
        catch (JsonException)
        {
            resultado.Mensaje = esHttpOk
                ? "La respuesta de SAP no tiene el formato esperado"
                : $"SAP respondio HTTP {codigo}";
        }

        return resultado;
    }
}
