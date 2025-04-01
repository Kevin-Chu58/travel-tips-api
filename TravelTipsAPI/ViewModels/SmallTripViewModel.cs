using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels
{
    public class SmallTripViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int TripId { get; set; }

        public static explicit operator SmallTripViewModel?(SmallTrip? smallTrip)
        {
            if (smallTrip == null) return null;

            var smallTripViewModel = new SmallTripViewModel
            {
                Id = smallTrip.Id,
                Name = smallTrip.Name,
                Description = smallTrip.Description,
                TripId = smallTrip.TripId,
            };

            return smallTripViewModel;
        }
    }
}
