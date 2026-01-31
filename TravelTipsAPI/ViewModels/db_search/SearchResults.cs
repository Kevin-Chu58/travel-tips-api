using Microsoft.OpenApi.Any;

namespace TravelTipsAPI.ViewModels.db_search
{
    public class SearchResults<T>
    {
        public string? Cursor { get; set; }
        public int? Timestamp { get; set; }
        public required IEnumerable<T> Results { get; set; }
    }
}
