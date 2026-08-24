using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Backend.Repositories;

namespace Backend.Services
{
    public class VisaEnLinkOptions
    {
        public string UrlVisa { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string UsuVisa { get; set; } = string.Empty;
        public string ClaveVisa { get; set; } = string.Empty;
        public bool UseProxy { get; set; }
        public string ProxyUrl { get; set; } = string.Empty;
        public string ProxyUser { get; set; } = string.Empty;
        public string ProxyPassword { get; set; } = string.Empty;
    }

    public class VisaEnLinkIntegrationService : IVisaEnLinkIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ISiteRepository _siteRepository;
        private readonly VisaEnLinkOptions _options;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<VisaEnLinkIntegrationService> _logger;

        public VisaEnLinkIntegrationService(
            HttpClient httpClient,
            ISiteRepository siteRepository, 
            IOptions<VisaEnLinkOptions> options,
            IWebHostEnvironment env,
            ILogger<VisaEnLinkIntegrationService> logger)
        {
            _httpClient = httpClient;
            _siteRepository = siteRepository;
            _options = options.Value;
            _env = env;
            _logger = logger;

            if (!string.IsNullOrEmpty(_options.UrlVisa))
            {
                _httpClient.BaseAddress = new Uri(_options.UrlVisa);
            }
        }

        // This method is now responsible for creating a client with the correct proxy settings
        // on a per-call basis if needed, or you can configure the injected _httpClient.
        // For simplicity and to match the request, we will create a new client when proxy is needed.
        public async Task<string> GetOrGenerateTokenAsync()
        {
            // If in Development environment, return a dummy token immediately
            if (_env.IsDevelopment())
            {
                return "dummy_development_token";
            }

            // 1. Check database for a valid token created today
            string? token = await _siteRepository.GetTokenInternoAsync();
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            // 2. Generate a new token if not found
            token = await GenerateNewTokenAsync();

            // 3. Cache the token in the database
            await _siteRepository.InsertTokenAsync(token);

            return token;
        }

        private async Task<string> GenerateNewTokenAsync()
        {
            _logger.LogInformation("Iniciando GenerateNewTokenAsync");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("usuario", _options.UsuVisa),
                new KeyValuePair<string, string>("clave", _options.ClaveVisa)
            });
            try
            {
                _logger.LogInformation("Enviando POST a /api/login para obtener token...");
                var response = await _httpClient.PostAsync("/api/login", content);
                
                // --- INICIO DE CAMBIOS PARA DEPURACIÓN ---
                // 1. Leer la respuesta como texto crudo para poder registrarla.
                var rawContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de Neo API /api/login - StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, rawContent);
                // --- FIN DE CAMBIOS PARA DEPURACIÓN ---

