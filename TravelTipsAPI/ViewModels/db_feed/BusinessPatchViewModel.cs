using System.ComponentModel.DataAnnotations;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BusinessPatchViewModel
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Website { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }
    }
}
