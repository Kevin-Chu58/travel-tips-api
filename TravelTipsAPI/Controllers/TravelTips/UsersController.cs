using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    /// <summary>
    /// The controller of Users
    /// </summary>
    /// <param name="usersService">users service</param>
    [Route("api/[controller]")]
    public class UsersController(
        IFollowersService followersService,
        IUsersService usersService,
        IUserRolesService userRolesService,
        IImagesService imagesService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get your current user basic information
        /// </summary>
        /// <returns>user basic information of the current user</returns>
        [HttpGet]
        [Route("me")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<UserViewModel>> GetCurrentUser()
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var user = usersService.GetUserById(userId);
                var userViewModel = (await usersService.GetUserViewModels([user])).First();

                userViewModel.IsAdmin = userRolesService.IsAdmin(userId);
                userViewModel.IsWriter = userRolesService.IsWriter(userId);

                return Ok(userViewModel);
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

        // user profile

        [HttpGet]
        [Route("{id}/profile")]
        [AllowAnonymous]
        [SetUserId]
        public async Task<ActionResult<UserProfileViewModel>> GetUserProfile(int id)
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var userProfileViewModel = await usersService.GetUserProfileViewModel(id);

                userProfileViewModel.IsFollowing = followersService.IsFollowing(id, userId);
                return Ok(userProfileViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // user picture

        [HttpPatch]
        [Route("me/picture/{imageId}")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<string>> UpdateUserPicture(int imageId)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // validate the user ownership on the image
            var ownership = imagesService.IsOwner(userId, imageId);

            if (!ownership)
                return Forbid(Messages.ImageUnauthorized);

            var user = usersService.GetUserById(userId);
            var image = (await imagesService.GetImagesByIds([imageId])).FirstOrDefault();

            try
            {
                var imageUrl = await usersService.UpdateUserPicture(user, image);
                return Ok(imageUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // user follower

        [HttpPost]
        [Route("{id}/follow")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult> FollowUser(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            try
            {
                await usersService.FollowAsync(id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id}/unfollow")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult> UnFollowUser(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            try
            {
                await usersService.UnfollowAsync(id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
