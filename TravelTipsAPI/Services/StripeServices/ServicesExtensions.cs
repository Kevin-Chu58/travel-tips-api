using static TravelTipsAPI.Services.StripeServices.StripeSchema;

namespace TravelTipsAPI.Services.StripeServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddStripeServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IStripeService, StripeService>();
            services.AddScoped<IStripeWebhooksService, StripeWebhooksService>();
            services.AddScoped<IStripeWebhooksFulfillService, StripeWebhooksFulfillService>();

            return services;
        }
    }
}
