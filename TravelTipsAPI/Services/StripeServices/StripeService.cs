using Stripe;
using Stripe.Checkout;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.Stripe;
using static TravelTipsAPI.Constants.Enums.StripeEnum;
using static TravelTipsAPI.Constants.StripeMetaData;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Services.StripeServices
{
    public class StripeService(IConfiguration config, ITargetRulesService targetRulesService)
        : IStripeService
    {
        private readonly string _apiKey =
            config["Stripe:ApiKey"] ?? throw new ArgumentException("Stripe:ApiKey not configured");

        public RequestOptions? GetRequestOptions()
        {
            return new RequestOptions { ApiKey = _apiKey };
        }

        public string GetApiKey()
        {
            return _apiKey;
        }

        // create sessions

        /// <summary>
        /// Create a Stripe checkout session on general membership subscription
        /// </summary>
        /// <param name="user">user who wants subsscription</param>
        /// <param name="request">general Stripe session request</param>
        /// <returns>an url to the Stripe checkout session</returns>
        public async Task<string> CreateSession(User user, ViewModels.Stripe.StripeRequest request)
        {
            var priceId = GetSubscriptionPriceStr(request.Subscription);
            if (priceId is null)
                throw new Exception(Messages.SubscriptionTypeInvalid);

            var options = new SessionCreateOptions
            {
                SuccessUrl = $"{Global.URL}/subscription",
                CancelUrl = $"{Global.URL}/subscription",
                // Use existing Stripe customer ID, or null if it does not exist
                Customer = user.StripeCustomerId,
                // Optional: pre-fill the customer's email in the Stripe checkout form
                CustomerEmail = user.StripeCustomerId == null ? user.Email : null,
                Mode = "subscription",
                LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { StripeMetaData.UserId, user.Id.ToString() },
                        {
                            StripeMetaData.ProductType,
                            GetPaymentTypeStr(ProductTypeEnum.Membership)!
                        },
                    },
                },
                Metadata = new Dictionary<string, string>
                {
                    { StripeMetaData.Subscription, request.Subscription.ToString() },
                    { StripeMetaData.RenewSubscription, user.RenewSubscription.ToString() },
                },
            };

            try
            {
                var client = new StripeClient(_apiKey);
                var service = new SessionService(client);
                Session session = await service.CreateAsync(options);
                return session.Url; // Redirect user to this URL
            }
            catch (StripeException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Create a Stripe checkout session on monthly ad-target weight subscription, for ads without sub id
        /// </summary>
        /// <param name="user">user who wants subsscription</param>
        /// <param name="ad">ad to be subscribed on</param>
        /// <param name="request">ad weight Stripe session request</param>
        /// <returns>an url to the Stripe checkout session</returns>
        public async Task<string> CreateSessionOnAdWeightsWithoutSubId(
            User user,
            Ad ad,
            StripeAdWeightRequest request
        )
        {
            var targetRule = targetRulesService.GetTargetRule(
                request.TargetType,
                request.TargetValue
            );

            if (targetRule is null)
                throw new Exception(Messages.TargetRuleNotFound);

            if (request.Weight < targetRule!.MinWeight)
                throw new Exception(Messages.TargetRuleMinWeightNotMet);

            // Condition - user does not have a Stripe subscription id
            var options = new SessionCreateOptions
            {
                SuccessUrl = $"{Global.URL}/business/{ad.BusinessId}/ads/{ad.Id}",
                CancelUrl = $"{Global.URL}/business/{ad.BusinessId}/ads/{ad.Id}",
                // Use existing Stripe customer ID, or null if it does not exist
                Customer = user.StripeCustomerId,
                // Optional: pre-fill the customer's email in the Stripe checkout form
                CustomerEmail = user.StripeCustomerId == null ? user.Email : null,
                Mode = "subscription",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = Ad_Target_Weight_Unit_Price,
                        Quantity = request.Weight,
                    },
                ],
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { StripeMetaData.UserId, user.Id.ToString() },
                        { StripeMetaData.AdId, ad.Id.ToString() },
                        {
                            StripeMetaData.ProductType,
                            GetPaymentTypeStr(ProductTypeEnum.AdWeight)!
                        },
                    },
                },
                Metadata = new Dictionary<string, string>
                {
                    { StripeMetaData.TargetType, request.TargetType },
                    { StripeMetaData.TargetValue, request.TargetValue },
                },
            };

            try
            {
                var client = new StripeClient(_apiKey);
                var service = new SessionService(client);
                Session session = await service.CreateAsync(options);
                return session.Url; // Redirect user to this URL
            }
            catch (StripeException e)
            {
                throw new Exception(e.Message);
            }
        }

        // preview invoices - use this when want to add items and quantities to existing subscription

        /// <summary>
        /// Preview the invoice for more ad weights on an existing ad with sub id
        /// </summary>
        /// <param name="user">user who wants to buy more ad weights</param>
        /// <param name="ad">ad attached to</param>
        /// <param name="request">stripe ad weight request</param>
        /// <returns>a response with previewed payment</returns>
        public async Task<StripePreviewInvoiceResponse> PreviewUpcomingInvoiceOnAdWeights(
            User user,
            Ad ad,
            StripeAdWeightRequest request
        )
        {
            if (user.StripeCustomerId == null)
                throw new Exception(Messages.UserStripeCustomerIdNotFound);

            var client = new StripeClient(_apiKey);
            var invoiceService = new InvoiceService(client);

            var startDate = DateTime.UtcNow;

            var options = new InvoiceCreatePreviewOptions
            {
                Customer = user.StripeCustomerId,
                Subscription = ad.StripeSubscriptionId,
                SubscriptionDetails = new InvoiceSubscriptionDetailsOptions
                {
                    Items = new List<InvoiceSubscriptionDetailsItemOptions>
                    {
                        new() { Price = Ad_Target_Weight_Unit_Price, Quantity = request.Weight },
                    },
                    StartDate = startDate,
                },
            };

            Invoice preview = await invoiceService.CreatePreviewAsync(options);

            return new StripePreviewInvoiceResponse
            {
                AmountToPayNow = preview.AmountDue,
                NextCycleTotal = preview.Total,
                StartDate = startDate,
            };
        }

        // update subscriptions

        /// <summary>
        /// Update Stripe subscription on ad weights (increase or decrease), the new weight cannot be 0
        /// </summary>
        /// <param name="ad">ad id</param>
        /// <param name="request">stripe ad weight request</param>
        /// <param name="adTarget">ad target</param>
        /// <returns></returns>
        public async Task UpdateSubscriptionOnAdWeights(
            Ad ad,
            StripeAdWeightRequest request,
            AdTarget? adTarget
        )
        {
            if (ad.StripeSubscriptionId == null)
                throw new Exception(Messages.AdStripeSubIdMissing);

            if (adTarget?.FutureWeight == 0)
                throw new Exception(Messages.AdTargetAlreadyCanceled);

            // check target rule whether the new weight meets the min weight requirement
            var targetRule = targetRulesService.GetTargetRule(
                request.TargetType,
                request.TargetValue
            );
            if (targetRule is null)
                throw new Exception(Messages.TargetRuleNotFound);

            if (request.Weight < targetRule.MinWeight)
                throw new Exception(Messages.TargetRuleMinWeightNotMet);

            var client = new StripeClient(_apiKey);
            var subscriptionService = new SubscriptionService(client);

            var existingQuantity = adTarget?.Weight ?? 0;

            var itemOptions = new SubscriptionItemOptions
            {
                Price = Ad_Target_Weight_Unit_Price,
                Quantity = request.Weight,
            };

            // if the ad target already has a Stripe item id, use that id;
            // otherwise, add a new item to the subscription
            if (!string.IsNullOrEmpty(adTarget?.StripeItemId))
            {
                itemOptions.Id = adTarget?.StripeItemId;
            }

            var metaData = new Dictionary<string, string>
            {
                { StripeMetaData.TargetType, request.TargetType },
                { StripeMetaData.TargetValue, request.TargetValue },
            };

            // this distinguishes the subscription_update on existing ad target
            // or a new ad target on the same subscription
            // Note: passing an empty string to metadata clear the value
            metaData.Add(StripeMetaData.AdTargetId, adTarget?.Id.ToString() ?? "");

            // if weight increase, prorate and invoice immediately;
            // if weight decrease, do not prorate and do not invoice immediately
            var isWeightIncrease = existingQuantity < request.Weight;
            var isWeightDecrease = existingQuantity > request.Weight;
            if (isWeightDecrease)
            {
                metaData.Add(
                    StripeMetaData.SubscriptionUpdateType,
                    GetSubscriptionUpdateTypeStr(SubscriptionUpdateTypeEnum.AdTargetWeightDecrease)!
                );
            }

            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions> { itemOptions },
                Metadata = metaData,
                ProrationBehavior = isWeightIncrease ? "always_invoice" : "none",
            };

            await subscriptionService.UpdateAsync(ad.StripeSubscriptionId, options);
        }

        /// <summary>
        /// Update Stripe subscription on deleting the corresponding Stripe Item by ad target
        /// </summary>
        /// <param name="ad">ad attached to</param>
        /// <param name="adTarget">ad target to be canceled</param>
        /// <returns></returns>
        public async Task UpdateSubscriptionOnDeleteAdTarget(Ad ad, AdTarget adTarget)
        {
            if (ad.StripeSubscriptionId == null)
                throw new Exception(Messages.AdStripeSubIdMissing);

            var client = new StripeClient(_apiKey);
            var subscriptionService = new SubscriptionService(client);

            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
                {
                    new() { Id = adTarget.StripeItemId, Deleted = true },
                },
                Metadata = new Dictionary<string, string>
                {
                    { StripeMetaData.AdTargetId, adTarget.Id.ToString() },
                    {
                        StripeMetaData.SubscriptionUpdateType,
                        GetSubscriptionUpdateTypeStr(SubscriptionUpdateTypeEnum.AdTargetDelete)!
                    },
                },
                ProrationBehavior = "none",
            };

            await subscriptionService.UpdateAsync(ad.StripeSubscriptionId, options);
        }
    }
}
