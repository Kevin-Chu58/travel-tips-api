using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TravelTipsAPI.Constants;
using static TravelTipsAPI.Services.BasicSchema;
using static TravelTipsAPI.Services.RoleSchema;

namespace TravelTipsAPI.Authorization
{
    public class UserHasRole : ActionFilterAttribute
    {
        public required string Role { get; set; }

        private ActionExecutingContext context;
        private IUsersService _usersService;
        private IUserRolesService _userRolesService;

        private int UserId { get; set; }

        public override async void OnActionExecuting(ActionExecutingContext actionContext)
        {
            context = actionContext;

            _usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
            _userRolesService = context.HttpContext.RequestServices.GetRequiredService<IUserRolesService>();

            var auth0Id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            UserId = (await _usersService.GetUserByUserId(auth0Id))?.Id ?? 0;

            var isAuthorized = HasRole(Role);
            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("Authorization has been denied for this request.");
            }
        }

        private bool HasRole(string role)
        {
            // arrange the roles from the highest to the lowest
            bool isAdmin;
            switch(role)
            {
                case UserRoles.ADMIN:
                    isAdmin = _userRolesService.IsAdmin(UserId);
                    return isAdmin;
                    
            }

            return false;
        }
    }
}
