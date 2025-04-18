using TravelTipsAPI.Models;
using static TravelTipsAPI.Services.RoleSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of User Roles
    /// </summary>
    /// <param name="context">context</param>
    public class UserRolesService(TravelTipsContext context) : IUserRolesService
    {
        /// <summary>
        /// Check if the user is admin
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>whether is admin</returns>
        public bool IsAdmin(int id)
        {
            var isAdmin = context.Admins.Find(id);

            return isAdmin != null;
        }
    }
}
