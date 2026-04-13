using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.BackgroundWorkers
{
    public class TripCountRebuildWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TripCountRebuildWorker> _logger;

        public TripCountRebuildWorker(
            IServiceProvider serviceProvider,
            ILogger<TripCountRebuildWorker> logger
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
                    _logger.LogInformation("Rebuilding trip counts...");

                    await RebuildTripCounts(stoppingToken);

                    _logger.LogInformation("Trip counts rebuild complete.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild trip counts.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task RebuildTripCounts(CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TravelTipsContext>();

            // optional: prevent multiple instances running simultaneously
            await db.Database.ExecuteSqlRawAsync(
                @"
                DECLARE @result int;
                EXEC @result = sp_getapplock 
                    @Resource = 'RebuildTripCount',
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

            // main SQL: rebuild trip counts
            await db.Database.ExecuteSqlRawAsync(
                @"
                SELECT CreatedBy, COUNT(*) AS Cnt 
                INTO #TripCounts 
                FROM db_basic.Trips 
                GROUP BY CreatedBy; 

                CREATE INDEX IDX_TripCounts_CreatedBy 
                ON #TripCounts(CreatedBy); 
                
                WHILE 1 = 1 
                BEGIN 
                    ;WITH cte AS ( 
                        SELECT TOP (200) u.UserId as Id
                        FROM db_basic.UserSubExtends u 
                        LEFT JOIN #TripCounts c 
                        ON c.CreatedBy = u.UserId 
                        WHERE u.TripCount <> ISNULL(c.Cnt, 0) 
                        ORDER BY u.UserId 
                    ) 
                    UPDATE u 
                    SET u.TripCount = ISNULL(c.Cnt, 0) 
                    FROM db_basic.UserSubExtends u 
                    JOIN cte ON cte.Id = u.UserId 
                    LEFT JOIN #TripCounts c ON c.CreatedBy = u.UserId; 

                IF @@ROWCOUNT = 0 BREAK; 
            END
            ",
                token
            );
        }
    }
}
