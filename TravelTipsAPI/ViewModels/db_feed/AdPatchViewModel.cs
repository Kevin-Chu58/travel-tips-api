using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdPatchViewModel
    {
        [MaxLength(50)]
        public string? Title { get; set; }

        [MaxLength(100)]
        public string? Text { get; set; }

        [MaxLength(50)]
        public string? LinkLabel { get; set; }

        [MaxLength(255)]
        public string? Link { get; set; }
        public int? TemplateId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
