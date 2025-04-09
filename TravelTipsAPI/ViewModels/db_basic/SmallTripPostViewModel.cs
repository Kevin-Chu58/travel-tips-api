using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class SmallTripPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int TripId { get; set; }

        public SmallTrip ToSmallTrip()
        {
            var smallTrip = new SmallTrip
            {
                Id = new int(),
                Name = Name.Trim(),
                Description = Description?.Trim(),
                TripId = TripId,
            };

            return smallTrip;
        }
    }
}
