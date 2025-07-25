using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_image
{
    public class ImageViewModel
    {
        public int Id { get; set; }
        public required Guid Guid { get; set; }
        public string? Name { get; set; }
        public int CreatedBy { get; set; }

        public static explicit operator ImageViewModel(Image image)
        {
            return new ImageViewModel
            {
                Id = image.Id,
                Guid = image.Guid,
                Name = image.Name,
                CreatedBy = image.CreatedBy,
            };
        }
    }
}
