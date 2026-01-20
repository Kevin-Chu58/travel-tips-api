namespace TravelTipsAPI.Constants
{
    public class OrderBy
    {
        public class HighlightOrderBy
        {
            public static readonly string Newest = "newest";
            public static readonly string Oldest = "oldest";
            public static readonly string MostUsed = "most_used";
            public static readonly string LeastUsed = "least_used";

            public enum HighlightOrderByEnum
            {
                Newest,
                Oldest,
                MostUsed,
                LeastUsed,
            };
        }
    }
}
