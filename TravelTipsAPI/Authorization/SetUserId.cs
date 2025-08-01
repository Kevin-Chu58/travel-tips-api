using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Authorization
{
    public class SetUserId : ActionFilterAttribute
    {
        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            var usersService =
                context.HttpContext.RequestServices.GetRequiredService<IUsersService>();

            var auth0Id =
                context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var userId = (usersService.GetUserByUserId(auth0Id))?.Id ?? 0;

            context.HttpContext.Items.TryAdd("user_id", userId);
        }
    }
}
