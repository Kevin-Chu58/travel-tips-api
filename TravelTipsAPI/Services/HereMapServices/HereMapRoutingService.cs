using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_image;
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

        public async Task<HereRoutingResponse?> GetRouteAsync(HereRoutingInput routeInput)
        {
            var key = GetUpstashKey(routeInput);

            var cacheJson = await cache.GetAsync(key);

            if (cacheJson != null)
            {
                return JsonSerializer.Deserialize<HereRoutingResponse>(cacheJson);
            }
            else
            {
                var hereRoutingResponse = await GetNewRouteAsync(routeInput, key);

                return hereRoutingResponse;
            }
        }

        public async Task<IEnumerable<HereRoutingResponse?>> GetRoutesAsync(
            List<HereRoutingInput> routeInputs
        )
        {
            if (routeInputs.Count == 0)
                return [];

            var keys = routeInputs.Select(input => GetUpstashKey(input)).ToArray();

            var cachesJson = await cache.GetMultipleAsync(keys);
            List<HereRoutingResponse?> hereRoutingResponses = [];

            for (var i = 0; i < cachesJson.Count; i++)
            {
                var cacheJson = cachesJson[i];

                if (cacheJson != null)
                {
                    hereRoutingResponses.Add(
                        JsonSerializer.Deserialize<HereRoutingResponse>(cacheJson)
                    );
                }
                else
                {
                    var routeInput = routeInputs[i];
                    var hereRoutingResponse = await GetNewRouteAsync(routeInput, keys[i]);

                    hereRoutingResponses.Add(hereRoutingResponse);
                }
            }

            return hereRoutingResponses;
        }

        private async Task<HereRoutingResponse?> GetNewRouteAsync(
            HereRoutingInput routeInput,
            string key
        )
        {
            HereMapEnum.ModeMap.TryGetValue(routeInput.TransportMode, out var mode);
            HereRoutingResponse? hereRoutingResponse;

            // public transit
            if (mode == HereMapEnum.RouteMode.PublicTransit)
            {
                hereRoutingResponse = await GetPublicTransitingAsync(
                    routeInput.OriginLat,
                    routeInput.OriginLng,
                    routeInput.DestinationLat,
                    routeInput.DestinationLng
                );
            }
            // non-public transit: e.g. car, pedestrian, ...
            else
            {
                hereRoutingResponse = await GetRoutingAsync(
                    routeInput.TransportMode,
                    routeInput.OriginLat,
                    routeInput.OriginLng,
                    routeInput.DestinationLat,
                    routeInput.DestinationLng
                );
            }

            // cache if exists
            try
            {
                string jsonString = JsonSerializer.Serialize(hereRoutingResponse);
                await cache.SetWithExpiryAsync(key, jsonString, Time.WEEK_1);
            }
            catch (Exception)
            {
                // store empty string to upstash
                await cache.SetWithExpiryAsync(key, "", Time.WEEK_1);
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
                    $"{_baseUrl}/v8/routes?apiKey={_apiKey}&transportMode={transportMode}&origin={originLat},{originLng}&destination={destinationLat},{destinationLng}&return=polyline,actions,travelSummary";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode(); // TODO - change this to null if it's unsuccessful

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
                    $"{_transitBaseUrl}/v8/routes?apiKey={_apiKey}&origin={originLat},{originLng}&destination={destinationLat},{destinationLng}&return=polyline,actions,intermediate,travelSummary&alternatives=0";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.UserAgent.ParseAdd(Global.USER_AGENT);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode(); // TODO - change this to null if it's unsuccessful

                using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<HereRoutingResponse>(
                    stream,
                    _jsonOptions
                );

                if (result != null)
                {
                    var preferredRoute = result.Routes?.FirstOrDefault(r =>
                        r.Sections != null && r.Sections.Any(s => s.Type == "transit")
                    );

                    // return the first route with transit if applicable
                    result.Routes = [preferredRoute ?? result.Routes.FirstOrDefault()];
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static string GetUpstashKey(HereRoutingInput input)
        {
            return $"mode:{input.TransportMode}:origin:{input.OriginLat},{input.OriginLng}:destinationLat:{input.DestinationLat},{input.DestinationLng}:v{CacheVersion.HereMap_Route_Version}";
        }
    }
}
