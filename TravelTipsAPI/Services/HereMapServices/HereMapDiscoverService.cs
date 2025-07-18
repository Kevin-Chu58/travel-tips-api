using System.Text.Json;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Utils;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;

namespace TravelTipsAPI.Services.HereMapServices
{
    public class HereMapDiscoverService(IHttpClientFactory httpClientFactory, IConfiguration config)
        : IHereMapDiscoverService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _baseUrl =
            config["HereMap:Discover:Domain"]
            ?? throw new ArgumentException("HereMap:Discover:Domain not configured");
        private readonly string _apiKey =
            config["HereMap:ApiKey"]
            ?? throw new ArgumentException("HereMaps:ApiKey not configured");

        // Cached and reused serializer optionsc
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Find a list of HerePlace by query name
        /// </summary>
        /// <param name="query">search name</param>
        /// <param name="lat">lat to search from</param>
        /// <param name="lng">lng to search from</param>
        /// <param name="limit">returned number of items</param>
        /// <returns>a list of HerePlace</returns>
        public async Task<IEnumerable<Attraction>> SearchPlaceByNameAsync(
            string query,
            decimal lat,
            decimal lng,
            int? limit
        )
        {
            var encoded = Uri.EscapeDataString(query);
            var actualLimit = limit ?? 20;
            var requestUrl =
                $"{_baseUrl}/v1/discover?q={encoded}&at={lat},{lng}&limit={actualLimit}&apiKey={_apiKey}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<HereDiscoverResponse>(
                stream,
                _jsonOptions
            );

            if (result is null || result.Items.Count == 0)
            {
                throw new Exception(Messages.HereMapPlaceNotFound);
            }

            var herePlaces = result
                .Items.Select(herePlace => ModelUtils.ToAttraction(herePlace))
                .ToList();
            return herePlaces;
        }
    }
}
