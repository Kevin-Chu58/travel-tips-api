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
                ClaimsPrincipal user = HttpContext.User;
                string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var hasUser = usersService.DoesCurrentUserExist(userId ?? "");

                UserViewModel userViewModel;
                if (userId == null) return NoContent();

                if (!hasUser)
                {
                    userViewModel = await usersService.PostNewUserAsync(userId);
                    //return CreatedAtAction(nameof(GetCurrentUserAsync), userViewModel);
                }
                else
                {
                    userViewModel = usersService.GetUserByUserId(userId);
                    //return Ok(userViewModel);
                }
                return Ok(userViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.StackTrace });
            }
        }
    }
}
