using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public int CreatedBy { get; set; }
        public int OsmId { get; set; }
        public int? LinkId { get; set; }

        public static explicit operator AttractionViewModel?(Attraction attraction)
        {
            if (attraction == null) return null;

            var attractionViewModel = new AttractionViewModel
            {
                Id = attraction.Id,
                Name = attraction.Name,
                Description = attraction.Description,
                Address = attraction.Address,
                CreatedBy = attraction.CreatedBy,
                OsmId = attraction.OsmId,
                LinkId = attraction.LinkId
            };

            return attractionViewModel;
        }
    }
}
