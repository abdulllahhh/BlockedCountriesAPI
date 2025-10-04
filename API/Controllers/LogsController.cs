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
            var allLogs = _logStore.GetAll()
                .Where(l => l.IsBlocked)
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new BlockedAttemptResponse(
                    l.Ip, l.CountryCode, l.CountryName, l.Timestamp, l.UserAgent))
                .ToList();

            var totalCount = allLogs.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = allLogs.Skip((page - 1) * pageSize).Take(pageSize);

            var result = new PagedResult<BlockedAttemptResponse>(
                Items: items,
                Page: page,
                PageSize: pageSize,
                TotalCount: totalCount,
                TotalPages: totalPages
            );

            return Ok(result);
        }
    }
}
