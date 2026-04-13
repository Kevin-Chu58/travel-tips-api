using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.BackgroundWorkers
{
    public class TripBookmarkRebuildWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TripBookmarkRebuildWorker> _logger;

        public TripBookmarkRebuildWorker(
            IServiceProvider serviceProvider,
            ILogger<TripBookmarkRebuildWorker> logger
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
                    @LockOwner = 'Session',
                    @LockTimeout = 0;

                IF @result < 0
                BEGIN
                    PRINT 'Another instance is running. Exiting.';
                    RETURN;
                END
            "
            );

            // main SQL: rebuild trip bookmark counts
            await db.Database.ExecuteSqlRawAsync(
                @"
                SELECT TripId, COUNT(*) AS Cnt 
                INTO #TripBookmarkCounts 
                FROM db_search.Bookmarks 
                GROUP BY TripId; 

                CREATE INDEX IDX_TripBookmarkCounts_TripId 
                ON #TripBookmarkCounts(TripId); 
                
                WHILE 1 = 1 
                BEGIN 
                    ;WITH cte AS ( 
                        SELECT TOP (200) t.Id 
                        FROM db_basic.Trips t 
                        LEFT JOIN #TripBookmarkCounts c 
                        ON c.TripId = t.Id 
                        WHERE ISNULL(t.BookmarkCount, 0) <> ISNULL(c.Cnt, 0) 
                        ORDER BY t.Id 
                    ) 
                    UPDATE t 
                    SET BookmarkCount = ISNULL(c.Cnt, 0) 
                    FROM db_basic.Trips t 
                    JOIN cte ON cte.Id = t.Id 
                    LEFT JOIN #TripBookmarkCounts c ON c.TripId = t.Id; 

                IF @@ROWCOUNT = 0 BREAK; 
            END
            ",
                token
            );
        }
    }
}
