using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TravelTipsAPI.Clients
{
    public class UpstashHttpClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _token;

        public UpstashHttpClient(string baseUrl, string token)
        {
            _baseUrl = baseUrl;
            _token = token;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _token
            );
        }

        public async Task<string?> GetAsync(string key)
        {
            var safeKey = Uri.EscapeDataString(key);

            var url = $"{_baseUrl}/GET/{safeKey}";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RedisResult>(json);

            // Result can be a string or null
            if (result == null || result.Result.ValueKind == JsonValueKind.Null)
                return null;

            return result.Result.GetString();
        }

        public async Task<List<string?>> GetMultipleAsync(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return [];

            // URL-encode each key
            var safeKeys = keys.Select(Uri.EscapeDataString);
            var url = $"{_baseUrl}/mget/{string.Join("/", safeKeys)}";

            var response = await _client.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();

            // Upstash returns something like:
            // [{"result":["val1",null,"val3"]}]
            var results = JsonSerializer.Deserialize<RedisResult>(json);

            // First command’s result is the array of values
            var values = results?.ResultArray;

            return values ?? [];
        }

        public async Task SetAsync(string key, string value)
        {
            var safeKey = Uri.EscapeDataString(key);
            var safeValue = Uri.EscapeDataString(value);

            var url = $"{_baseUrl}/SET/{safeKey}/{safeValue}";
            var response = await _client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> SetWithExpiryAsync(string key, string value, int seconds)
        {
            var safeKey = Uri.EscapeDataString(key);
            var safeValue = Uri.EscapeDataString(value);

            var response = await _client.PostAsync(
                $"{_baseUrl}/SET/{safeKey}/{safeValue}?EX={seconds}",
                null
            );

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            var safeKey = Uri.EscapeDataString(key);

            var response = await _client.PostAsync($"{_baseUrl}/DEL/{safeKey}", null);
            return response.IsSuccessStatusCode;
        }

        private class RedisResult
        {
            [JsonPropertyName("result")]
            public JsonElement Result { get; set; }

            // If the result is a single string (like from GET)
            public string? ResultString =>
                Result.ValueKind == JsonValueKind.String ? Result.GetString() : null;

            // If the result is an array (like from MGET)
            public List<string?>? ResultArray =>
                Result.ValueKind == JsonValueKind.Array
                    ? Result
                        .EnumerateArray()
                        .Select(e => e.ValueKind == JsonValueKind.Null ? null : e.GetString())
                        .ToList()
                    : null;
        }
    }
}
