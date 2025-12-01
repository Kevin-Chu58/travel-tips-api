using static TravelTipsAPI.Services.UtilServices.UtilSchema;

namespace TravelTipsAPI.Services.UtilServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddUtilServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<ISpellCheckerService, SpellCheckerService>();

            return services;
        }
    }
}
