using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_search
{
    public class RegionViewModel
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Slug { get; set; }

        public int? ParentRegionId { get; set; }

        public required string Type { get; set; }

        public static explicit operator RegionViewModel(Region r)
        {
            return new RegionViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Slug = r.Slug,
                ParentRegionId = r.ParentRegionId,
                Type = r.Type,
            };
        }
    }
}
