

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
        private readonly ILogger<IpapiProvider> _logger;

        public IpapiProvider(HttpClient httpClient, IConfiguration config, ILogger<IpapiProvider> logger)
        {
            _httpClient = httpClient;
            _baseUrl = config["GeoProvider:Ipapi:BaseUrl"] ?? "https://ipapi.co";
            _logger = logger;
        }

        public async Task<GeoResult?> LookupIpAsync(string ip, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/{ip}/json/";
            try
            {
                var response = await _httpClient.GetAsync($"https://ipapi.co/{ip}/json/", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 429)
                        _logger.LogWarning("Rate limit reached for ipapi.co");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonConvert.DeserializeObject<GeoResult>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch IP geolocation for {Ip}", ip);
                return null;
            }
        }
    }
}
