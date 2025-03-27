using TravelTipsAPI.Controllers;

namespace TravelTipsAPI.Services
{
    public static class ServicesExtensions
    {

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITripsService, TripsService>();
            services.AddScoped<IUsersService, UsersService>();
            return services;
        }
    }
}
