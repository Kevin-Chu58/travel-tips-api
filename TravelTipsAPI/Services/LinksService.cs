using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Links
    /// </summary>
    /// <param name="basicContext">db_basic context</param>
    public class LinksService(TravelTipsBasicContext basicContext) : ILinksService
    {
        /// <summary>
        /// Get links by the name
        /// </summary>
        /// <param name="timeStamp">the time in milliseconds when the http request created</param>
        /// <param name="name">name to search</param>
        /// <returns>the search result of links</returns>
        public LinkSearchViewModel GetLinksByName(int timeStamp, string name, int createdBy)
        {
            var linkViewModels = basicContext.Links
                .Where(link => link.Name.Contains(name)
                    && link.CreatedBy == createdBy)
                .Select(link => (LinkViewModel)link)
                .ToList();

            var linkSearchViewModel = new LinkSearchViewModel
            {
                TimeStamp = timeStamp,
                Links = linkViewModels
            };

            return linkSearchViewModel;
        }

        /// <summary>
        /// Create a new link
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="linkPostViewModel">new link detail</param>
        /// <returns>the new link</returns>
        public async Task<LinkViewModel> PostNewLinkAsync(int createdBy, LinkPostViewModel linkPostViewModel)
        {
            var newLink = linkPostViewModel.ToLink(createdBy);

            await basicContext.Links.AddAsync(newLink);
            await basicContext.SaveChangesAsync();

            return (LinkViewModel)newLink;
        }

        /// <summary>
        /// Update a link with link detail
        /// </summary>
        /// <param name="id">link id</param>
        /// <param name="linkPatchViewModel">link detail to be updated</param>
        /// <returns>the link updated</returns>
        public async Task<LinkViewModel> PatchLinkAsync(int id, LinkPatchViewModel linkPatchViewModel)
        {
            var link = basicContext.Links.Find(id) ?? throw new Exception("Link not found.");

            link.Name = linkPatchViewModel.Name ?? link.Name;
            link.Url = linkPatchViewModel?.Url ?? link.Url;

            await basicContext.SaveChangesAsync();

            return (LinkViewModel)link;
        }

        public bool IsOwner(int id, int linkId)
        {
            var link = basicContext.Links.Find(linkId);

            return link?.CreatedBy == id;
        }
    }
}
