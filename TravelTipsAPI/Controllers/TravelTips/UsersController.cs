using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Controllers.TravelTips
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
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<UserViewModel> GetCurrentUser()
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var user = usersService.GetUserById(userId);
                return Ok((UserViewModel)user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Accepts user agreement
        /// </summary>
        /// <returns>updated user agreement status</returns>
        [HttpPatch]
        [Route("me/user-agreement")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<bool>> AcceptUserAgreement()
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var userAgreementStatus = await usersService.AcceptUserAgreementAsync(userId);
                return Ok(userAgreementStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
