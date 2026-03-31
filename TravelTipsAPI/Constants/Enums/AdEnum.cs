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
            switch (status)
            {
                case AdStatus.Active:
                    return "active";
                case AdStatus.Inactive:
                    return "inactive";
                case AdStatus.Pending:
                    return "pending";
                case AdStatus.RequestChange:
                    return "request change";
                case AdStatus.Denied:
                    return "denied";
                default:
                    return null;
            }
        }
    }
}
