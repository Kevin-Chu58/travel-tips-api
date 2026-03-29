using Stripe;

namespace TravelTipsAPI.Services.StripeServices
{
    public class StripeSchema
    {
        public interface IStripeService
        {
            RequestOptions? GetRequestOptions();
            string GetApiKey();
        }
    }
}
