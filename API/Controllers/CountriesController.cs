using Business.Interfaces;
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

        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] BlockCountryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CountryCode))
                return BadRequest(new MessageResponse("Country code is required."));

            var code = request.CountryCode.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(code, @"^[A-Z]{2}$"))
                return BadRequest(new MessageResponse("Invalid country code format. Use ISO Alpha-2."));

            if (_store.IsBlocked(code))
                return Conflict(new MessageResponse($"Country '{code}' is already blocked."));

            if (!_store.Add(code, request.CountryName))
                return StatusCode(500, new MessageResponse("Could not add country to blocked list."));

            return Ok(new MessageResponse($"Country '{code}' has been blocked successfully."));
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return BadRequest(new MessageResponse("Country code is required."));

            var code = countryCode.Trim().ToUpperInvariant();

            if (!_store.IsBlocked(code))
                return NotFound(new MessageResponse($"Country '{code}' is not in the blocked list."));

            if (!_store.Remove(code))
                return StatusCode(500, new MessageResponse($"Failed to remove '{code}' from the blocked list."));

            return Ok(new MessageResponse($"Country '{code}' has been unblocked successfully."));
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            // Avoid premature ToList() to keep memory footprint low
            var query = _store.GetAll();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                                      || (c.Name != null && c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .Select(c => new BlockedCountryResponse(c.Code, c.Name, c.IsTemporary, c.ExpiresAt))
                             .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new PagedResult<BlockedCountryResponse>(items, page, pageSize, totalCount, totalPages));
        }

        [HttpPost("temporal-block")]
        public IActionResult TemporalBlock([FromBody] TemporalBlockRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CountryCode))
                return BadRequest(new MessageResponse("Country code is required."));

            var code = request.CountryCode.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(code, @"^[A-Z]{2}$"))
                return BadRequest(new MessageResponse("Invalid country code format (ISO Alpha-2)."));

            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
                return BadRequest(new MessageResponse("Duration must be between 1 and 1440 minutes."));

            if (_store.IsBlocked(code))
                return Conflict(new MessageResponse($"Country '{code}' is already blocked."));

            var duration = TimeSpan.FromMinutes(request.DurationMinutes);
            if (!_store.AddTemporary(code, request.CountryName, duration))
                return StatusCode(500, new MessageResponse("Failed to add temporary block."));

            var expiresAt = DateTime.UtcNow.Add(duration);
            _logger.LogInformation("Country {Code} temporarily blocked until {ExpiresAt}.", code, expiresAt);

            return Ok(new TemporalBlockResponse($"Country '{code}' has been temporarily blocked.", expiresAt));
        }
    }
}

}
