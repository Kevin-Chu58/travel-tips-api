using System.Net.Http.Headers;
using System.Text.Json;

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
            var url = $"{_baseUrl}/GET/{key}";
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RedisResult>(json);
            return result?.Result;
        }

        public async Task SetAsync(string key, string value)
        {
            var url = $"{_baseUrl}/SET/{key}/{value}";
            var response = await _client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> SetWithExpiryAsync(string key, string value, int seconds)
        {
            var response = await _client.PostAsync(
                $"{_baseUrl}/SET/{key}/{value}?EX={seconds}",
                null
            );
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            var response = await _client.PostAsync($"{_baseUrl}/DEL/{key}", null);
            return response.IsSuccessStatusCode;
        }

        private class RedisResult
        {
            public string? Result { get; set; }
        }
    }
}
