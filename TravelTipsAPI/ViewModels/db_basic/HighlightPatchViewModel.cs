using System.ComponentModel.DataAnnotations;
using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class HighlightPatchViewModel
    {
        public int Id { get; set; }

        [MinLength(1)]
        [MaxLength(500)]
        public required string Description { get; set; }
    }
}
