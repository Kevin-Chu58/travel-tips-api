using System.Text.Json;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Utils;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;

namespace TravelTipsAPI.Services.HereMapServices
{
    public class HereMapLookupService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        UpstashHttpClient cache
    ) : IHereMapLookupService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _baseUrl =
            config["HereMap:Lookup:Domain"]
            ?? throw new ArgumentException("HereMap:Lookup:Domain not configured");
        private readonly string _apiKey =
            config["HereMap:ApiKey"]
            ?? throw new ArgumentException("HereMaps:ApiKey not configured");

        // Cached and reused serializer options
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Find a place by hereId
        /// </summary>
        /// <param name="hereId">hereId</param>
        /// <returns>a place with the hereId</returns>
        public async Task<Attraction> LookupPlaceByIdAsync(string hereId)
        {
            HerePlace? result;

            // check cache first, if does not exist, send request to HereMap API
            var cacheJson = await cache.GetAsync(hereId);
            if (cacheJson != null)
            {
                result = JsonSerializer.Deserialize<HerePlace>(cacheJson);
            }
            else
            {
                var requestUrl = $"{_baseUrl}/v1/lookup?id={hereId}&apiKey={_apiKey}";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<HerePlace>(stream, _jsonOptions);
            }

            if (result is null)
            {
                throw new Exception(Messages.HereMapPlaceNotFound);
            }
            else
            {
                // cache if exists
                string jsonString = JsonSerializer.Serialize(result);
                await cache.SetWithExpiryAsync(hereId, jsonString, Time.WEEK_2);
            }

            return ModelUtils.ToAttraction(result);
        }
    }
}
