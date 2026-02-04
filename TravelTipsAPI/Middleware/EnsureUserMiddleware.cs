using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.Auth0Services.Auth0Schema;

namespace TravelTipsAPI.Middleware
{
    public class EnsureUserMiddleware : IMiddleware
    {
        private readonly IAuth0Service _auth0Service;
        private readonly IDbContextFactory<TravelTipsContext> _dbFactory;

        public EnsureUserMiddleware(
            IAuth0Service auth0Service,
            IDbContextFactory<TravelTipsContext> dbFactory
        )
        {
            _auth0Service = auth0Service;
            _dbFactory = dbFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Skip CORS preflight requests
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await next(context);
                return;
            }

            var auth0UserInfo = await _auth0Service.GetUserInfoAsync();

            if (auth0UserInfo != null && !string.IsNullOrEmpty(auth0UserInfo.Sub))
            {
                using var db = _dbFactory.CreateDbContext();

                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == auth0UserInfo.Sub);
                if (user == null)
                {
                    var userPost = new User
                    {
                        UserId = auth0UserInfo.Sub,
                        Username = auth0UserInfo.Name ?? "",
                        Email = auth0UserInfo.Email ?? "",
                        ExternalImageUrl = auth0UserInfo.Picture ?? "",
                    };

                    db.Users.Add(userPost);
                    await db.SaveChangesAsync();
                }
            }

            await next(context);
        }
    }
}
