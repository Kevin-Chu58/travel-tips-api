using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.BackgroundServices
{
    public class TripBookmarkRebuildService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TripBookmarkRebuildService> _logger;

        public TripBookmarkRebuildService(
            IServiceProvider serviceProvider,
            ILogger<TripBookmarkRebuildService> logger
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
                    _logger.LogInformation("Rebuilding trip bookmark counts...");

                    await RebuildBookmarkCounts(stoppingToken);

                    _logger.LogInformation("Trip bookmark counts rebuild complete.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild trip bookmark counts.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task RebuildBookmarkCounts(CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TravelTipsContext>();

            // optional: prevent multiple instances running simultaneously
            await db.Database.ExecuteSqlRawAsync(
                @"
                DECLARE @result int;
                EXEC @result = sp_getapplock 
                    @Resource = 'RebuildTripBookmark',
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
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
                UPDATE t
                SET BookmarkCount = COALESCE(sub.Count, 0)
                FROM db_basic.Trips t
                LEFT JOIN (
                    SELECT TripId, COUNT(*) AS Count
                    FROM db_search.Bookmarks
                    GROUP BY TripId
                ) sub ON sub.TripId = t.Id;
            ",
                token
            );
        }
    }
}
