using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BusinessViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Website { get; set; }
        public required string Address { get; set; }
        public required string Status { get; set; }

        public static explicit operator BusinessViewModel(Business business)
        {
            return new BusinessViewModel
            {
                Id = business.Id,
                Name = business.Name,
                Website = business.Website,
                Address = business.Address,
                Status = business.Status,
            };
        }
    }
}
