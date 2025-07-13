using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class HighlightPostViewModel
    {
        public int AttractionId { get; set; }
        public required string Description { get; set; }
        public int? LinkId { get; set; }

        public Highlight ToHighlight(int createdBy)
        {
            var highlight = new Highlight
            {
                Id = new int(),
                AttractionId = AttractionId,
                IsDeprecated = false,
                Description = Description,
                CreatedBy = createdBy,
                LinkId = LinkId,
            };

            return highlight;
        }
    }
}
