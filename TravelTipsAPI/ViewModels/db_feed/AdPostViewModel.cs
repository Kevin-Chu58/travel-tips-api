using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdPostViewModel
    {
        [MinLength(1)]
        [MaxLength(50)]
        public required string Title { get; set; }

        [MaxLength(100)]
        public string? Text { get; set; }

        [MaxLength(50)]
        public string? ButtonLabel { get; set; }

        [MaxLength(255)]
        public string? Link { get; set; }
        public int TemplateId { get; set; }
        public required IFormFile ImageFile { get; set; }
        public int? ImageId { get; set; }
    }
}
