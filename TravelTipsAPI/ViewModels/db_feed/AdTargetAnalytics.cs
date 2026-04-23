namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdTargetAnalytics
    {
        public int Id { get; set; }
        public required string Rank { get; set; }
        public double Percent { get; set; }

        public static explicit operator AdTargetAnalytics(AdTargetAnalyticsForSql analytics)
        {
            return new AdTargetAnalytics
            {
                Id = analytics.Id,
                Rank = analytics.Rank.ToString(),
                Percent = analytics.Percent,
            };
        }
    }

    public class AdTargetAnalyticsForSql
    {
        public int Id { get; set; }
        public long Rank { get; set; }
        public double Percent { get; set; }
    }
}
