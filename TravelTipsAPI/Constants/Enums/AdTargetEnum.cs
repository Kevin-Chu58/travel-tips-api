namespace TravelTipsAPI.Constants.Enums
{
    public class AdTargetEnum
    {
        public enum AdTarget
        {
            Region,
            Budget,
            CreatedBy,
            Keyword,
        };

        public static string? GetAdTargetStr(AdTarget? target)
        {
            return target switch
            {
                AdTarget.Region => "region",
                AdTarget.Budget => "budget",
                AdTarget.CreatedBy => "createdBy",
                AdTarget.Keyword => "keyword",
                _ => null,
            };
        }
    }
}
