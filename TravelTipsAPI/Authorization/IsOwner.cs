using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TravelTipsAPI.Constants;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

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

        private ActionExecutingContext context;
        private IUsersService _usersService;
        private ITripsService _tripsService;
        private IDaysService _daysService;
        private ILinksService _linksService;
        private IAttractionsService _attractionsService;
        private IHighlightsService _highlightsService;

        //private IPreferRoutesService _preferRoutesService;
        //private ITripAttractionOrdersService _tripAttractionOrdersService;

        private int ResourceId { get; set; }
        private int UserId { get; set; }

        public override async void OnActionExecuting(ActionExecutingContext actionContext)
        {
            context = actionContext;

            _usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
            _tripsService = context.HttpContext.RequestServices.GetRequiredService<ITripsService>();
            _daysService = context.HttpContext.RequestServices.GetRequiredService<IDaysService>();
            _linksService = context.HttpContext.RequestServices.GetRequiredService<ILinksService>();
            _attractionsService =
                context.HttpContext.RequestServices.GetRequiredService<IAttractionsService>();
            _highlightsService =
                context.HttpContext.RequestServices.GetRequiredService<IHighlightsService>();
            //_preferRoutesService =
            //    context.HttpContext.RequestServices.GetRequiredService<IPreferRoutesService>();
            //_tripAttractionOrdersService =
            //    context.HttpContext.RequestServices.GetRequiredService<ITripAttractionOrdersService>();

            var auth0Id =
                context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
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
                myLinks,
                myAttractions,
                myHighlights,
                myPreferRoutes,
                myTripAttractionOrders;

            switch (resource)
            {
                case Resources.TRIPS:
                    myTrips = _tripsService.GetMyTripIds(UserId);
                    return myTrips.Any(tripId => tripId == ResourceId);

                case Resources.DAYS:
                    myDays = _daysService.GetMyDayIds(UserId);
                    return myDays.Any(dayId => dayId == ResourceId);

                case Resources.LINKS:
                    myLinks = _linksService.GetMyLinkIds(UserId);
                    return myLinks.Any(linkId => linkId == ResourceId);

                case Resources.ATTRACTIONS:
                    myAttractions = _attractionsService.GetMyHighlights(UserId);
                    return myAttractions.Any(aId => aId == ResourceId);

                case Resources.HIGHLIGHTS:
                    myHighlights = _highlightsService.GetMyHighlights(UserId);
                    return myHighlights.Any(aId => aId == ResourceId);

                //case Resources.PREFER_ROUTES:
                //    myPreferRoutes = _preferRoutesService.GetMyPreferRoutes(UserId);
                //    return myPreferRoutes.Any(prId => prId == ResourceId);

                //case Resources.TRIP_ATTRACTION_ORDERS:
                //    myTripAttractionOrders = _tripAttractionOrdersService.GetMyTripAttractionOrders(
                //        UserId
                //    );
                //    return myTripAttractionOrders.Any(taoId => taoId == ResourceId);
            }
            return false;
        }
    }
}
