using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using TravelTipsAPI.Constants;

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
            return result?.Result;
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
            public string? Result { get; set; }
        }
    }
}
