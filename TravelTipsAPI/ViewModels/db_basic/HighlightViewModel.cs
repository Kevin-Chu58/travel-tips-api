using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class HighlightViewModel
    {
        public int Id { get; set; }
        public int AttractionId { get; set; }
        public string? Description { get; set; }
        public UserViewModel? CreatedBy { get; set; }

        public static explicit operator HighlightViewModel(Highlight highlight)
        {
            var highlightViewModel = new HighlightViewModel
            {
                Id = highlight.Id,
                AttractionId = highlight.AttractionId,
                Description = highlight.Description,
            };

            return highlightViewModel;
        }
    }
}
