using Business.Interfaces;
using Business.Models;
using Business.Record;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly IBlockedCountriesStore _store;
        private readonly ILogger<CountriesController> _logger;


        public CountriesController(IBlockedCountriesStore store, ILogger<CountriesController> logger)
        {
            _store = store;
            _logger = logger;
        }

        /// <summary>
        /// Add a country to the blocked list.
        /// </summary>
        /// <param name="request">Contains the country code (e.g., "US")</param>
        /// <returns>Status message</returns>
        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] BlockCountryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CountryCode))
                return BadRequest(new { message = "Country code is required." });

            var code = request.CountryCode.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(code, @"^[A-Z]{2}$"))
                return BadRequest(new { message = "Invalid country code format. Use ISO Alpha-2 (e.g., 'US', 'GB')." });

            if (_store.IsBlocked(code))
                return Conflict(new { message = $"Country '{code}' is already blocked." });

            var added = _store.Add(code, request.CountryName);

            if (!added)
                return StatusCode(500, new { message = "Could not add country to blocked list." });

            return Ok(new { message = $"Country '{code}' has been blocked successfully." });
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return BadRequest(new { message = "Country code is required." });

            var code = countryCode.Trim().ToUpperInvariant();

            if (!_store.IsBlocked(code))
                return NotFound(new { message = $"Country '{code}' is not in the blocked list." });

            var removed = _store.Remove(code);

            if (!removed)
                return StatusCode(500, new { message = $"Failed to remove '{code}' from the blocked list." });

            return Ok(new { message = $"Country '{code}' has been unblocked successfully." });
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var all = _store.GetAll()
                .Where(c => string.IsNullOrWhiteSpace(search)
                    || c.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (c.Name != null && c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(c => c.Code)
                .Select(c => new BlockedCountryResponse(c.Code, c.Name, c.IsTemporary, c.ExpiresAt))
                .ToList();

            var totalCount = all.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = all.Skip((page - 1) * pageSize).Take(pageSize);

            var result = new PagedResult<BlockedCountryResponse>(
                Items: items,
                Page: page,
                PageSize: pageSize,
                TotalCount: totalCount,
                TotalPages: totalPages
            );

            return Ok(result);
        }

        /// <summary>
        /// Temporarily block a country for a given duration (in minutes).
        /// </summary>
        [HttpPost("temporal-block")]
        public IActionResult TemporalBlock([FromBody] TemporalBlockRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CountryCode))
                return BadRequest(new { message = "Country code is required." });

            var code = request.CountryCode.Trim().ToUpperInvariant();

            // Validate ISO Alpha-2 country code format
            if (!Regex.IsMatch(code, @"^[A-Z]{2}$"))
                return BadRequest(new { message = "Invalid country code format. Use ISO Alpha-2 (e.g., 'US', 'EG')." });

            // Validate duration (1–1440 minutes)
            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
                return BadRequest(new { message = "Duration must be between 1 and 1440 minutes (24 hours)." });

            // Prevent duplicate temporal blocks
            if (_store.IsBlocked(code))
                return Conflict(new { message = $"Country '{code}' is already blocked (either permanently or temporarily)." });

            var duration = TimeSpan.FromMinutes(request.DurationMinutes);

            var added = _store.AddTemporary(code, request.CountryName, duration);
            if (!added)
                return StatusCode(500, new { message = "Failed to add temporary block." });

            _logger.LogInformation("Country {Code} temporarily blocked for {Minutes} minutes.", code, request.DurationMinutes);

            return Ok(new
            {
                message = $"Country '{code}' has been temporarily blocked for {request.DurationMinutes} minutes.",
                expiresAt = DateTime.UtcNow.Add(duration)
            });
        }

    }
}
