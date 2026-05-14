using global::SmartLogistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Infrastructure.Services.Background
{
    // خدمة بتشتغل في الخلفية عشان تنضف الداتا القديمة اللي ملهاش لزمة
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
                // بنعمل التنضيف
                await CleanDatabaseAsync(stoppingToken);

                // بنستنى 24 ساعة قبل ما نكرر العملية تاني
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanDatabaseAsync(CancellationToken ct)
        {
            // بما إن الـ Background Service هي Singleton فلازم نعمل Scope عشان نجيب الـ DbContext
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                _logger.LogInformation("Starting database cleanup process...");

                // 1. مسح الـ Refresh Tokens اللي بقالها أكتر من شهر أو الملغية
                var tokenLimit = DateTime.Now.AddDays(-30);
                var expiredTokens = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < tokenLimit || t.RevokedAt != null)
                    .ExecuteDeleteAsync(ct);

                // 2. مسح سجلات تحرك السواقين القديمة (أكتر من 7 أيام) عشان حجم الداتا ميكبرش
                var locationLimit = DateTime.Now.AddDays(-7);
                var oldLocations = await db.DriverLocations
                    .Where(l => l.RecordedAt < locationLimit)
                    .ExecuteDeleteAsync(ct);

                _logger.LogInformation($"Cleanup finished. Removed {expiredTokens} tokens and {oldLocations} locations.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Oops! Something went wrong during cleanup: {ex.Message}");
            }
        }
    }
}