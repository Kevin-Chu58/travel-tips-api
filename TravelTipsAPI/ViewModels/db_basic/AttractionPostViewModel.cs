using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPostViewModel
    {
        // attractions
        public long OsmId { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }

        // highlights
        public string? Description { get; set; }
        public int? LinkId { get; set; }

        public static explicit operator AttractionViewModel(AttractionPostViewModel model)
        {
            var attraction = new AttractionViewModel
            {
                OsmId = model.OsmId,
                Lng = model.Lng,
                Lat = model.Lat,
                Name = model.Name,
                Address = model.Address,
            };

            return attraction;
        }

        public Attraction ToAttraction()
        {
            return new Attraction
            {
                Id = new int(),
                Name = Name.Trim(),
                Address = Address.Trim(),
                Lng = Lng,
                Lat = Lat,
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
