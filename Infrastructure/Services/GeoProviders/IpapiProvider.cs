

using Business.Interfaces;
using Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Services.GeoProviders
{
    public class IpapiProvider : IGeoProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string? _apiKey;
        private readonly ILogger<IpapiProvider> _logger;

        public IpapiProvider(HttpClient httpClient, IConfiguration config, ILogger<IpapiProvider> logger)
        {
            _httpClient = httpClient;
            _baseUrl = config["GeoProvider:Ipapi:BaseUrl"] ?? "https://ipapi.co";
            _apiKey = config["GeoProvider:Ipapi:ApiKey"];
            _logger = logger;
        }

        public async Task<GeoResult?> LookupIpAsync(string ip, CancellationToken cancellationToken = default)
        {
            // Some providers use API key as a query param or header. ipapi.co (paid) can use key=...
            var url = $"{_baseUrl.TrimEnd('/')}/{ip}/json/";
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                url += $"?key={_apiKey}";
            }

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 429)
                    {
                        _logger.LogWarning("Rate limit reached for provider {BaseUrl}. IP: {Ip}", _baseUrl, ip);
                    }
                    else
                    {
                        _logger.LogError("Provider {BaseUrl} returned {StatusCode} for IP {Ip}", _baseUrl, response.StatusCode, ip);
                    }
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<GeoResult>(content);

                if (result == null)
                {
                    _logger.LogWarning("Provider {BaseUrl} returned empty or invalid JSON for IP {Ip}", _baseUrl, ip);
                }

                return result;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout occurred while calling provider {BaseUrl} for IP {Ip}", _baseUrl, ip);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch IP geolocation for {Ip} from {BaseUrl}", ip, _baseUrl);
                return null;
            }
        }
    }
}

