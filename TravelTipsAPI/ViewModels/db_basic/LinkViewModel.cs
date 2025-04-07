using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class LinkViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public int CreatedBy { get; set; }

        public static explicit operator LinkViewModel?(Link? link)
        {
            if (link == null) return null;

            var linkViewModel = new LinkViewModel
            {
                Id = link.Id,
                Name = link.Name,
                Url = link.Url,
                CreatedBy = link.CreatedBy
            };

            return linkViewModel;
        }
    }
}
