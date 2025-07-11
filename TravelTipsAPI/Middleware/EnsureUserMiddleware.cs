using System.Security.Claims;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Middleware
{
    public class EnsureUserMiddleware
    {
        private readonly RequestDelegate _next;

        public EnsureUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUsersService usersService)
        {
            var auth0Id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(auth0Id))
            {
                var user = usersService.GetUserByUserId(auth0Id);
                if (user == null)
                {
                    await usersService.PostNewUserAsync(auth0Id);
                }
            }

            await _next(context);
        }
    }
}
