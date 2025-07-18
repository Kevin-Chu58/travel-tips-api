using static TravelTipsAPI.Services.NominatimServices.NominatimSchema;

namespace TravelTipsAPI.Services.NominatimServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddNominatimServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<INominatimService, NominatimService>();

            return services;
        }
    }
}
