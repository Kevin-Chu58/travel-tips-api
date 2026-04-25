using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TravelTipsAPI.Constants;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.GospelSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Authorization
{
    /// <summary>
    /// This ActionFilter annotation serves the simple purpose of verifying the ownership of the resource
    /// the request intends to update, it does not verify the ownership of its parent resource or of any
    /// foreign key relation
    /// </summary>
    public class IsOwner : ActionFilterAttribute
    {
        public required string Resource { get; set; }
        public bool VerifyEmail { get; set; } = true;

        private ActionExecutingContext context;
        private IUsersService _usersService;
        private ITripsService _tripsService;
        private IDaysService _daysService;
        private IAttractionsService _attractionsService;
        private IHighlightsService _highlightsService;
        private ITripAttractionOrdersService _tripAttractionOrdersService;
        private IImagesService _imagesService;
        private IWritingsService _writingsService;
        private IBusinessesService _businessesService;
        private IAdsService _adsService;

        private int ResourceId { get; set; }
        private int UserId { get; set; }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            context = actionContext;

            _usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
            _tripsService = context.HttpContext.RequestServices.GetRequiredService<ITripsService>();
            _daysService = context.HttpContext.RequestServices.GetRequiredService<IDaysService>();
            _attractionsService =
                context.HttpContext.RequestServices.GetRequiredService<IAttractionsService>();
            _highlightsService =
                context.HttpContext.RequestServices.GetRequiredService<IHighlightsService>();
            _tripAttractionOrdersService =
                context.HttpContext.RequestServices.GetRequiredService<ITripAttractionOrdersService>();
            _imagesService =
                context.HttpContext.RequestServices.GetRequiredService<IImagesService>();
            _writingsService =
                context.HttpContext.RequestServices.GetRequiredService<IWritingsService>();
            _businessesService =
                context.HttpContext.RequestServices.GetRequiredService<IBusinessesService>();
            _adsService = context.HttpContext.RequestServices.GetRequiredService<IAdsService>();

            UserId = (int)(context.HttpContext.Items["user_id"] ?? 0);

            if (UserId == 0)
            {
                context.Result = new ObjectResult(Messages.AuthenticationFailed)
                {
                    StatusCode = 401,
                };
                return;
            }

            if (VerifyEmail)
            {
                var emailVerified = (bool)(context.HttpContext.Items["email_verified"] ?? false);

                if (!emailVerified)
                    context.Result = new ObjectResult(Messages.EmailUnverified)
                    {
                        StatusCode = 401,
                    };
                return;
            }

            if (Resource != Resources.NONE)
            {
                // the id of the resource, e.g. id of a Trip, id of a Day
                ResourceId = (int)(context.ActionArguments["id"] ?? 0);
                var isAuthorized = HasOwnership(Resource);
                if (!isAuthorized)
                {
                    context.Result = new ObjectResult(Messages.AccessDenied) { StatusCode = 403 };
                    return;
                }
            }
        }

        private bool HasOwnership(string resource)
        {
            switch (resource)
            {
                case Resources.TRIPS:
                    var myTrips = _tripsService.GetMyTripIds(UserId);
                    return myTrips.Any(tripId => tripId == ResourceId);

                case Resources.DAYS:
                    var myDays = _daysService.GetMyDayIds(UserId);
                    return myDays.Any(dayId => dayId == ResourceId);

                case Resources.ATTRACTIONS:
                    var myAttractions = _attractionsService.GetMyHighlights(UserId);
                    return myAttractions.Any(aId => aId == ResourceId);

                case Resources.HIGHLIGHTS:
                    var myHighlights = _highlightsService.GetMyHighlights(UserId);
                    return myHighlights.Any(aId => aId == ResourceId);

                case Resources.TRIP_ATTRACTION_ORDERS:
                    var myTripAttractionOrders = _tripAttractionOrdersService.GetMyTaos(UserId);
                    return myTripAttractionOrders.Any(taoId => taoId == ResourceId);

                case Resources.IMAGES:
                    return _imagesService.IsOwner(UserId, ResourceId);

                case Resources.WRITINGS:
                    var myWritings = _writingsService.GetMyWritings(UserId);
                    return myWritings.Any(writingId => writingId == ResourceId);

                case Resources.BUSINESSES:
                    var myBusinesses = _businessesService.GetMyBusinesses(UserId);
                    return myBusinesses.Any(businessId => businessId == ResourceId);

                case Resources.ADS:
                    var myAds = _adsService.GetMyAds(UserId);
                    return myAds.Any(adId => adId == ResourceId);

                default:
                    return false;
            }
        }
    }
}
