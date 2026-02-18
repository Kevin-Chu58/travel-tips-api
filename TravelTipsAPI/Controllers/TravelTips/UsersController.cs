using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

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
        /// Get user by userId
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user with the user id</returns>
        [HttpGet]
        [Route("{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserSimpleViewModel>> GetUserByUserId(string userId)
        {
            var user = usersService.GetUserByUserId(userId);

            if (user is null)
                return NotFound(Messages.UserNotFound);

            try
            {
                var userSimples = await usersService.GetUserSimpleViewModels([user]);
                return Ok(userSimples.First());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get a list of users by username with cursor
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="cursor">cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of users</returns>
        [HttpGet]
        [Route("username")]
        [AllowAnonymous]
        public async Task<ActionResult<SearchResults<UserViewModel>>> SearchUsersByUsername(
            [FromQuery] string username,
            string? cursor = null,
            int? limit = null
        )
        {
            // decode cursor if provided
            GeneralCursor? generalCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                generalCursor = DecodeCursor<GeneralCursor>(cursor);
                if (generalCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            try
            {
                var users = usersService.GetUsersByUsernameWithCursor(
                    out int? lastUserId,
                    username,
                    generalCursor,
                    limit ?? Global.USER_DEFAULT_LIMIT
                );

                var userViewModels = await usersService.GetUserSimpleViewModels(users);

                // encode cursor
                string? newCursor = null;
                if (lastUserId != null)
                {
                    newCursor = EncodeCursor(new GeneralCursor { Id = (int)lastUserId });
                }

                var result = new SearchResults<UserSimpleViewModel>
                {
                    Results = userViewModels,
                    Cursor = newCursor,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
        [Route("{auth0Id}/profile")]
        [AllowAnonymous]
        [SetUserId]
        public async Task<ActionResult<UserProfileViewModel>> GetUserProfile(string auth0Id)
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var userProfileViewModel = await usersService.GetUserProfileViewModel(auth0Id);

                userProfileViewModel.IsFollowing = followersService.IsFollowing(
                    userProfileViewModel.Id,
                    userId
                );
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

        [HttpGet]
        [Route("followers")]
        [AllowAnonymous]
        public async Task<ActionResult<SearchResults<UserSimpleViewModel>>> GetFollowers(
            [FromQuery] int userId,
            string? cursor = null,
            int? limit = null
        )
        {
            // decode cursor if provided
            GeneralCursor? generalCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                generalCursor = DecodeCursor<GeneralCursor>(cursor);
                if (generalCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            try
            {
                var followers = followersService.GetFollowingUsersByUserIdWithCursor(
                    out int? lastFollowerId,
                    userId,
                    generalCursor,
                    limit ?? Global.USER_DEFAULT_LIMIT
                );
                var followerViewModels = await usersService.GetUserSimpleViewModels(followers);

                // encode cursor
                string? newCursor = null;
                if (lastFollowerId != null)
                {
                    newCursor = EncodeCursor(new GeneralCursor { Id = (int)lastFollowerId });
                }

                var result = new SearchResults<UserSimpleViewModel>
                {
                    Results = followerViewModels,
                    Cursor = newCursor,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("followings")]
        [AllowAnonymous]
        public async Task<ActionResult<SearchResults<UserSimpleViewModel>>> GetFollowings(
            [FromQuery] int userId,
            string? cursor = null,
            int? limit = null
        )
        {
            // decode cursor if provided
            GeneralCursor? generalCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                generalCursor = DecodeCursor<GeneralCursor>(cursor);
                if (generalCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            try
            {
                var followers = followersService.GetFollowedUsersByUserIdWithCursor(
                    out int? lastFollowerId,
                    userId,
                    generalCursor,
                    limit ?? Global.USER_DEFAULT_LIMIT
                );
                var followingViewModels = await usersService.GetUserSimpleViewModels(followers);

                // encode cursor
                string? newCursor = null;
                if (lastFollowerId != null)
                {
                    newCursor = EncodeCursor(new GeneralCursor { Id = (int)lastFollowerId });
                }

                var result = new SearchResults<UserSimpleViewModel>
                {
                    Results = followingViewModels,
                    Cursor = newCursor,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
