using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.Utils
{
    public class ModelUtils
    {
        // Trips
        public List<string> ValidateTripPostViewModel(TripPostViewModel trip)
        {
            List<string> invalidParams = [];

            if (trip.Name.Length > 50)
                invalidParams.Add("name");
            if (trip.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }

        public List<string> ValidateTripPatchViewModel(TripPatchViewModel trip)
        {
            List<string> invalidParams = [];

            if (trip.Name?.Length > 50)
                invalidParams.Add("name");
            if (trip.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }
    }
}
