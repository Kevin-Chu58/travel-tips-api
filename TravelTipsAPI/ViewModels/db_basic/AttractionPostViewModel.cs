using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public int OsmId { get; set; }
        public int? LinkId { get; set; }

        public Attraction ToAttraction(int createdBy)
        {
            var attraction = new Attraction
            {
                Id = new int(),
                Name = Name,
                Description = Description,
                Address = Address,
                OsmId = OsmId,
                LinkId = LinkId,
                CreatedBy = createdBy
            };

            return attraction;
        }
    }
}
