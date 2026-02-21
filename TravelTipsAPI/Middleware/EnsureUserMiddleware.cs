using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.Auth0Services.Auth0Schema;

namespace TravelTipsAPI.Middleware
{
    public class EnsureUserMiddleware : IMiddleware
    {
        private readonly IDbContextFactory<TravelTipsContext> _dbFactory;

        public EnsureUserMiddleware(IDbContextFactory<TravelTipsContext> dbFactory)
        {
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

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var auth0Id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

                var name = context.User.FindFirst("name")?.Value;

                var picture = context.User.FindFirst("picture")?.Value;

                var emailVerified = context.User.FindFirst("email_verified")?.Value == "true";

                if (!string.IsNullOrEmpty(auth0Id))
                {
                    using var db = _dbFactory.CreateDbContext();

                    var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == auth0Id);

                    if (user == null)
                    {
                        db.Users.Add(
                            new User
                            {
                                UserId = auth0Id,
                                Username = name ?? "",
                                Email = email ?? "",
                                ExternalImageUrl = picture ?? "",
                                EmailVerified = emailVerified,
                            }
                        );

                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        // Optional: keep user profile in sync
                        if (user.EmailVerified != emailVerified)
                        {
                            user.EmailVerified = emailVerified;
                            await db.SaveChangesAsync();
                        }

                        // Optional: append default imageUrl in sync
                        if (user.ExternalImageUrl is null)
                        {
                            user.ExternalImageUrl = picture;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }

            await next(context);
        }
    }
}
