using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;
using static TravelTipsAPI.Services.Auth0Services.Auth0Schema;

namespace TravelTipsAPI.Services.Auth0Services
{
    public class Auth0Service(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration config
    ) : IAuth0Service
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _auth0Domain =
            config["Auth0:Domain"] ?? throw new ArgumentException("Auth0:Domain not configured");

        public async Task<Auth0UserInfo?> GetUserInfoAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return null;

            var accessToken = await context.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
                return null;

            var userInfoUrl = $"https://{_auth0Domain}/userinfo";

            var request = new HttpRequestMessage(HttpMethod.Get, userInfoUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return new Auth0UserInfo
            {
                Sub = json.Value<string>("sub"),
                Name = json.Value<string>("name"),
                Email = json.Value<string>("email"),
                Picture = json.Value<string>("picture"),
            };
        }
    }

    public class Auth0UserInfo
    {
        public string? Sub { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Picture { get; set; }
    }
}
