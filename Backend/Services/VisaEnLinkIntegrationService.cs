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

        public VisaEnLinkIntegrationService(
            HttpClient httpClient, 
            ISiteRepository siteRepository, 
            IOptions<VisaEnLinkOptions> options,
            IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _siteRepository = siteRepository;
            _options = options.Value;
            _env = env;
        }

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
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("usuario", _options.UsuVisa),
                new KeyValuePair<string, string>("clave", _options.ClaveVisa)
            });

            var response = await _httpClient.PostAsync("/api/login", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VisaLoginResponse>();
            if (result == null || result.Result != "success" || result.Data == null || string.IsNullOrEmpty(result.Data.Token))
            {
                throw new Exception($"Error al generar token de Visa: {result?.Message ?? "Respuesta inválida"}");
            }

            return result.Data.Token;
        }

        private async Task<string> GetFirstSocialNetworkAsync(string token)
        {
            if (_env.IsDevelopment())
            {
                return "DUMMY_NETWORK";
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("token", token)
            });

            var response = await _httpClient.PostAsync("/api/network/all", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VisaNetworkResponse>();
            if (result == null || result.Result != "success" || result.Data == null || result.Data.Count == 0)
            {
                throw new Exception($"Error al consultar redes de Visa: {result?.Message ?? "Sin redes disponibles"}");
            }

            return result.Data[0].Codigo;
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

            var response = await _httpClient.PostAsync("/api/link/maintenance", linkContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VisaLinkResponse>();
            if (result == null || result.Result != "success" || result.Data == null || result.Data.Count == 0)
            {
                throw new Exception($"Error al registrar link en Visa: {result?.Message ?? "Respuesta vacía"}");
            }

            string sku = codigoInterno;
            string linkUrl = result.Data[0].Url;

            // 2. Upload advertising image if provided
            if (!string.IsNullOrEmpty(imgPublicitaria))
            {
                var imgContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("llave", _options.Key),
                    new KeyValuePair<string, string>("token", token),
                    new KeyValuePair<string, string>("codigo", sku),
                    new KeyValuePair<string, string>("imagen", imgPublicitaria),
                    new KeyValuePair<string, string>("tipo", "jpg")
                });

                var imgResponse = await _httpClient.PostAsync("/api/link/image", imgContent);
                imgResponse.EnsureSuccessStatusCode();

                var imgResult = await imgResponse.Content.ReadFromJsonAsync<VisaGenericResponse>();
                if (imgResult == null || imgResult.Result != "success")
                {
                    throw new Exception($"Error al asociar imagen publicitaria en Visa: {imgResult?.Message ?? "Falla en carga"}");
                }
            }

            return (sku, linkUrl);
        }

        public async Task<VisaLinkInfo?> ConsultaLinkAsync(string sku)
        {
            // Development bypass as requested
            if (_env.IsDevelopment())
            {
                return new VisaLinkInfo
                {
                    Sku = sku,
                    LinkUrl = $"https://dummy.link/{sku}",
                    Estado = "PAID",
                    Nombre = "Dummy Link (Development Mode)",
                    Monto = 100.00,
                    Autorizacion = "8956540" // Dummy auth code from legacy code
                };
            }

            string token = await GetOrGenerateTokenAsync();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("llave", _options.Key),
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("codigo", sku)
            });

            var response = await _httpClient.PostAsync("/api/link/single", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VisaLinkInfoResponse>();
            if (result == null || result.Result != "success" || result.Data == null)
            {
                throw new Exception($"Error al consultar link en Visa: {result?.Message ?? "No se encontraron datos"}");
            }

            var info = new VisaLinkInfo
            {
                Sku = result.Data.CodigoInterno ?? string.Empty,
                LinkUrl = result.Data.Token ?? string.Empty,
                Estado = result.Data.Estado ?? string.Empty,
                Nombre = result.Data.NombreInterno ?? string.Empty,
                Monto = result.Data.Precio
            };

            if (result.Data.Ventas != null && result.Data.Ventas.Count > 0)
            {
                info.Autorizacion = result.Data.Ventas[0].Autorizacion ?? string.Empty;
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
