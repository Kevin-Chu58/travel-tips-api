//using Microsoft.IdentityModel.Tokens;
//using TravelTipsAPI.Models.TravelTipsModels;

//namespace TravelTipsAPI.ViewModels.db_basic
//{
//    public class AttractionHighlightsViewModel
//    {
//        // attractions
//        public int Id { get; set; }
//        public required string Name { get; set; }
//        public required string Address { get; set; }
//        public decimal Lng { get; set; }
//        public decimal Lat { get; set; }
//        public long OsmId { get; set; }
//        public required string OsmType { get; set; }
//        public required IEnumerable<HighlightViewModel> Highlights { get; set; }

//        public static explicit operator AttractionHighlightsViewModel(Attraction attraction)
//        {
//            var name = attraction.Name.Trim();
//            var addresses = attraction.Address.Split(",");
//            var count = Math.Min(addresses.Length, 2);
//            var addressName = string.Join(", ", addresses[..count]);

//            var ahViewModel = new AttractionHighlightsViewModel
//            {
//                Id = attraction.Id,
//                Name = name.IsNullOrEmpty() ? addressName : name,
//                Address = attraction.Address,
//                Lng = attraction.Lng,
//                Lat = attraction.Lat,
//                OsmId = attraction.OsmId,
//                OsmType = attraction.OsmType,
//                Highlights = [],
//            };

//            return ahViewModel;
//        }
//    }
//}
