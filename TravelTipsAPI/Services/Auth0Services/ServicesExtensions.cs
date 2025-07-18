using static TravelTipsAPI.Services.Auth0Services.Auth0Schema;

namespace TravelTipsAPI.Services.Auth0Services
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddAuth0Services(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IAuth0Service, Auth0Service>();

            return services;
        }
    }
}
