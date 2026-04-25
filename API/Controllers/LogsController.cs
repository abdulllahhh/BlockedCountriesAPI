using Business.Interfaces;
using Business.Record;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IRequestLogStore _logStore;

        public LogsController(IRequestLogStore logStore)
        {
            _logStore = logStore;
        }

        /// <summary>
        /// Returns paginated blocked attempts.
        /// </summary>
        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _logStore.GetAll().Where(l => l.IsBlocked);

            var totalCount = query.Count();
            var items = query.OrderByDescending(l => l.Timestamp)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .Select(l => new BlockedAttemptResponse(
                                 l.Ip, l.CountryCode, l.CountryName, l.Timestamp, l.UserAgent))
                             .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new PagedResult<BlockedAttemptResponse>(items, page, pageSize, totalCount, totalPages));
        }
    }
}

