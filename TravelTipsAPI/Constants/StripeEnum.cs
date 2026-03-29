using Swashbuckle.AspNetCore.SwaggerGen;

namespace TravelTipsAPI.Constants
{
    public class StripeEnum
    {
        public enum Subscription
        {
            MonthlyMember = 1,
            ThreeMonthMember = 2,
            SixMonthMember = 3,
            YearlyMember = 4,
        };

        public static readonly Dictionary<Subscription, string> PriceIdMap = new()
        {
            { Subscription.MonthlyMember, "price_1T9TX42WannVeKGh9JyipcA7" },
            { Subscription.ThreeMonthMember, "price_1T9TZF2WannVeKGhjhZNnNvs" },
            { Subscription.SixMonthMember, "price_1T9Tb82WannVeKGhw1HeQk3j" },
            { Subscription.YearlyMember, "price_1T9Tbo2WannVeKGhBWsUD8za" },
        };
    }
}
