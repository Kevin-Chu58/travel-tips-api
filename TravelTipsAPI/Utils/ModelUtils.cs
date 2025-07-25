using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.HereMap;

namespace TravelTipsAPI.Utils
{
    public class ModelUtils
    {
        // HereMap model conversion
        public static Attraction ToAttraction(HerePlace herePlace)
        {
            var category = herePlace.Categories?.FirstOrDefault(c => c.Primary == true)?.Name;

            return new Attraction
            {
                Id = new int(),
                HereId = herePlace.Id,
                Title = herePlace.Title,
                ResultType = herePlace.ResultType,
                Category = category,
                Lat = (decimal)herePlace.Position.Lat,
                Lng = (decimal)herePlace.Position.Lng,
                Address = herePlace.Address.Label,
                Country = herePlace.Address.CountryName,
                State = herePlace.Address.State,
                City = herePlace.Address.City,
            };
        }
    }
}
