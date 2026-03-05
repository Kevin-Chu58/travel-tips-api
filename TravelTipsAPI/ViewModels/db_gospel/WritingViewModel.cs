using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.ViewModels.db_gospel
{
    public class WritingViewModel
    {
        public int Id { get; set; }
        public required UserSimpleViewModel CreatedBy { get; set; }
        public required string Title { get; set; }
        public WritingLabelCompleteViewModel? Label { get; set; }
        public string? Content { get; set; }
        public DateOnly PublishAt { get; set; }
        public bool IsBanner { get; set; }
    }
}
