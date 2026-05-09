
using global::SmartLogistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace SmartLogistics.Infrastructure.Services.Background
{
    
   
    /// <summary>
    /// Background service that periodically cleans up stale data:
    /// - Expired refresh tokens older than 30 days
    /// - Driver location history older than 7 days
    /// Runs every 24 hours.
    /// </summary>
    public class DataCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public DataCleanupService(IServiceScopeFactory scopeFactory, ILogger<DataCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DataCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await DoCleanupAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task DoCleanupAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var cutoffTokens = DateTime.UtcNow.AddDays(-30);
                var deletedTokens = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < cutoffTokens || t.RevokedAt != null)
                    .ExecuteDeleteAsync(ct);

                var cutoffLocation = DateTime.UtcNow.AddDays(-7);
                var deletedLocations = await db.DriverLocations
                    .Where(l => l.RecordedAt < cutoffLocation)
                    .ExecuteDeleteAsync(ct);

                _logger.LogInformation("Cleanup complete: {Tokens} expired tokens, {Locations} old location records removed.",
                    deletedTokens, deletedLocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled data cleanup.");
            }
        }
    }

  
}
