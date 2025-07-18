using TravelTipsAPI.Services.HereMapServices;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;

namespace TravelTipsAPI.HereMapServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddHereMapServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IHereMapDiscoverService, HereMapDiscoverService>();
            services.AddScoped<IHereMapLookupService, HereMapLookupService>();

            return services;
        }
    }
}
