using TravelTipsAPI.ViewModels.nominatim;

namespace TravelTipsAPI.Services.NominatimServices
{
    public class NominatimSchema
    {
        public interface INominatimService
        {
            Task<IEnumerable<OsmEntity>> GetOsmEntitiesByNameAsync(string search);
        }
    }
}
