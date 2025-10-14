using System.Text.Json;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using static TravelTipsAPI.Services.WikiCommonsServices.WikiCommonsSchema;

namespace TravelTipsAPI.Services.WikiCommonsServices
{
    public class WikiCommonsService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        UpstashHttpClient cache
    ) : IWikiCommonsService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _baseUrl =
            config["WikiCommons:Domain"]
            ?? throw new ArgumentException("WikiCommons:Domain not configured");

        /// <summary>
        /// Find a list of images on Wiki Commons API
        /// </summary>
        /// <param name="search">search string</param>
        /// <returns>a list of images from Wiki Commons API</returns>
        public async Task<IEnumerable<WikiImage>> SearchImagesByTitleAsync(string search)
        {
            var result = new List<WikiImage>();
            var key = $"WikiCommons:{search}:v{CacheVersion.Wiki_Commons_Version}";

            // check cache first, if does not exist, send request to HereMap API
            var cacheJson = await cache.GetAsync(key);
            if (cacheJson != null)
            {
                result = JsonSerializer.Deserialize<List<WikiImage>>(cacheJson);
            }
            else
            {
                var requestUrl =
                    $"{_baseUrl}?action=query&generator=search&gsrnamespace=6&prop=imageinfo&iiprop=url|mime|extmetadata&format=json&gsrsearch={search} filetype:image license:PD";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("query", out var queryElement))
                {
                    await cache.SetWithExpiryAsync(key, "[]", Time.WEEK_2);
                    throw new Exception(Messages.WikiCommonsQueryNotFound);
                }

                if (!queryElement.TryGetProperty("pages", out var pagesElement))
                {
                    await cache.SetWithExpiryAsync(key, "[]", Time.WEEK_2);
                    throw new Exception(Messages.WikiCommonsPagesNotFound);
                }

                // Iterate over each page (key is the pageid)
                foreach (var page in pagesElement.EnumerateObject())
                {
                    var pageObj = page.Value;

                    // Skip pages without imageinfo
                    if (
                        !pageObj.TryGetProperty("imageinfo", out var infoArr)
                        || infoArr.GetArrayLength() == 0
                    )
                        continue;

                    var info = infoArr[0];
                    if (!info.TryGetProperty("extmetadata", out var meta))
                        continue;

                    var isUsOnly = GetMeta("UsageTerms")?.Contains("United States") ?? false;
                    if (isUsOnly)
                        continue;

                    string GetMeta(string key) =>
                        meta.TryGetProperty(key, out var val)
                        && val.TryGetProperty("value", out var valueEl)
                            ? valueEl.GetString() ?? ""
                            : "";

                    var title = pageObj.GetProperty("title").GetString() ?? "";
                    var fileName = title.StartsWith("File:", StringComparison.OrdinalIgnoreCase)
                        ? title.Substring(5)
                        : title;

                    var imageUrl = info.TryGetProperty("url", out var urlEl)
                        ? urlEl.GetString() ?? ""
                        : "";

                    // Replace all URLs with JPEG thumbnail version with width 480
                    imageUrl =
                        $"https://commons.wikimedia.org/wiki/Special:FilePath/{Uri.EscapeDataString(fileName)}?width=480";

                    // Optional: filter out non-image mimetypes just in case
                    var mimeType = info.TryGetProperty("mime", out var mimeEl)
                        ? mimeEl.GetString() ?? ""
                        : "";
                    if (!mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        continue;

                    result.Add(
                        new WikiImage
                        {
                            Title = title,
                            Url = imageUrl,
                            Author = GetMeta("Artist"),
                            License = GetMeta("LicenseShortName"),
                        }
                    );
                }

                string jsonString = JsonSerializer.Serialize(result);
                await cache.SetWithExpiryAsync(key, jsonString, Time.WEEK_2);
            }

            return result;
        }
    }

    public class WikiImage
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Author { get; set; } = "";
        public string License { get; set; } = "";
        //public string LicenseUrl { get; set; } = "";
    }
}
