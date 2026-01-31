namespace TravelTipsAPI.ViewModels.db_search
{
    public class SearchResult<T>
    {
        public int? Timestamp { get; set; }
        public required T Result { get; set; }
    }
}
