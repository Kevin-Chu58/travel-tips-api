using Microsoft.IdentityModel.Tokens;
using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    /// <summary>
    /// A separation from the mixture of Attraction and Highlight,
    /// slowly replacing all the old AttractionViewModels
    /// </summary>
    public class Attraction2ViewModel
    {
        public int? Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public long OsmId { get; set; }
        public required string OsmType { get; set; }

        public static explicit operator Attraction2ViewModel(Attraction attraction)
        {
            var name = attraction.Name.Trim();
            var addresses = attraction.Address.Split(",");
            var count = Math.Min(addresses.Length, 2);
            var addressName = string.Join(", ", addresses[..count]);

            var attractionViewModel = new Attraction2ViewModel
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

        public static explicit operator Attraction(Attraction2ViewModel attractionViewModel)
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
