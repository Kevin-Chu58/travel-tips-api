using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BannerPatchViewModel
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(300)]
        public string? Overview { get; set; }
        public int? ImageId { get; set; }

        [MaxLength(100)]
        public string? Link { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }

        [MaxLength(100)]
        public string? Label { get; set; }

        [MaxLength(100)]
        public string? SubLabel { get; set; }
        public int? StylingId { get; set; }
    }
}
