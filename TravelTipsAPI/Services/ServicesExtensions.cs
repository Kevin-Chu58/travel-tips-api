using TravelTipsAPI.Controllers;
using static TravelTipsAPI.Services.BasicSchema;
using static TravelTipsAPI.Services.RoleSchema;

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
            // basic schema
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ITripsService, TripsService>();
            services.AddScoped<ISmallTripsService, SmallTripsService>();
            services.AddScoped<IDaysService, DaysService>();
            services.AddScoped<ILinksService, LinksService>();
            services.AddScoped<IAttractionsService, AttractionsService>();
            // user role schema
            services.AddScoped<IUserRolesService, UserRolesService>();

            return services;
        }
    }
}
