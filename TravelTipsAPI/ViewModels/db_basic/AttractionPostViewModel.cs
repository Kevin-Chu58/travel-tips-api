using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public long OsmId { get; set; }
        public int? LinkId { get; set; }

        public Attraction ToAttraction()
        {
            return new Attraction
            {
                Id = new int(),
                Name = Name.Trim(),
                Address = Address.Trim(),
                OsmId = OsmId,
            };
        }

        public Highlight ToHighlight(int attractionId, int? createdBy = null)
        {
            var highlight = new Highlight
            {
                Id = new int(),
                AttractionId = attractionId,
                CreatedBy = createdBy,
            };

            if (createdBy != null)
            {
                highlight.Description = Description?.Trim();
                highlight.LinkId = LinkId;
            }

            return highlight;
        }
    }
}
