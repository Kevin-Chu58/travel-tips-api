using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public long OsmId { get; set; }
        public int? LinkId { get; set; }

        public Attraction ToAttraction(int? createdBy)
        {
            if (createdBy is null)
            {
                return new Attraction
                {
                    Id = new int(),
                    Name = Name.Trim(),
                    Description = null,
                    Address = Address.Trim(),
                    OsmId = OsmId,
                    LinkId = null,
                    CreatedBy = createdBy,
                };
            }

            var attraction = new Attraction
            {
                Id = new int(),
                Name = Name.Trim(),
                Description = Description?.Trim(),
                Address = Address.Trim(),
                OsmId = OsmId,
                LinkId = LinkId,
                CreatedBy = createdBy,
            };

            return attraction;
        }
    }
}
