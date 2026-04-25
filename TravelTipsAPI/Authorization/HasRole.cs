using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TravelTipsAPI.Constants;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;

namespace TravelTipsAPI.Authorization
{
    public class HasRole : ActionFilterAttribute
    {
        public required string Role { get; set; }
        public bool VerifyEmail { get; set; } = true;

        private ActionExecutingContext? context;
        private IUsersService? _usersService;
        private IUserRolesService? _userRolesService;

        private int UserId { get; set; }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            context = actionContext;

            _usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
            _userRolesService =
                context.HttpContext.RequestServices.GetRequiredService<IUserRolesService>();

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
                {
                    context.Result = new ObjectResult(Messages.EmailUnverified)
                    {
                        StatusCode = 401,
                    };
                    return;
                }
            }

            var isAuthorized = UserHasRole(Role);
            if (!isAuthorized)
            {
                if (Role == UserRoles.MEMBER)
                {
                    context.Result = new ObjectResult(Messages.MembershipRequired)
                    {
                        StatusCode = 403,
                    };
                }
                else
                {
                    context.Result = new ObjectResult(Messages.AccessDenied) { StatusCode = 403 };
                }
                return;
            }
        }

        private bool UserHasRole(string role)
        {
            // admin has all permissions except using max limit kind of services
            if (_userRolesService.IsAdmin(UserId))
                return true;

            return role switch
            {
                UserRoles.WRITER => _userRolesService.IsWriter(UserId),
                UserRoles.BANNER_MAN => _userRolesService.IsBannerMan(UserId),
                UserRoles.REVIEWER => _userRolesService.IsReviewer(UserId),
                // subscriptions
                UserRoles.MEMBER => _userRolesService.IsUserMember(UserId),
                _ => false,
            };
        }
    }
}
