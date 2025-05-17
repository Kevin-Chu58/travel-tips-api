using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public int? CreatedBy { get; set; }
        public long OsmId { get; set; }
        public int? LinkId { get; set; }

        public static explicit operator AttractionViewModel(Attraction attraction)
        {
            var attractionViewModel = new AttractionViewModel
            {
                Id = attraction.Id,
                Name = attraction.Name.Trim(),
                Description = attraction.Description?.Trim(),
                Address = attraction.Address.Trim(),
                CreatedBy = attraction.CreatedBy,
                OsmId = attraction.OsmId,
                LinkId = attraction.LinkId,
            };

            return attractionViewModel;
        }
    }
}
