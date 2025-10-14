using static TravelTipsAPI.Services.WikiCommonsServices.WikiCommonsSchema;

namespace TravelTipsAPI.Services.WikiCommonsServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddWikiCommonsServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IWikiCommonsService, WikiCommonsService>();

            return services;
        }
    }
}
