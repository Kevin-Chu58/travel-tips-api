using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BusinessPostViewModel
    {
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(100)]
        public required string Website { get; set; }

        [MaxLength(200)]
        public required string Address { get; set; }
    }
}
