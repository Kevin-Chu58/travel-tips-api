using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.BackgroundWorkers
{
    public class UserFollowRebuildWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TripBookmarkRebuildWorker> _logger;

        public UserFollowRebuildWorker(
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
                    _logger.LogInformation("Rebuilding user follower & following counts...");

                    await RebuildFollowerCounts(stoppingToken);
                    await RebuildFollowingCounts(stoppingToken);

                    _logger.LogInformation("User follower & following counts rebuild complete.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild user follower & following counts.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task RebuildFollowerCounts(CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TravelTipsContext>();

            // optional: prevent multiple instances running simultaneously
            await db.Database.ExecuteSqlRawAsync(
                @"
                DECLARE @result int;
                EXEC @result = sp_getapplock 
                    @Resource = 'RebuildUserFollower',
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

            // main SQL: rebuild user follower counts
            await db.Database.ExecuteSqlRawAsync(
                @"
                SELECT Followed, COUNT(*) AS Cnt 
                INTO #FollowerCounts 
                FROM db_search.Followers 
                GROUP BY Followed; 

                CREATE INDEX IDX_FollowerCounts_Followed
                ON #FollowerCounts(Followed); 
                
                WHILE 1 = 1 
                BEGIN 
                    ;WITH cte AS ( 
                        SELECT TOP (200) u.Id 
                        FROM db_basic.Users u 
                        LEFT JOIN #FollowerCounts c 
                        ON c.Followed = u.Id 
                        WHERE ISNULL(u.FollowerCount, 0) <> ISNULL(c.Cnt, 0) 
                        ORDER BY u.Id 
                    ) 
                    UPDATE u 
                    SET FollowerCount = ISNULL(c.Cnt, 0) 
                    FROM db_basic.Users u 
                    JOIN cte ON cte.Id = u.Id 
                    LEFT JOIN #FollowerCounts c ON c.Followed = u.Id; 

                IF @@ROWCOUNT = 0 BREAK; 
            END
            ",
                token
            );
        }

        private async Task RebuildFollowingCounts(CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TravelTipsContext>();

            // optional: prevent multiple instances running simultaneously
            await db.Database.ExecuteSqlRawAsync(
                @"
                DECLARE @result int;
                EXEC @result = sp_getapplock 
                    @Resource = 'RebuildUserFollowing',
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

            // main SQL: rebuild user following counts
            await db.Database.ExecuteSqlRawAsync(
                @"
                SELECT Following, COUNT(*) AS Cnt 
                INTO #FollowingCounts 
                FROM db_search.Followers 
                GROUP BY Following; 

                CREATE INDEX IDX_FollowingCounts_Following 
                ON #FollowingCounts(Following); 
                
                WHILE 1 = 1 
                BEGIN 
                    ;WITH cte AS ( 
                        SELECT TOP (200) u.Id 
                        FROM db_basic.Users u 
                        LEFT JOIN #FollowingCounts c 
                        ON c.Following = u.Id 
                        WHERE ISNULL(u.FollowingCount, 0) <> ISNULL(c.Cnt, 0) 
                        ORDER BY u.Id 
                    ) 
                    UPDATE u 
                    SET FollowingCount = ISNULL(c.Cnt, 0) 
                    FROM db_basic.Users u 
                    JOIN cte ON cte.Id = u.Id 
                    LEFT JOIN #FollowingCounts c ON c.Following = u.Id; 

                IF @@ROWCOUNT = 0 BREAK; 
            END
            ",
                token
            );
        }
    }
}
