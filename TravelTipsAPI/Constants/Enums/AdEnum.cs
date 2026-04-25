namespace TravelTipsAPI.Constants.Enums
{
    public class AdEnum
    {
        public enum AdStatus
        {
            Pending,
            Active,
            Inactive,
            RequestChange,
            Denied,
        };

        public static string? GetAdStatusStr(AdStatus? status)
        {
            return status switch
            {
                AdStatus.Pending => "pending",
                AdStatus.Active => "active",
                AdStatus.Inactive => "inactive",
                AdStatus.RequestChange => "request change",
                AdStatus.Denied => "denied",
                _ => null,
            };
        }
    }
}
