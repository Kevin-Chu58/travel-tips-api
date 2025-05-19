using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionViewModel
    {
        public int? Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public int? CreatedBy { get; set; }
        public long OsmId { get; set; }
        public int? LinkId { get; set; }

        public static explicit operator AttractionViewModel(Attraction attraction)
        {
            var attractionViewModel = new AttractionViewModel
            {
                // only fill in the info from Attraction, other info are filled by Highlight
                Name = attraction.Name.Trim(),
                Address = attraction.Address.Trim(),
                OsmId = attraction.OsmId,
            };

            return attractionViewModel;
        }
    }
}
