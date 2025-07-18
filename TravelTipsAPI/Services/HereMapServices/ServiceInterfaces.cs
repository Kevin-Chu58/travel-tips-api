using TravelTipsAPI.Models.TravelTipsModels;

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
    }
}
