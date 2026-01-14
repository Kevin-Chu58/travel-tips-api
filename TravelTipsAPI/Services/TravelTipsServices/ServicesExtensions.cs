using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Services.TravelTipsServices.Search;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // db context
            services.AddDbContext<TravelTipsContext>(ServiceLifetime.Transient);

            // search schema
            services.AddScoped<IRegionsService, RegionsService>();
            // basic schema
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ITripsService, TripsService>();
            services.AddScoped<ITripSharesService, TripSharesService>();
            services.AddScoped<IDaysService, DaysService>();
            services.AddScoped<IAttractionsService, AttractionsService>();
            services.AddScoped<IHighlightsService, HighlightsService>();
            services.AddScoped<ITripAttractionOrdersService, TripAttractionOrdersService>();
            // user role schema
            services.AddScoped<IUserRolesService, UserRolesService>();
            // image schema
            services.AddScoped<IImagesService, ImagesService>();

            return services;
        }
    }
}
