using Stripe;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;

namespace TravelTipsAPI.Services.StripeServices
{
    public class StripeService(IConfiguration config) : IStripeService
    {
        private readonly string _apiKey =
            config["Stripe:ApiKey"] ?? throw new ArgumentException("Stripe:ApiKey not configured");

        public RequestOptions? GetRequestOptions()
        {
            return new RequestOptions { ApiKey = _apiKey };
        }

        public string GetApiKey()
        {
            return _apiKey;
        }
    }
}
