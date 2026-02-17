namespace TravelTipsAPI.ViewModels.db_search
{
    public class SearchCursors
    {
        public class GeneralCursor
        {
            public int Id { get; set; }
        }

        public class TripCursor
        {
            public int? Id { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int? BookmarkCount { get; set; }
        }

        public class HighlightCursor
        {
            public int Id { get; set; }
            public int? UsageCount { get; set; }
        }
    }
}
