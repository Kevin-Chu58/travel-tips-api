using System.Net.Http;
using System.Text.Json;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.nominatim;
using static TravelTipsAPI.Constants.TypeEnums;
using static TravelTipsAPI.Services.NominatimServices.NominatimSchema;

namespace TravelTipsAPI.Services.NominatimServices
{
    public class NominatimService(IHttpClientFactory httpClientFactory, IConfiguration config)
        : INominatimService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _nominatimBaseUrl =
            config["Nominatim:Domain"]
            ?? throw new ArgumentException("Nominatim:Domain not configured");

        // Cached and reused serializer options
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Get a list of osm entities by search
        /// </summary>
        /// <param name="search">search name</param>
        /// <returns>a list of osm entities</returns>
        public async Task<IEnumerable<OsmEntity>> GetOsmEntitiesByNameAsync(string search)
        {
            var encoded = Uri.EscapeDataString(search);
            var requestUrl = $"{_nominatimBaseUrl}/search?q={encoded}&format=json&extratags=1";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.UserAgent.ParseAdd("TravelTips/0.1");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var entities = await JsonSerializer.DeserializeAsync<List<OsmEntity>>(
                stream,
                _jsonOptions
            );

            entities = entities
                ?.GroupBy(x => (x.Osm_id, x.Osm_type))
                .Select(g => g.First())
                .ToList();

            return entities ?? [];
        }

        public async Task<OsmEntity> GetOsmEntitiesByOsmIdTypeAsync(long osmId, string osmType)
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TravelTips/1.0");

            string url = $"{_nominatimBaseUrl}/lookup?osm_ids={osmType}{osmId}&format=json";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var entities = await JsonSerializer.DeserializeAsync<List<OsmEntity>>(
                stream,
                _jsonOptions
            );

            if (entities is null)
            {
                throw new Exception(Messages.OsmEntityNotFound);
            }

            return entities[0];
        }
    }
}
