using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderPostViewModel
    {
        public int DayId { get; set; }
        public int Order {  get; set; }
        public int AttractionId { get; set; }
        public int EstimateTime { get; set; }
        public bool IsDrivePreferred { get; set; }
        public bool IsBikePreferred { get; set; }
        public bool IsOnFootPreferred { get; set; }

        public TripAttractionOrder ToTripAttractionOrder(int createdBy)
        {
            var newTripAttractionOrder = new TripAttractionOrder
            {
                Id = new int(),
                DayId = DayId,
                Order = Order,
                AttractionId = AttractionId,
                EstimateTime = EstimateTime,
                IsDrivePreferred = IsDrivePreferred,
                IsBikePreferred = IsBikePreferred,
                IsOnFootPreferred = IsOnFootPreferred,
                CreatedBy = createdBy
            };

            return newTripAttractionOrder;
        }
    }
}
