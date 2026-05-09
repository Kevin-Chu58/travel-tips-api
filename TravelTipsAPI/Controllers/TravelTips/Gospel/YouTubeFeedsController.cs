using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.ViewModels.YouTube;
using static TravelTipsAPI.Services.WikiCommonsServices.YouTubeSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Gospel
{
    [Route("api/[controller]")]
    public class YouTubeFeedsController(IYouTubeService youTubeService) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<YouTubeChannel>>> GetGospelYouTubeFeed()
        {
            var channels = (await youTubeService.GetYouTubeChannelsFeed()).ToList();
            var videosByChannel = (await youTubeService.GetYouTubeVideosFeed(channels)).ToList();

            foreach (var channel in channels)
                channel.Videos = videosByChannel
                    .SelectMany(v => v)
                    .Where(v => v.ChannelId == channel.Id)
                    .ToList();

            return Ok(channels);
        }
    }
}
