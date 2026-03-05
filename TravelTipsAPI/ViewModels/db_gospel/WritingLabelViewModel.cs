using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_gospel
{
    public class WritingLabelViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public int? ParentLabelId { get; set; }
        public required string Type { get; set; }

        public static explicit operator WritingLabelViewModel(WritingLabel l)
        {
            return new WritingLabelViewModel
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
