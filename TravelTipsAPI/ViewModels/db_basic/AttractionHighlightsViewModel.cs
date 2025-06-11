using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionHighlightsViewModel
    {
        // attractions
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public long OsmId { get; set; }
        public required string OsmType { get; set; }
        public required IEnumerable<HighlightViewModel> Highlights { get; set; }

        public static explicit operator AttractionHighlightsViewModel(Attraction attraction)
        {
            var ahViewModel = new AttractionHighlightsViewModel
            {
                Id = attraction.Id,
                Name = attraction.Name,
                Address = attraction.Address,
                Lng = attraction.Lng,
                Lat = attraction.Lat,
                OsmId = attraction.OsmId,
                OsmType = attraction.OsmType,
                Highlights = [],
            };

            return ahViewModel;
        }
    }
}
