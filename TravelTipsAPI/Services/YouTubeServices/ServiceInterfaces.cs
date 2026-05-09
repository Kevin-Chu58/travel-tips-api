using TravelTipsAPI.ViewModels.YouTube;

namespace TravelTipsAPI.Services.WikiCommonsServices
{
    public class YouTubeSchema
    {
        public interface IYouTubeService
        {
            Task<IEnumerable<YouTubeChannel>> GetYouTubeChannelsFeed();
            Task<IEnumerable<IEnumerable<YouTubeVideo>>> GetYouTubeVideosFeed(
                IEnumerable<YouTubeChannel> channels
            );
        }
    }
}
