using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Users
    /// </summary>
    /// <param name="usersService">users service</param>
    [Route("api/[controller]")]
    public class UsersController(IUsersService usersService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get your current user basic information
        /// </summary>
        /// <returns>user basic information of the current user</returns>
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
