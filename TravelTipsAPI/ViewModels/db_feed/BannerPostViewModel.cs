using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BannerPostViewModel
    {
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(300)]
        public required string Overview { get; set; }
        public int ImageId { get; set; }

        [MaxLength(100)]
        public required string Link { get; set; }
        public DateOnly From { get; set; }
        public DateOnly? To { get; set; }

        [MaxLength(100)]
        public string? Label { get; set; }

        [MaxLength(100)]
        public string? SubLabel { get; set; }
        public int? StylingId { get; set; }
    }
}
