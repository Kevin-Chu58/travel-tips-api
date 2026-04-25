namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdTargetAnalysis
    {
        public int Id { get; set; }
        public required string Rank { get; set; }
        public double Percent { get; set; }

        public static explicit operator AdTargetAnalysis(AdTargetAnalysisForSql analytics)
        {
            return new AdTargetAnalysis
            {
                Id = analytics.Id,
                Rank = analytics.Rank.ToString(),
                Percent = analytics.Percent,
            };
        }
    }

    public class AdTargetAnalysisForSql
    {
        public int Id { get; set; }
        public long Rank { get; set; }
        public double Percent { get; set; }
    }
}
