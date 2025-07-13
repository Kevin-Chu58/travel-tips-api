using System.Security.Claims;
using TravelTipsAPI.ViewModels.db_basic;
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
            var user = context.User;
            var auth0Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(auth0Id))
            {
                var _user = usersService.GetUserByUserId(auth0Id);
                if (_user == null)
                {
                    // Full name (optional, often present)
                    var fullName = user.FindFirst("name")?.Value ?? "";
                    if (fullName.Length > 20)
                        fullName = "";

                    // Email
                    var email = user.FindFirst("email")?.Value ?? "";
                    if (email.Length > 50)
                        email = "";

                    var userPost = new UserPostViewModel
                    {
                        UserId = auth0Id,
                        Username = fullName,
                        Email = email,
                    };

                    await usersService.PostNewUserAsync(userPost);
                }
            }

            await _next(context);
        }
    }
}
