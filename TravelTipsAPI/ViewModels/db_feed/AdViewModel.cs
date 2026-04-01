using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdViewModel
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int ImageId { get; set; }
        public string? Picture { get; set; }
        public string? SubStatus { get; set; }
        public required string Status { get; set; }

        public static explicit operator AdViewModel(Ad ad)
        {
            return new AdViewModel
            {
                Id = ad.Id,
                BusinessId = ad.BusinessId,
                ImageId = ad.ImageId,
                SubStatus = ad.SubStatus,
                Status = ad.Status,
            };
        }
    }
}
