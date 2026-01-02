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
                int? limit = 20
            );
        }

        public interface IHereMapLookupService
        {
            Task<HerePlace> LookupPlaceByIdAsync(string hereId);
        }

        public interface IHereMapRoutingService
        {
            Task<HereRoutingResponse?> GetRouteAsync(HereRoutingInput routeInput);
            Task<IEnumerable<HereRoutingResponse?>> GetRoutesAsync(
                List<HereRoutingInput> routeInputs
            );
        }
    }
}
