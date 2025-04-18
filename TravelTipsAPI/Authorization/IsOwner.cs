using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Security.Claims;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

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
        private IPreferRoutesService _preferRoutesService;
        private ITripAttractionOrdersService _tripAttractionOrdersService;

        private int ResourceId { get; set; }
        private int UserId { get; set; }

        public override async void OnActionExecuting(ActionExecutingContext actionContext)
        {
            context = actionContext;

            _usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
            _tripsService = context.HttpContext.RequestServices.GetRequiredService<ITripsService>();
            _daysService = context.HttpContext.RequestServices.GetRequiredService<IDaysService>();
            _linksService = context.HttpContext.RequestServices.GetRequiredService<ILinksService>();
            _attractionsService = context.HttpContext.RequestServices.GetRequiredService<IAttractionsService>();
            _preferRoutesService = context.HttpContext.RequestServices.GetRequiredService<IPreferRoutesService>();
            _tripAttractionOrdersService = context.HttpContext.RequestServices.GetRequiredService<ITripAttractionOrdersService>();

            var auth0Id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            UserId = (await _usersService.GetUserByUserId(auth0Id))?.Id ?? 0;

            if (UserId == 0)
            {
                throw new AuthenticationFailureException("Authentication has failed for this request.");
            }

            // caching for easy reuse
            context.HttpContext.Items.Add("user_id", UserId);

            if (Resource != Resources.NONE)
            {
                // the id of the resource, e.g. id of a Trip, id of a Day
                ResourceId = (int)(context.ActionArguments["id"] ?? 0);
                var isAuthorized = HasOwnership(Resource);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException("Authorization has been denied for this request.");
                }
            }            
        }

        private bool HasOwnership(string resource)
        {
            IEnumerable<int> yourTrips, yourDays, yourLinks, yourAttractions, yourPreferRoutes, yourTripAttractionOrders;
            switch (resource)
            {
                case Resources.TRIPS:
                    yourTrips = _tripsService.GetYourTripIds(UserId);
                    return yourTrips.Any(tripId => tripId == ResourceId);

                case Resources.DAYS:
                    yourDays = _daysService.GetYourDayIds(UserId);
                    return yourDays.Any(dayId => dayId == ResourceId);

                case Resources.LINKS:
                    yourLinks = _linksService.GetYourLinkIds(UserId);
                    return yourLinks.Any(linkId => linkId == ResourceId);

                case Resources.ATTRACTIONS:
                    yourAttractions = _attractionsService.GetYourAttractions(UserId);
                    return yourAttractions.Any(aId => aId == ResourceId);

                case Resources.PREFER_ROUTES:
                    yourPreferRoutes = _preferRoutesService.GetYourPreferRoutes(UserId);
                    return yourPreferRoutes.Any(prId => prId == ResourceId);

                case Resources.TRIP_ATTRACTION_ORDERS:
                    yourTripAttractionOrders = _tripAttractionOrdersService.GetYourTripAttractionOrders(UserId);
                    return yourTripAttractionOrders.Any(taoId => taoId == ResourceId);
            }
            return false;
        }
    }
}
