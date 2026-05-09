using static TravelTipsAPI.Services.WikiCommonsServices.YouTubeSchema;

namespace TravelTipsAPI.Services.YouTubeServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddYouTubeServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IYouTubeService, YouTubeService>();

            return services;
        }
    }
}