                VisaLoginResponse? result;
                try
                {
                    // 2. Intentar deserializar la respuesta.
                    result = JsonSerializer.Deserialize<VisaLoginResponse>(rawContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException jsonEx)
                {
                    // Si la deserialización falla, la respuesta no es un JSON válido.
                    _logger.LogError(jsonEx, "Fallo al deserializar la respuesta de Neo API /api/login. El contenido no es un JSON válido. Contenido Crudo: {RawContent}", rawContent);
                    throw new Exception($"La respuesta del servicio de autenticación no tiene un formato válido.");
                }

                // 3. Validar el estado HTTP y el resultado de la API.
                // Si el estado HTTP no es de éxito, o si el JSON indica un error, lanzamos una excepción con el mensaje de la API.
                if (!response.IsSuccessStatusCode || result == null || result.Result != "success" || result.Data == null || string.IsNullOrEmpty(result.Data.Token))
                {
                    string errorMessage;
                    if (result != null && !string.IsNullOrEmpty(result.Message))
                    {
                        // La API devolvió un JSON válido con un mensaje de error específico.
                        errorMessage = result.Message;
                        _logger.LogWarning("La API de Neo devolvió un error. Mensaje: {Message}. StatusCode: {StatusCode}", errorMessage, response.StatusCode);
                    }
                    else
                    {
                        // La API devolvió un error HTTP, pero el JSON no contenía un mensaje específico o era nulo.
                        errorMessage = $"Error HTTP {response.StatusCode} al contactar la API de Neo.";
                        _logger.LogError("La API de Neo devolvió un error HTTP sin mensaje específico. StatusCode: {StatusCode}. Contenido: {RawContent}", response.StatusCode, rawContent);
                    }
                    throw new Exception(errorMessage);
                }

                _logger.LogInformation("Token generado exitosamente.");
                return result.Data.Token;
            }
            catch (Exception ex) when (ex is not JsonException)
            {
                _logger.LogError(ex, "Excepción no controlada ocurrida en GenerateNewTokenAsync al contactar la API de Neo.");
                throw;
            }
        }

        private async Task<string> GetFirstSocialNetworkAsync(string token)
        {
            if (_env.IsDevelopment())
            {
                return "DUMMY_NETWORK";
            }

            _logger.LogInformation("Iniciando GetFirstSocialNetworkAsync");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("token", token)
            });
            try
            {
                _logger.LogInformation("Enviando POST a /api/network/all para consultar redes...");
                var response = await _httpClient.PostAsync("/api/network/all", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error HTTP {StatusCode} en /api/network/all. Contenido: {Content}", response.StatusCode, errorContent);
                }
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<VisaNetworkResponse>();
                if (result == null || result.Result != "success" || result.Data == null || result.Data.Count == 0)
                {
                    _logger.LogWarning("Respuesta de redes no fue exitosa o no trajo datos. Message: {Message}", result?.Message);
                    throw new Exception($"Error al consultar redes de Neo: {result?.Message ?? "Sin redes disponibles"}");
                }

                _logger.LogInformation("Redes consultadas exitosamente, retornando primera red: {Red}", result.Data[0].Codigo);
                return result.Data[0].Codigo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción ocurrida en GetFirstSocialNetworkAsync.");
                throw;
            }
        }

        public async Task<(string SKU, string LinkUrl)> CrearLinkAsync(
            string producto, 
            string monto, 
            string imgPublicitaria, 
            string codigoInterno)
        {
            if (_env.IsDevelopment())
            {
                return (codigoInterno, $"https://dummy.link/{codigoInterno}");
            }

            _logger.LogInformation("Iniciando CrearLinkAsync para el código {CodigoInterno}", codigoInterno);
            string token = await GetOrGenerateTokenAsync();
            string networks = await GetFirstSocialNetworkAsync(token);

            // 1. Create the link
            var linkContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("codigo_interno", codigoInterno),
                new KeyValuePair<string, string>("titulo", producto),
                new KeyValuePair<string, string>("cuotas", "VC00"),
                new KeyValuePair<string, string>("nombre_interno", producto),
                new KeyValuePair<string, string>("descripcion", producto),
                new KeyValuePair<string, string>("monto", monto),
                new KeyValuePair<string, string>("estado", "1"),
                new KeyValuePair<string, string>("redes_sociales", networks)
            });

            try
            {
                _logger.LogInformation("Enviando POST a /api/link/maintenance para crear link...");
                var response = await _httpClient.PostAsync("/api/link/maintenance", linkContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error HTTP {StatusCode} en /api/link/maintenance. Contenido: {Content}", response.StatusCode, errorContent);
                }
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<VisaLinkResponse>();
                if (result == null || result.Result != "success" || result.Data == null || result.Data.Count == 0)
                {
                    _logger.LogWarning("Respuesta de creación de link no fue exitosa. Message: {Message}", result?.Message);
                    throw new Exception($"Error al registrar link en Neo: {result?.Message ?? "Respuesta vacía"}");
                }

                string sku = codigoInterno;
                string linkUrl = result.Data[0].Url;
                _logger.LogInformation("Link creado exitosamente. Url: {Url}", linkUrl);

                // 2. Upload advertising image if provided
                if (!string.IsNullOrEmpty(imgPublicitaria))
                {
                    _logger.LogInformation("Iniciando subida de imagen publicitaria para el código {CodigoInterno}...", sku);
                    var imgContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("llave", _options.Key),
                        new KeyValuePair<string, string>("token", token),
                        new KeyValuePair<string, string>("codigo", sku),
                        new KeyValuePair<string, string>("imagen", imgPublicitaria),
                        new KeyValuePair<string, string>("tipo", "jpg")
                    });
                    
                    var imgResponse = await _httpClient.PostAsync("/api/link/image", imgContent);
                    
                    if (!imgResponse.IsSuccessStatusCode)
                    {
                        string imgErrorContent = await imgResponse.Content.ReadAsStringAsync();
                        _logger.LogError("Error HTTP {StatusCode} en /api/link/image. Contenido: {Content}", imgResponse.StatusCode, imgErrorContent);
                    }
                    imgResponse.EnsureSuccessStatusCode();

                    var imgResult = await imgResponse.Content.ReadFromJsonAsync<VisaGenericResponse>();
                    if (imgResult == null || imgResult.Result != "success")
                    {
                        _logger.LogWarning("Respuesta de carga de imagen no fue exitosa. Message: {Message}", imgResult?.Message);
                        throw new Exception($"Error al asociar imagen publicitaria en Neo: {imgResult?.Message ?? "Falla en carga"}");
                    }
                    _logger.LogInformation("Imagen subida exitosamente para {CodigoInterno}.", sku);
                }

                return (sku, linkUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción ocurrida en CrearLinkAsync.");
                throw;
            }
        }

        public async Task<VisaLinkInfo?> ConsultaLinkAsync(string sku)
        {
            // Development bypass as requested
            // This bypass is now smarter:
            // - If SKU ends in an even number, it simulates a PAID link.
            // - If SKU ends in an odd number or is not a number, it simulates a PENDING link.
            /*if (_env.IsDevelopment())
            {
                _logger.LogInformation("[DEV MODE] Simulando ConsultaLinkAsync para SKU: {Sku}", sku);
                char lastChar = sku.LastOrDefault();
                bool isEven = char.IsDigit(lastChar) && (int.Parse(lastChar.ToString()) % 2 == 0);

                if (isEven)
                {
                    _logger.LogInformation("[DEV MODE] Simulando respuesta PAGADA.");
                    return new VisaLinkInfo
                    {
                        Sku = sku,
                        Estado = "PAID",
                        Monto = 125.50,
                        Moneda = "Q",
                        Ventas = new List<VentaData> { new VentaData { Autorizacion = "888888" } }
                    };
                }

                _logger.LogInformation("[DEV MODE] Simulando respuesta PENDIENTE.");
                return new VisaLinkInfo { Sku = sku, Estado = "PENDING", Monto = 75.00, Moneda = "Q", Ventas = new List<VentaData>() };
            }*/

            string token = await GetOrGenerateTokenAsync();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("codigo", sku)
            });
            _logger.LogInformation("Neo API ConsultaLinkAsync Request - URL: /api/link/single, Sku: {Sku}", sku);

            var response = await _httpClient.PostAsync("/api/link/single", content);
            
            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Neo API ConsultaLinkAsync Response - StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, rawContent);

            response.EnsureSuccessStatusCode();

            // First, deserialize only the generic part to check the 'result' field
            var genericResult = JsonSerializer.Deserialize<VisaGenericResponse>(rawContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (genericResult == null || genericResult.Result != "success")
            {
                _logger.LogWarning("Neo API ConsultaLinkAsync no fue exitosa. Result: {Result}, Message: {Message}", genericResult?.Result, genericResult?.Message);
                throw new Exception($"Error al consultar link en Neo: {genericResult?.Message ?? "No se encontraron datos"}");
            }

            // If successful, now deserialize the full response
            var result = JsonSerializer.Deserialize<VisaLinkInfoResponse>(rawContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (result == null || result.Result != "success" || result.Data == null)
            {
                _logger.LogWarning("Neo API ConsultaLinkAsync fallida o sin datos. Result: {Result}, Message: {Message}", result?.Result, result?.Message);
                throw new Exception($"Error al consultar link en Neo: {result?.Message ?? "No se encontraron datos"}");
            }

            var info = new VisaLinkInfo
            {
                Sku = result.Data.CodigoInterno ?? string.Empty,
                LinkUrl = result.Data.Token ?? string.Empty,
                Estado = result.Data.Estado ?? string.Empty,
                Nombre = result.Data.NombreInterno ?? string.Empty,
                Monto = result.Data.Precio,
                Moneda = result.Data.Moneda ?? "Q" // Asegurar que la moneda se mapee
            };

            if (result.Data.Ventas != null && result.Data.Ventas.Count > 0)
            {
                info.Ventas = result.Data.Ventas.Select(v => new VentaData { Autorizacion = v.Autorizacion }).ToList();
            }

            return info;
        }

        public async Task<bool> CambioEstadoAsync(string sku, string nombre, double precio, string estado)
        {
            if (_env.IsDevelopment())
            {
                return true;
            }

            string token = await GetOrGenerateTokenAsync();
            string networks = await GetFirstSocialNetworkAsync(token);

            // Note: Legacy endpoint was "/index.php/rest_movil/link"
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("key", _options.Key), // uses 'key' instead of 'llave'
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("codigo", sku),
                new KeyValuePair<string, string>("activar", estado.Equals("A", StringComparison.OrdinalIgnoreCase) ? "SI" : "NO"),
                new KeyValuePair<string, string>("nombre", nombre),
                new KeyValuePair<string, string>("nombre_interno", nombre),
                new KeyValuePair<string, string>("precio", precio.ToString()),
                new KeyValuePair<string, string>("redes", networks),
                new KeyValuePair<string, string>("cuota", "0")
            });
            var response = await _httpClient.PostAsync("/index.php/rest_movil/link", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VisaLinkResponse>();
            return result != null && result.Result == "success";
        }

        #region Helper classes for json mapping
        private class VisaGenericResponse
        {
            [JsonPropertyName("result")]
            public string Result { get; set; } = string.Empty;
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
        }

        private class VisaLoginResponse : VisaGenericResponse
        {
            [JsonPropertyName("data")]
            public TokenData? Data { get; set; }

            public class TokenData
            {
                [JsonPropertyName("token")]
                public string Token { get; set; } = string.Empty;
            }
        }

        private class VisaNetworkResponse : VisaGenericResponse
        {
            [JsonPropertyName("data")]
            public List<NetworkData> Data { get; set; } = new();

            public class NetworkData
            {
                [JsonPropertyName("codigo")]
                public string Codigo { get; set; } = string.Empty;
                [JsonPropertyName("nombre")]
                public string Nombre { get; set; } = string.Empty;
            }
        }

        private class VisaLinkResponse : VisaGenericResponse
        {
            [JsonPropertyName("data")]
            public List<LinkData> Data { get; set; } = new();

            public class LinkData
            {
                [JsonPropertyName("nombre")]
                public string Nombre { get; set; } = string.Empty;
                [JsonPropertyName("url")]
                public string Url { get; set; } = string.Empty;
            }
        }

        private class VisaLinkInfoResponse : VisaGenericResponse
        {
            [JsonPropertyName("data")]
            public DataInfo? Data { get; set; }

            public class DataInfo
            {
                [JsonPropertyName("nombre_interno")]
                public string? NombreInterno { get; set; }
                [JsonPropertyName("codigo_interno")]
                public string? CodigoInterno { get; set; }
                [JsonPropertyName("token")]
                public string? Token { get; set; }
                [JsonPropertyName("moneda")]
                 public string? Moneda { get; set; }
                
                [JsonPropertyName("precio")]
                public double Precio { get; set; }
                [JsonPropertyName("estado")]
                public string? Estado { get; set; }
                [JsonPropertyName("ventas")]
                public List<VentaData> Ventas { get; set; } = new();
            }

            public class VentaData
            {
                [JsonPropertyName("autorizacion")]
                public string? Autorizacion { get; set; }
            }
        }
        #endregion
    }
}
