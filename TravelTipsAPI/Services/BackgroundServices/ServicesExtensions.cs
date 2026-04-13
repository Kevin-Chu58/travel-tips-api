using static TravelTipsAPI.Services.BackgroundServices.ServiceInterface;

namespace TravelTipsAPI.Services.BackgroundServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddBackgroundWorkerServices(
            this IServiceCollection services
        )
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Singleton because all services share the same queue instance
            services.AddSingleton<
                IStripeWebhookBackgroundTaskQueue,
                StripeWebhookBackgroundTaskQueue
            >();

            // HostedService runs in the background automatically
            services.AddHostedService<WebhookWorker>();

            return services;
        }
    }
}
