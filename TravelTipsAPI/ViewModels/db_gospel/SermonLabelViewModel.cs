using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_sermon
{
    public class SermonLabelViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public int? ParentLabelId { get; set; }
        public required string Type { get; set; }

        public static explicit operator SermonLabelViewModel(SermonLabel l)
        {
            return new SermonLabelViewModel
            {
                Id = l.Id,
                Name = l.Name,
                Slug = l.Slug,
                ParentLabelId = l.ParentLabelId,
                Type = l.Type,
            };
        }
    }
}
