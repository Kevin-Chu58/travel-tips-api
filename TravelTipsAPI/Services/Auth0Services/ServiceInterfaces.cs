namespace TravelTipsAPI.Services.Auth0Services
{
    public class Auth0Schema
    {
        public interface IAuth0Service
        {
            Task<Auth0UserInfo?> GetUserInfoAsync();
        }
    }
}
