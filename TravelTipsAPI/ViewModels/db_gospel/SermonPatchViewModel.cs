using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_gospel
{
    public class SermonPatchViewModel
    {
        [MinLength(1)]
        [MaxLength(50)]
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? LabelId { get; set; }
        public DateOnly? PublishAt { get; set; }
        public bool? IsBanner { get; set; }
    }
}
