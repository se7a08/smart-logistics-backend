using global::SmartLogistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Infrastructure.Services.Background
{
    
    public class DataCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(IServiceScopeFactory scopeFactory, ILogger<DataCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Data Cleanup Service has started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanDatabaseAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanDatabaseAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                _logger.LogInformation("Starting database cleanup process...");

                var tokenLimit = DateTime.UtcNow.AddDays(-30);
                var expiredTokens = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < tokenLimit || t.RevokedAt != null)
                    .ExecuteDeleteAsync(ct);

                var locationLimit = DateTime.UtcNow.AddDays(-7);
                var oldLocations = await db.DriverLocations
                    .Where(l => l.RecordedAt < locationLimit)
                    .ExecuteDeleteAsync(ct);

                _logger.LogInformation($"Cleanup finished. Removed {expiredTokens} tokens and {oldLocations} locations.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during database cleanup: {ex.Message}");
            }
        }
    }
}