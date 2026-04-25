namespace TravelTipsAPI.Constants
{
    public class StripeMetaData
    {
        // product type - the things users can purchase
        public enum ProductTypeEnum
        {
            Membership = 1,
            AdWeight = 2,
        };

        public static string? GetPaymentTypeStr(ProductTypeEnum productTypeEnum)
        {
            return productTypeEnum switch
            {
                ProductTypeEnum.Membership => "membership",
                ProductTypeEnum.AdWeight => "adWeight",
                _ => null,
            };
        }

        // subscription update type
        public enum SubscriptionUpdateTypeEnum
        {
            AdTargetWeightDecrease = 1,
            AdTargetDelete = 2,
        };

        public static string? GetSubscriptionUpdateTypeStr(
            SubscriptionUpdateTypeEnum subscriptionUpdateTypeEnum
        )
        {
            return subscriptionUpdateTypeEnum switch
            {
                SubscriptionUpdateTypeEnum.AdTargetWeightDecrease => "adTargetWeightDecrease",
                SubscriptionUpdateTypeEnum.AdTargetDelete => "adTargetDelete",
                _ => null,
            };
        }

        // Subscription-wide meta data attributes (the same for all subscription events (created, updated, deleted, etc.)
        public static readonly string UserId = "user_id";
        public static readonly string ProductType = "product_type"; // ProductTypeEnum value

        // - for Ad Weight product type
        public static readonly string AdId = "ad_id";

        // ProductType - Membership meta data attributes
        public static readonly string Subscription = "subscription"; // SubscriptionEnum value
        public static readonly string RenewSubscription = "renew_subscription"; // bool value, only for subscription renewal events

        // ProductType - Ad Weight meta data attributes
        public static readonly string TargetType = "target_type"; // AdTargetEnum.AdTarget value
        public static readonly string TargetValue = "target_value";

        // EventType - subscription.updated meta data attributes
        public static readonly string SubscriptionUpdateType = "subscription_update_type"; // SubscriptionUpdateTypeEnum value

        // - for Ad Weight product type
        //   and invoice.paid with "subscription_update" as the billing reason
        public static readonly string AdTargetId = "ad_target_id";
        public static readonly string AdWeight = "ad_weight";
    }
}
