using Swashbuckle.AspNetCore.SwaggerGen;

namespace TravelTipsAPI.Constants.Enums
{
    public class StripeEnum
    {
        // membership

        public enum SubscriptionEnum
        {
            MonthlyMember = 1,
            ThreeMonthMember = 2,
            SixMonthMember = 3,
            YearlyMember = 4,
        };

        public static string? GetSubscriptionPriceStr(SubscriptionEnum subscription)
        {
            return subscription switch
            {
                SubscriptionEnum.MonthlyMember => "price_1T9TX42WannVeKGh9JyipcA7",
                SubscriptionEnum.ThreeMonthMember => "price_1T9TZF2WannVeKGhjhZNnNvs",
                SubscriptionEnum.SixMonthMember => "price_1T9Tb82WannVeKGhw1HeQk3j",
                SubscriptionEnum.YearlyMember => "price_1T9Tbo2WannVeKGhBWsUD8za",
                _ => null,
            };
        }

        // ad weight

        public static readonly string Ad_Target_Weight_Unit_Price =
            "price_1T9U2E2WannVeKGhXa5hGMH9";
    }
}
