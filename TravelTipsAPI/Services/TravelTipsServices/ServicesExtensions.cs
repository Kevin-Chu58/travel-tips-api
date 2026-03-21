using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Services.TravelTipsServices.Feed;
using TravelTipsAPI.Services.TravelTipsServices.Gospel;
using TravelTipsAPI.Services.TravelTipsServices.Plan;
using TravelTipsAPI.Services.TravelTipsServices.Search;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.GospelSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;
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
            services.AddScoped<IBookmarksService, BookmarksService>();
            services.AddScoped<IFollowersService, FollowersService>();
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
            // gospel schema
            services.AddScoped<IWritingsService, WritingsService>();
            // feed schema
            services.AddScoped<IBannersService, BannersService>();
            // plan schema
            services.AddScoped<ISubscriptionsService, SubscriptionsService>();

            return services;
        }
    }
}
