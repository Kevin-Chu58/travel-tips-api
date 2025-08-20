using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.HereMap;

namespace TravelTipsAPI.Services.HereMapServices
{
    public class HereMapSchema
    {
        public interface IHereMapDiscoverService
        {
            Task<IEnumerable<Attraction>> SearchPlaceByNameAsync(
                string query,
                decimal lat,
                decimal lng,
                int? limit
            );
        }

        public interface IHereMapLookupService
        {
            Task<Attraction> LookupPlaceByIdAsync(string hereId);
        }

        public interface IHereMapRoutingService
        {
            Task<HereRoutingResponse> GetRouteAsync(
                string transportMode,
                double originLat,
                double originLng,
                double destinationLat,
                double destinationLng
            );
        }
    }
}
