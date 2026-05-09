using System.Text.Json;
using Google.Apis.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Services.WikiCommonsServices;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.YouTube;
using static TravelTipsAPI.Services.WikiCommonsServices.YouTubeSchema;

namespace TravelTipsAPI.Services.YouTubeServices
{
    public class YouTubeService(
        System.Net.Http.IHttpClientFactory httpClientFactory,
        IConfiguration config,
        UpstashHttpClient cache
    ) : IYouTubeService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string apiKey =
            config["YouTube:ApiKey"]
            ?? throw new ArgumentException("YouTube:ApiKey not configured");

        private readonly string[] channelIds =
        [
            "UCut8939DdQsJI3Gw1ziAc4w", // Ligonier Ministries
            "UCVfwlh9XpX2Y_tQfjeln9QA", // BibleProject
            "UCVrK_pMRp_q8IelpfUCTGLQ", // Turning Point USA
            "UCKYXHMpRr66QsKvJ6GXB8EA", // AI NEWS
            "UCEMUHug71GiSrtrL2OPpW7A", // 摩西講經
            "UCiBpHOMzsUq8ITYQH5D4UWA", // Nicolas Bowling
            "UCiw0g1A8BS_Jo1gdwIzaO8Q", // Right Response
        ];

        /// <summary>
        /// Get YouTube Channels Feed on Gospel Section
        /// </summary>
        /// <returns>a list of YouTube channel info</returns>
        public async Task<IEnumerable<YouTubeChannel>> GetYouTubeChannelsFeed()
        {
            var result = new List<YouTubeChannel>();
            var key = $"Gospel:YouTubeChannels:v{CacheVersion.YouTube_Version}";

            // check cache first, if does not exist, send request to HereMap API
            var cacheJson = await cache.GetAsync(key);
            if (cacheJson != null)
            {
                result = JsonSerializer.Deserialize<List<YouTubeChannel>>(cacheJson);
            }
            else
            {
                var ids = string.Join(",", channelIds);
                var requestUrl =
                    $"https://www.googleapis.com/youtube/v3/channels?key={apiKey}&part=snippet,contentDetails&id={ids}";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                result = doc
                    .RootElement.GetProperty("items")
                    .EnumerateArray()
                    .Select(item => new YouTubeChannel
                    {
                        Id = item.GetProperty("id").GetString()!,
                        Title = item.GetProperty("snippet").GetProperty("title").GetString()!,
                        ThumbnailUrl = item.GetProperty("snippet")
                            .GetProperty("thumbnails")
                            .GetProperty("high")
                            .GetProperty("url")
                            .GetString()!,
                        UploadsPlaylistId = item.GetProperty("contentDetails")
                            .GetProperty("relatedPlaylists")
                            .GetProperty("uploads")
                            .GetString()!,
                        CustomUrl = item.GetProperty("snippet")
                            .GetProperty("customUrl")
                            .GetString()!,
                    })
                    .ToList();

                // sort to match your defined channelIds order
                result = result.OrderBy(c => Array.IndexOf(channelIds, c.Id)).ToList();

                string jsonString = JsonSerializer.Serialize(result);
                await cache.SetWithExpiryAsync(key, jsonString, Time.WEEK_2);
            }
            return result;
        }

        /// <summary>
        /// Get YouTube Videos Feed of each channel in Gospel Section,
        /// return a list of list of videos, each inner list is the videos of a channel
        /// </summary>
        /// <returns>a list of list of videos</returns>
        public async Task<IEnumerable<IEnumerable<YouTubeVideo>>> GetYouTubeVideosFeed(
            IEnumerable<YouTubeChannel> channels
        )
        {
            var channelList = channels.ToList();
            var keys = channelList.Select(c => GetYouTubeChannelVideosUpstashKey(c.Id)).ToArray();
            var cachesJson = await cache.GetMultipleAsync(keys);

            // Separate cached hits from misses
            var tasks = channelList.Select(
                (channel, i) =>
                {
                    var cacheJson = cachesJson[i];
                    if (cacheJson != null)
                    {
                        var cached = JsonSerializer.Deserialize<IEnumerable<YouTubeVideo>>(
                            cacheJson
                        )!;
                        return Task.FromResult(cached);
                    }

                    return FetchAndCacheChannelVideosAsync(channel, keys[i]);
                }
            );

            var results = await Task.WhenAll(tasks);
            return results;
        }

        private async Task<IEnumerable<YouTubeVideo>> FetchAndCacheChannelVideosAsync(
            YouTubeChannel channel,
            string cacheKey
        )
        {
            try
            {
                var requestUrl =
                    $"https://www.googleapis.com/youtube/v3/playlistItems?key={apiKey}&part=snippet&playlistId={channel.UploadsPlaylistId}&maxResults=5";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var videos = doc
                    .RootElement.GetProperty("items")
                    .EnumerateArray()
                    .Select(item =>
                    {
                        var snippet = item.GetProperty("snippet");
                        var thumbnails = snippet.GetProperty("thumbnails");

                        return new YouTubeVideo
                        {
                            VideoId = snippet
                                .GetProperty("resourceId")
                                .GetProperty("videoId")
                                .GetString()!,
                            ChannelId = channel.Id,
                            Title = snippet.GetProperty("title").GetString()!,
                            ThumbnailUrl =
                                thumbnails.TryGetProperty("high", out var high)
                                    ? high.GetProperty("url").GetString()!
                                : thumbnails.TryGetProperty("medium", out var medium)
                                    ? medium.GetProperty("url").GetString()!
                                : thumbnails.TryGetProperty("default", out var def)
                                    ? def.GetProperty("url").GetString()!
                                : string.Empty,
                            PublishedAt = snippet.GetProperty("publishedAt").GetDateTime(),
                        };
                    })
                    .ToList();

                var jsonString = JsonSerializer.Serialize(videos);
                await cache.SetWithExpiryAsync(cacheKey, jsonString, Time.HOUR_6);

                return videos;
            }
            catch (Exception ex)
            {
                // log the error and return empty list so one bad channel doesn't break the whole feed
                Console.Error.WriteLine(
                    $"[YouTubeService] Failed to fetch videos for channel {channel.Id}: {ex.Message}"
                );
                return [];
            }
        }

        private static string GetYouTubeChannelVideosUpstashKey(string id)
        {
            return $"Gospel:YouTubeChannel:{id}:v{CacheVersion.YouTube_Version}";
        }
    }
}
