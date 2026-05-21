using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class UrlShortenerOptions
    {
        public string Server { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string DomainId { get; set; } = "76b6fd2fb2814a729c67d881a118181c"; // Fallback domain ID from legacy
    }

    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly HttpClient _httpClient;
        private readonly UrlShortenerOptions _options;

        public UrlShortenerService(HttpClient httpClient, IOptions<UrlShortenerOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            if (Uri.TryCreate(_options.Server, UriKind.Absolute, out var baseUri))
            {
                _httpClient.BaseAddress = baseUri;
            }
            _httpClient.DefaultRequestHeaders.Add("apikey", _options.ApiKey);
        }

        public async Task<string> ShortenUrlAsync(string destinationUrl)
        {
            var payload = new[]
            {
                new
                {
                    destination = destinationUrl,
                    domain = new
                    {
                        fullName = _options.Domain // e.g. "lc.bpgt.com.gt"
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/shortlinks/shorten", payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<List<ShortenResponseItem>>();
            if (result == null || result.Count == 0 || string.IsNullOrEmpty(result[0].ShortUrl))
            {
                throw new Exception("El acortador de URLs no retornó un link corto válido.");
            }

            return result[0].ShortUrl ?? string.Empty;
        }

        public async Task<List<string>> ShortenUrlsBulkAsync(List<string> destinationUrls)
        {
            var payload = new List<object>();
            foreach (var url in destinationUrls)
            {
                payload.Add(new
                {
                    destination = url,
                    domain = new
                    {
                        id = _options.DomainId // standard domain id from legacy for bulk
                    }
                });
            }

            // Legacy bulk shortening makes a POST to "/shortlinks/shorten" sending the array
            var response = await _httpClient.PostAsJsonAsync("/shortlinks/shorten", payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<List<ShortenResponseItem>>();
            var shortUrls = new List<string>();
            if (result != null)
            {
                foreach (var item in result)
                {
                    shortUrls.Add(item.ShortUrl ?? string.Empty);
                }
            }

            return shortUrls;
        }

        private class ShortenResponseItem
        {
            [JsonPropertyName("shortUrl")]
            public string? ShortUrl { get; set; }
        }
    }
}
