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
            switch (target)
            {
                case AdTarget.Region:
                    return "region";
                case AdTarget.Budget:
                    return "budget";
                case AdTarget.CreatedBy:
                    return "createdBy";
                case AdTarget.Keyword:
                    return "keyword";
                default:
                    return null;
            }
        }
    }
}
