using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_image
{
    public class ImageRelationViewModel
    {
        public int Id { get; set; }
        public int RelationId { get; set; }
        public int ImageId { get; set; }

        public static explicit operator ImageRelationViewModel(TripImage tripImage)
        {
            return new ImageRelationViewModel
            {
                Id = tripImage.Id,
                RelationId = tripImage.TripId,
                ImageId = tripImage.ImageId,
            };
        }
    }
}
