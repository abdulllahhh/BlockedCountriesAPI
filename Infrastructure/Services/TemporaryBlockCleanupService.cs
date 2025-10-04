using Business.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services
{
    public class TemporaryBlockCleanupService : BackgroundService
    {
        private readonly IBlockedCountriesStore _store;
        private readonly ILogger<TemporaryBlockCleanupService> _logger;

        public TemporaryBlockCleanupService(IBlockedCountriesStore store, ILogger<TemporaryBlockCleanupService> logger)
        {
            _store = store;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporary block cleanup service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _store.CleanupExpired();
                    _logger.LogInformation("Expired temporary blocks cleaned up at: {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cleanup of temporary blocks.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Temporary block cleanup service stopped.");
        }
    }
}
