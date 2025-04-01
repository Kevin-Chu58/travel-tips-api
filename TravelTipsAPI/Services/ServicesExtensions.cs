using TravelTipsAPI.Controllers;

namespace TravelTipsAPI.Services
{
    public static class ServicesExtensions
    {

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000",
                    builder => builder.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                ); 
            });

            services.AddScoped<ITripsService, TripsService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ISmallTripsService, SmallTripsService>();
            return services;
        }
    }
}
