using TravelTipsAPI.ViewModels.db_gospel;

namespace TravelTipsAPI.ViewModels.db_gospel
{
    public class WritingLabelSearchResult
    {
        public IEnumerable<WritingLabelViewModel>? Categories { get; set; }
        public IEnumerable<WritingLabelViewModel>? Topics { get; set; }
    }
}
