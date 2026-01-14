using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripPostViewModel
    {
        [MinLength(1)]
        [MaxLength(50)]
        public required string Title { get; set; }
    }
}
