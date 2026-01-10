using System.ComponentModel.DataAnnotations;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class HighlightPostViewModel
    {
        public int AttractionId { get; set; }

        [MinLength(1)]
        [MaxLength(500)]
        public required string Description { get; set; }

        public Highlight ToHighlight(int createdBy)
        {
            var highlight = new Highlight
            {
                Id = new int(),
                AttractionId = AttractionId,
                Description = Description,
                CreatedBy = createdBy,
            };

            return highlight;
        }
    }
}
