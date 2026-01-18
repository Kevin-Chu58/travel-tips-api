using Microsoft.OpenApi.Any;

namespace TravelTipsAPI.ViewModels.db_search
{
    public class SearchResult<T>
    {
        public string? Cursor { get; set; }
        public required IEnumerable<T> Results { get; set; }
    }
}
