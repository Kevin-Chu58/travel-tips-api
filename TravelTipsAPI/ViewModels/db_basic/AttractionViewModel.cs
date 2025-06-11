using Microsoft.IdentityModel.Tokens;
using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionViewModel
    {
        // attractions
        public int? Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public long OsmId { get; set; }
        public required string OsmType { get; set; }

        // highlights
        public string? Description { get; set; }
        public bool? IsDeprecated { get; set; }
        public int? CreatedBy { get; set; }
        public int? LinkId { get; set; }

        public static explicit operator AttractionViewModel(Attraction attraction)
        {
            var name = attraction.Name.Trim();
            var addresses = attraction.Address.Split(",");
            var addressName = string.Join(", ", addresses[0..2]);

            var attractionViewModel = new AttractionViewModel
            {
                // only fill in the info from Attraction, other info are filled by Highlight
                Id = attraction.Id,
                Name = name.IsNullOrEmpty() ? addressName : name,
                Address = attraction.Address.Trim(),
                Lng = attraction.Lng,
                Lat = attraction.Lat,
                OsmId = attraction.OsmId,
                OsmType = attraction.OsmType,
            };

            return attractionViewModel;
        }

        public static explicit operator Attraction(AttractionViewModel attractionViewModel)
        {
            var attraction = new Attraction
            {
                Id = attractionViewModel.Id ?? new int(),
                OsmId = attractionViewModel.OsmId,
                OsmType = attractionViewModel.OsmType,
                Lng = attractionViewModel.Lng,
                Lat = attractionViewModel.Lat,
                Name = attractionViewModel.Name,
                Address = attractionViewModel.Address,
            };

            return attraction;
        }
    }
}
