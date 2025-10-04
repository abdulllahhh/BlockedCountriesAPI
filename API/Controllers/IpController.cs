using API.Helpers;
using Business.Interfaces;
using Business.Record;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<IpController> _logger;

        public IpController(
        IGeoProvider geoProvider,
        IBlockedCountriesStore blockedStore,
        IRequestLogStore logStore,
        ILogger<IpController> logger)
        {
            _geoProvider = geoProvider;
            _blockedStore = blockedStore;
            _logStore = logStore;
            _logger = logger;
        }

        /// <summary>
        /// Lookup country information for the given IP address (or caller IP if omitted).
        /// </summary>
        /// <param name="ipAddress">Optional IP address.</param>
        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress, CancellationToken cancellationToken)
        {
            // If IP not provided, use caller IP
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrWhiteSpace(ipAddress))
                    return BadRequest(new { message = "Unable to determine caller IP address." });
            }

            // Validate IP format (IPv4 or IPv6)
            if (!Helper.IsValidIp(ipAddress))
                return BadRequest(new { message = "Invalid IP address format." });

            var result = await _geoProvider.LookupIpAsync(ipAddress, cancellationToken);

            if (result == null)
                return StatusCode(502, new { message = "Failed to fetch geolocation data from provider." });

            return Ok(new
            {
                result.Ip,
                result.CountryCode,
                result.CountryName,
                result.Isp
            });
        }


        /// <summary>
        /// Check if the caller's IP address belongs to a blocked country.
        /// </summary>
        [HttpGet("check-block")]
        public async Task<IActionResult> CheckIfBlocked(CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ipAddress))
                return BadRequest(new { message = "Unable to determine caller IP address." });

            if (!IPAddress.TryParse(ipAddress, out _))
                return BadRequest(new { message = "Invalid IP address format." });

            var geo = await _geoProvider.LookupIpAsync(ipAddress, cancellationToken);
            if (geo == null)
                return StatusCode(502, new { message = "Failed to fetch geolocation data." });

            var isBlocked = !string.IsNullOrEmpty(geo.CountryCode) &&
                            _blockedStore.IsBlocked(geo.CountryCode);
            var userAgent = Request.Headers.UserAgent.ToString();


            // Log the attempt
            _logStore.AddLog(new IpCheckLog(
                Ip: ipAddress,
                CountryCode: geo.CountryCode,
                CountryName: geo.CountryName,
                IsBlocked: isBlocked,
                UserAgent: userAgent,
                Timestamp: DateTime.UtcNow
            ));

            _logger.LogInformation("IP {IP} ({CountryCode}) checked — Blocked: {Blocked}",
                ipAddress, geo.CountryCode, isBlocked);

            return Ok(new
            {
                ip = ipAddress,
                geo.CountryCode,
                geo.CountryName,
                isBlocked
            });
        }
    }
}
