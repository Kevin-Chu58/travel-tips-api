using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.ViewModels.db_sermon
{
    public class SermonViewModel
    {
        public int Id { get; set; }
        public required UserSimpleViewModel CreatedBy { get; set; }
        public required string Title { get; set; }
        public SermonLabelCompleteViewModel? Label { get; set; }
        public string? Content { get; set; }
        public DateOnly PublishAt { get; set; }
        public bool IsBanner { get; set; }
    }
}
