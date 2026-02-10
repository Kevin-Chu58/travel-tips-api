namespace TravelTipsAPI.Constants
{
    public class OrderBy
    {
        public class TripOrderBy
        {
            public enum TripOrderByEnum
            {
                Newest,
                Oldest,
                MostBookmarked,
                LeastBookmarked,
            };
        }

        public class HighlightOrderBy
        {
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
