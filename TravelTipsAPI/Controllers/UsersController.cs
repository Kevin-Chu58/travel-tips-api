using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController(IUsersService usersService) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("me")]
        public async Task<ActionResult<UserViewModel>> GetCurrentUserAsync()
        {
            try
            {
                // Get Auth0 UserId
                string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return NotFound();

                UserViewModel userViewModel = await usersService.GetUserByUserId(userId);
                return Ok(userViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message, Stacktrace = ex.StackTrace });
            }
        }
    }
}
