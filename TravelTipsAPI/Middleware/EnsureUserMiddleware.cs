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
        private readonly ILogger<EnsureUserMiddleware> _logger;

        public EnsureUserMiddleware(
            IDbContextFactory<TravelTipsContext> dbFactory,
            ILogger<EnsureUserMiddleware> logger
        )
        {
            _dbFactory = dbFactory;
            _logger = logger;
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
                        var newUser = new User
                        {
                            UserId = auth0Id,
                            Username = name ?? "",
                            Email = email ?? "",
                            ExternalImageUrl = picture ?? "",
                            EmailVerified = emailVerified,
                            UserSubExtend = new UserSubExtend(),
                        };

                        db.Users.Add(newUser);

                        await db.SaveChangesAsync();

                        context.Items["user_id"] = user?.Id;
                        context.Items["email_verified"] = emailVerified;
                    }
                    else
                    {
                        // caching for easy reuse, nothing happen if already exists
                        context.Items.TryAdd("user_id", user.Id);
                        context.Items.TryAdd("email_verified", emailVerified);

                        var hasChange = false;

                        // Optional: keep user profile in sync
                        if (email != null && user.Email != email)
                        {
                            user.Email = email;
                            hasChange = true;
                        }

                        // Optional: keep user profile in sync
                        if (user.EmailVerified != emailVerified)
                        {
                            user.EmailVerified = emailVerified;
                            hasChange = true;
                        }

                        // Optional: append default imageUrl in sync
                        if (user.ExternalImageUrl is null)
                        {
                            user.ExternalImageUrl = picture;
                            hasChange = true;
                        }

                        if (hasChange)
                        {
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }

            await next(context);
        }
    }
}
