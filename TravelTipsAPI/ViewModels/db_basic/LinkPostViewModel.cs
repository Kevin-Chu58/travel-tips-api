using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class LinkPostViewModel
    {
        public required string Name { get; set; }
        public required string Url { get; set; }

        public Link ToLink(int createdBy)
        {
            var link = new Link
            {
                Id = new int(),
                Name = Name,
                Url = Url,
                CreatedBy = createdBy
            };

            return link;
        }
    }
}
