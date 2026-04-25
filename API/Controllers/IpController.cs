using API.Helpers;
using Business.Interfaces;
using Business.Record;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly IGeoProvider _geoProvider;
        private readonly IBlockedCountriesStore _blockedStore;
        private readonly IRequestLogStore _logStore;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IpController> _logger;

        public IpController(
            IGeoProvider geoProvider,
            IBlockedCountriesStore blockedStore,
            IRequestLogStore logStore,
            IMemoryCache cache,
            ILogger<IpController> logger)
        {
            _geoProvider = geoProvider;
            _blockedStore = blockedStore;
            _logStore = logStore;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Lookup country information for the given IP address (or caller IP if omitted).
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress, CancellationToken cancellationToken)
        {
            var targetIp = string.IsNullOrWhiteSpace(ipAddress) 
                ? GetCallerIp() 
                : ipAddress;

            if (string.IsNullOrWhiteSpace(targetIp))
                return BadRequest(new MessageResponse("Unable to determine IP address."));

            if (!Helper.IsValidIp(targetIp))
                return BadRequest(new MessageResponse("Invalid IP address format."));

            var geo = await GetCachedGeoAsync(targetIp, cancellationToken);

            if (geo == null)
                return StatusCode(502, new MessageResponse("Failed to fetch geolocation data."));

            return Ok(new IpLookupResponse(geo.Ip ?? targetIp, geo.CountryCode, geo.CountryName, geo.Isp));
        }

        /// <summary>
        /// Check if the caller's IP address belongs to a blocked country.
        /// </summary>
        [HttpGet("check-block")]
        public async Task<IActionResult> CheckIfBlocked(CancellationToken cancellationToken)
        {
            var ipAddress = GetCallerIp();
            if (string.IsNullOrWhiteSpace(ipAddress))
                return BadRequest(new MessageResponse("Unable to determine caller IP address."));

            var geo = await GetCachedGeoAsync(ipAddress, cancellationToken);
            if (geo == null)
                return StatusCode(502, new MessageResponse("Failed to fetch geolocation data."));

            var isBlocked = !string.IsNullOrEmpty(geo.CountryCode) && _blockedStore.IsBlocked(geo.CountryCode);
            
            _logStore.AddLog(new IpCheckLog(
                Ip: ipAddress,
                CountryCode: geo.CountryCode,
                CountryName: geo.CountryName,
                IsBlocked: isBlocked,
                UserAgent: Request.Headers.UserAgent.ToString(),
                Timestamp: DateTime.UtcNow
            ));

            return Ok(new CheckBlockResponse(ipAddress, geo.CountryCode, geo.CountryName, isBlocked));
        }

        private string? GetCallerIp()
        {
            // Try to get from X-Forwarded-For first (if middleware didn't already populate RemoteIpAddress)
            // Note: UseForwardedHeaders middleware usually moves this to RemoteIpAddress
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private async Task<Business.Models.GeoResult?> GetCachedGeoAsync(string ip, CancellationToken ct)
        {
            if (Helper.IsInternalIp(ip))
            {
                return new Business.Models.GeoResult 
                { 
                    Ip = ip, 
                    CountryCode = "LOCAL", 
                    CountryName = "Internal Network", 
                    Isp = "Private/Loopback" 
                };
            }

            var cacheKey = $"geo_{ip}";
            if (_cache.TryGetValue(cacheKey, out Business.Models.GeoResult? cachedGeo))
            {
                return cachedGeo;
            }

            var geo = await _geoProvider.LookupIpAsync(ip, ct);
            if (geo != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                
                _cache.Set(cacheKey, geo, cacheOptions);
            }

            return geo;
        }

    }
}

