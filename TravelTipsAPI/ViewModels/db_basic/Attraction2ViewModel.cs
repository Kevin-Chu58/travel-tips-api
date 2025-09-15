using Microsoft.IdentityModel.Tokens;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    /// <summary>
    /// A separation from the mixture of Attraction and Highlight,
    /// slowly replacing all the old AttractionViewModels
    /// </summary>
    public class Attraction2ViewModel
    {
        public int? Id { get; set; }
        public required string HereId { get; set; }
        public required string Title { get; set; }
        public required string ResultType { get; set; }
        public string? Category { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public int? NumHighlights { get; set; }

        public static explicit operator Attraction2ViewModel(Attraction attraction)
        {
            var attractionViewModel = new Attraction2ViewModel
            {
                // NumHighlights is not set
                Id = attraction.Id,
                HereId = attraction.HereId,
                Title = attraction.Title,
                ResultType = attraction.ResultType,
                Category = attraction.Category,
                Lat = attraction.Lat,
                Lng = attraction.Lng,
                Address = attraction.Address,
                City = attraction.City,
                State = attraction.State,
                Country = attraction.Country,
            };

            return attractionViewModel;
        }

        public static explicit operator Attraction(Attraction2ViewModel attractionViewModel)
        {
            var attraction = new Attraction
            {
                Id = attractionViewModel.Id ?? new int(),
                HereId = attractionViewModel.HereId,
                Title = attractionViewModel.Title,
                Category = attractionViewModel.Category,
                Lat = attractionViewModel.Lat,
                Lng = attractionViewModel.Lng,
                Address = attractionViewModel.Address,
                City = attractionViewModel.City,
                State = attractionViewModel.State,
                Country = attractionViewModel.Country,
            };

            return attraction;
        }
    }
}
