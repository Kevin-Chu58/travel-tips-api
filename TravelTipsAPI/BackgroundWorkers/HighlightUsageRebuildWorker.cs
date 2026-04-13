using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.BackgroundWorkers
{
    public class HighlightUsageRebuildWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HighlightUsageRebuildWorker> _logger;

        public HighlightUsageRebuildWorker(
            IServiceProvider serviceProvider,
            ILogger<HighlightUsageRebuildWorker> logger
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
                    _logger.LogError(ex, "Failed to rebuild highlight usage counts.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
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
                    @LockOwner = 'Session',
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
                SELECT HighlightId, COUNT(*) AS Cnt 
                INTO #HighlightCounts 
                FROM db_basic.TripAttractionOrders 
                WHERE HighlightId IS NOT NULL 
                GROUP BY HighlightId; 

                CREATE INDEX IDX_HighlightCounts_HighlightId
                ON #HighlightCounts(HighlightId); 
                
                WHILE 1 = 1 
                BEGIN 
                    ;WITH cte AS ( 
                        SELECT TOP (200) h.Id
                        FROM db_basic.Highlights h 
                        LEFT JOIN #HighlightCounts c 
                        ON c.HighlightId = h.Id 
                        WHERE ISNULL(h.UsageCount, 0) <> ISNULL(c.Cnt, 0) 
                        ORDER BY h.Id 
                    ) 
                    UPDATE h 
                    SET UsageCount = ISNULL(c.Cnt, 0) 
                    FROM db_basic.Highlights h 
                    JOIN cte ON cte.Id = h.Id 
                    LEFT JOIN #HighlightCounts c ON c.HighlightId = h.Id; 

                IF @@ROWCOUNT = 0 BREAK; 
            END
            ",
                token
            );
        }
    }
}
