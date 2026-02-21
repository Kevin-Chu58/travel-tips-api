using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TravelTipsAPI.Constants;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
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
        private ISermonsService _sermonsService;

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
            _sermonsService =
                context.HttpContext.RequestServices.GetRequiredService<ISermonsService>();

            var auth0Id =
                context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            if (auth0Id is null)
            {
                context.Result = new ObjectResult(Messages.AuthenticationFailed)
                {
                    StatusCode = 401,
                };
                return;
            }

            if (VerifyEmail)
            {
                var emailVerified =
                    context.HttpContext.User.FindFirst("email_verified")?.Value == "true";

                if (!emailVerified)
                    context.Result = new ObjectResult(Messages.EmailUnverified)
                    {
                        StatusCode = 401,
                    };
                return;
            }

            UserId = (_usersService.GetUserByUserId(auth0Id))?.Id ?? 0;

            if (UserId == 0)
            {
                context.Result = new ObjectResult(Messages.AuthenticationFailed)
                {
                    StatusCode = 401,
                };
                return;
            }

            // caching for easy reuse, nothing happen if already exist
            context.HttpContext.Items.TryAdd("user_id", UserId);

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
            IEnumerable<int> myTrips,
                myDays,
                myAttractions,
                myHighlights,
                myTripAttractionOrders,
                mySermons;

            switch (resource)
            {
                case Resources.TRIPS:
                    myTrips = _tripsService.GetMyTripIds(UserId);
                    return myTrips.Any(tripId => tripId == ResourceId);

                case Resources.DAYS:
                    myDays = _daysService.GetMyDayIds(UserId);
                    return myDays.Any(dayId => dayId == ResourceId);

                case Resources.ATTRACTIONS:
                    myAttractions = _attractionsService.GetMyHighlights(UserId);
                    return myAttractions.Any(aId => aId == ResourceId);

                case Resources.HIGHLIGHTS:
                    myHighlights = _highlightsService.GetMyHighlights(UserId);
                    return myHighlights.Any(aId => aId == ResourceId);

                case Resources.TRIP_ATTRACTION_ORDERS:
                    myTripAttractionOrders = _tripAttractionOrdersService.GetMyTaos(UserId);
                    return myTripAttractionOrders.Any(taoId => taoId == ResourceId);

                case Resources.IMAGES:
                    return _imagesService.IsOwner(UserId, ResourceId);

                case Resources.SERMONS:
                    mySermons = _sermonsService.GetMySermons(UserId);
                    return mySermons.Any(sermonId => sermonId == ResourceId);
            }
            return false;
        }
    }
}
