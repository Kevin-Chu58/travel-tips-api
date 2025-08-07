using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderPostViewModel
    {
        public int DayId { get; set; }
        public int AttractionId { get; set; }
        public int? HighlightId { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }

        public TripAttractionOrder ToTripAttractionOrder(int createdBy)
        {
            var newTripAttractionOrder = new TripAttractionOrder
            {
                Id = new int(),
                DayId = DayId,
                AttractionId = AttractionId,
                HighlightId = HighlightId,
                Start = Start,
                End = End,
                CreatedBy = createdBy,
            };

            return newTripAttractionOrder;
        }
    }
}
