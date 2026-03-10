using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_gospel
{
    public class WritingPostViewModel
    {
        [MinLength(1)]
        [MaxLength(50)]
        public required string Title { get; set; }
        public required string Content { get; set; }
        public int? LabelId { get; set; }
        public required DateOnly PublishAt { get; set; }
        public bool? IsBanner { get; set; }
    }
}
