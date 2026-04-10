using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BusinessPostViewModel
    {
        [MinLength(1)]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MinLength(1)]
        [MaxLength(100)]
        public required string Website { get; set; }

        [MinLength(1)]
        [MaxLength(200)]
        public required string Address { get; set; }
    }
}
