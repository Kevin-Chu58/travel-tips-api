namespace TravelTipsAPI.Services.WikiCommonsServices
{
    public class WikiCommonsSchema
    {
        public interface IWikiCommonsService
        {
            Task<IEnumerable<WikiImage>> SearchImagesByTitleAsync(string search);
        }
    }
}
