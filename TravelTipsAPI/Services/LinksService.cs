using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Links
    /// </summary>
    /// <param name="context">context</param>
    public class LinksService(TravelTipsContext context) : ILinksService
    {
        /// <summary>
        /// Get links by the name
        /// </summary>
        /// <param name="name">name to search</param>
        /// <param name="createdBy">user id</param>
        /// <returns>the search result of links</returns>
        public IEnumerable<LinkViewModel> GetLinksByName(string name, int createdBy)
        {
            name = name.Trim().ToLower();

            var linkViewModels = new List<LinkViewModel>();

            if (name.Length >= SearchConstants.LINK_SEARCH_MIN_LENGTH)
            {
                linkViewModels = context
                    .Links.Where(link =>
                        link.Name.ToLower().Contains(name) && link.CreatedBy == createdBy
                    )
                    .Select(link => (LinkViewModel)link)
                    .ToList();
            }

            return linkViewModels;
        }

        /// <summary>
        /// Get the links I own
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>the link ids I own</returns>
        public IEnumerable<int> GetMyLinkIds(int id)
        {
            var myLinkIds = context
                .Links.Where(link => link.CreatedBy == id)
                .Select(link => link.Id)
                .ToList();

            return myLinkIds;
        }

        /// <summary>
        /// Create a new link
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="linkPostViewModel">new link detail</param>
        /// <returns>the new link</returns>
        public async Task<LinkViewModel> PostNewLinkAsync(
            int createdBy,
            LinkPostViewModel linkPostViewModel
        )
        {
            var newLink = linkPostViewModel.ToLink(createdBy);

            await context.Links.AddAsync(newLink);
            await context.SaveChangesAsync();

            return (LinkViewModel)newLink;
        }

        /// <summary>
        /// Update a link with link detail
        /// </summary>
        /// <param name="id">link id</param>
        /// <param name="linkPatchViewModel">link detail to be updated</param>
        /// <returns>the link updated</returns>
        public async Task<LinkViewModel> PatchLinkAsync(
            int id,
            LinkPatchViewModel linkPatchViewModel
        )
        {
            var link = context.Links.Find(id) ?? throw new Exception("Link not found.");

            link.Name = linkPatchViewModel.Name?.Trim() ?? link.Name;
            link.Url = linkPatchViewModel?.Url?.Trim() ?? link.Url;

            await context.SaveChangesAsync();

            return (LinkViewModel)link;
        }

        /// <summary>
        /// Check if new link's detail is valid
        /// </summary>
        /// <param name="newLink">new link</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePost(LinkPostViewModel newLink)
        {
            var invalidParams = new List<string>();

            if (newLink.Name.Length > 50)
                invalidParams.Add("name");
            if (newLink.Url.Length > 100)
                invalidParams.Add("url");

            return invalidParams;
        }

        /// <summary>
        /// Check if link's detail is valid
        /// </summary>
        /// <param name="link">existing link</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePatch(LinkPatchViewModel link)
        {
            var invalidParams = new List<string>();

            if (link.Name?.Length > 50)
                invalidParams.Add("name");
            if (link.Url?.Length > 100)
                invalidParams.Add("url");

            return invalidParams;
        }
    }
}
