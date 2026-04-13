namespace TravelTipsAPI.Constants.Enums
{
    public class AdEnum
    {
        public enum AdStatus
        {
            Active,
            Inactive,
            Pending,
            RequestChange,
            Denied,
        };

        public static string? GetAdStatusStr(AdStatus? status)
        {
            return status switch
            {
                AdStatus.Active => "active",
                AdStatus.Inactive => "inactive",
                AdStatus.Pending => "pending",
                AdStatus.RequestChange => "request change",
                AdStatus.Denied => "denied",
                _ => null,
            };
        }
    }
}
