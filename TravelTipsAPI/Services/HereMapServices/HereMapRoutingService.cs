using System.Text.Json;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;

namespace TravelTipsAPI.Services.HereMapServices
{
    public class HereMapRoutingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        UpstashHttpClient cache
    ) : IHereMapRoutingService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _baseUrl =
            config["HereMap:Routing:Domain"]
            ?? throw new ArgumentException("HereMap:Routing:Domain not configured.");
        private readonly string _transitBaseUrl =
            config["HereMap:PublicTransit:Domain"]
            ?? throw new ArgumentException("HereMap:PublicTransit:Domain not configured.");
        private readonly string _apiKey =
            config["HereMap:ApiKey"]
            ?? throw new ArgumentException("HereMap:ApiKey not configured");

        // Cached and reused serializer options
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public async Task<HereRoutingResponse> GetRouteAsync(
            string transportMode,
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng
        )
        {
            var isModeValid = HereMapEnum.ModeMap.TryGetValue(transportMode, out var mode);

            if (!isModeValid)
                throw new Exception(Messages.HereMapTransportModeNotFound);

            HereRoutingResponse? hereRoutingResponse;

            var key =
                $"mode:{transportMode}:origin:{originLat},{originLng}:destinationLat:{destinationLat},{destinationLng}:v{CacheVersion.HereMap_Route_Version}";

            // check cache first, if does not exist, send request to HereMap API
            var cacheJson = await cache.GetAsync(key);
            if (cacheJson != null)
            {
                hereRoutingResponse = JsonSerializer.Deserialize<HereRoutingResponse>(cacheJson);
            }
            else
            {
                // public transit
                if (mode == HereMapEnum.RouteMode.PublicTransit)
                {
                    hereRoutingResponse = await GetPublicTransitingAsync(
                        originLat,
                        originLng,
                        destinationLat,
                        destinationLng
                    );
                }
                // non-public transit: e.g. car, pedestrian, ...
                else
                {
                    hereRoutingResponse = await GetRoutingAsync(
                        transportMode,
                        originLat,
                        originLng,
                        destinationLat,
                        destinationLng
                    );
                }

                // cache if exists
                string jsonString = JsonSerializer.Serialize(hereRoutingResponse);
                await cache.SetWithExpiryAsync(key, jsonString, Time.WEEK_1);
            }

            if (hereRoutingResponse is null)
            {
                throw new Exception(Messages.HereMapRouteNotFound);
            }

            return hereRoutingResponse;
        }

        /// <summary>
        /// Get here map Routing by transport mode and origin & destination geo coordinates
        /// </summary>
        /// <param name="transportMode">transport mode</param>
        /// <param name="originLat">origin lat</param>
        /// <param name="originLng">origin lng</param>
        /// <param name="destinationLat">destination lat</param>
        /// <param name="destinationLng">destination lng</param>
        /// <returns>a map routing object</returns>
        private async Task<HereRoutingResponse?> GetRoutingAsync(
            string transportMode,
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng
        )
        {
            try
            {
                var requestUrl =
                    $"{_baseUrl}/v8/routes?apiKey={_apiKey}&transportMode={transportMode}&origin={originLat},{originLng}&destination={destinationLat},{destinationLng}&return=polyline,travelSummary";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<HereRoutingResponse>(
                    stream,
                    _jsonOptions
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get here map Public Transit by transport mode and origin & destination geo coordinates
        /// </summary>
        /// <param name="originLat">origin lat</param>
        /// <param name="originLng">origin lng</param>
        /// <param name="destinationLat">destination lat</param>
        /// <param name="destinationLng">destination lng</param>
        /// <returns>a map routing object</returns>
        private async Task<HereRoutingResponse?> GetPublicTransitingAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng
        )
        {
            try
            {
                var requestUrl =
                    $"{_transitBaseUrl}/v8/routes?apiKey={_apiKey}&origin={originLat},{originLng}&destination={destinationLat},{destinationLng}&return=polyline,intermediate,travelSummary";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<HereRoutingResponse>(
                    stream,
                    _jsonOptions
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
