using TravelTipsAPI.ViewModels.db_sermon;

namespace TravelTipsAPI.ViewModels.db_gospel
{
    public class SermonLabelSearchResult
    {
        public IEnumerable<SermonLabelViewModel>? Categories { get; set; }
        public IEnumerable<SermonLabelViewModel>? Topics { get; set; }
    }
}
