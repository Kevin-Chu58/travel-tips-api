using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class LinkViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public int CreatedBy { get; set; }

        public static explicit operator LinkViewModel(Link link)
        {
            var linkViewModel = new LinkViewModel
            {
                Id = link.Id,
                Name = link.Name.Trim(),
                Url = link.Url.Trim(),
                CreatedBy = link.CreatedBy,
            };

            return linkViewModel;
        }
    }
}
