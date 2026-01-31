using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.BackgroundServices
{
    public class HighlightUsageRebuildService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HighlightUsageRebuildService> _logger;

        public HighlightUsageRebuildService(
            IServiceProvider serviceProvider,
            ILogger<HighlightUsageRebuildService> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Optional: delay on startup
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Rebuilding highlight usage counts...");

                    await RebuildUsageCounts(stoppingToken);

                    _logger.LogInformation("Highlight usage counts rebuild complete.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild highlight useage counts");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task RebuildUsageCounts(CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TravelTipsContext>();

            // optional: prevent multiple instances running simultaneously
            await db.Database.ExecuteSqlRawAsync(
                @"
                DECLARE @result int;
                EXEC @result = sp_getapplock 
                    @Resource = 'RebuildHighlightUsage',
                    @LockMode = 'Exclusive',
                    @LockTimeout = 0;

                IF @result < 0
                BEGIN
                    PRINT 'Another instance is running. Exiting.';
                    RETURN;
                END
            "
            );

            // main SQL: rebuild highlight usage counts
            await db.Database.ExecuteSqlRawAsync(
                @"
                UPDATE h
                SET UsageCount = COALESCE(sub.Count, 0)
                FROM db_basic.Highlights h
                LEFT JOIN (
                    SELECT HighlightId, COUNT(*) AS Count
                    FROM db_basic.TripAttractionOrders
                    WHERE HighlightId IS NOT NULL
                    GROUP BY HighlightId
                ) sub ON sub.HighlightId = h.Id;
            ",
                token
            );
        }
    }
}
