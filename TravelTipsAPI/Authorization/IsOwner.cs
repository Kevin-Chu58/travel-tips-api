using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Security.Claims;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;

namespace TravelTipsAPI.Authorization
{
    public class IsOwner : ActionFilterAttribute
    {
        public required string Resource { get; set; }

        private ActionExecutingContext context;
        private IUsersService _usersService;
        private ITripsService _tripsService;

        private int ResourceId { get; set; }
        private int UserId { get; set; }

        public override async void OnActionExecuting(ActionExecutingContext actionContext)
        {
            context = actionContext;

            ResourceId = (int)context.ActionArguments["id"];

            _usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
            _tripsService = context.HttpContext.RequestServices.GetRequiredService<ITripsService>();

            var auth0Id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            UserId = (await _usersService.GetUserByUserId(auth0Id)).Id;

            if (UserId == null)
            {
                throw new AuthenticationFailureException("Authentication has failed for this request.");
            }

            var isAuthorized = HasOwnership(Resource);
            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("Authorization has been denied for this request.");
            }
        }

        public bool HasOwnership(string resource)
        {
            switch (resource)
            {
                case Resources.TRIPS:
                    var yourTrips = _tripsService.GetYourTripIds(UserId);
                    return yourTrips.Any(tripId => tripId == ResourceId);
                    // TODO - for other resources

            }
            return false;
        }
    }
}
