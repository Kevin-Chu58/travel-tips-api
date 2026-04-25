using System.Collections.Generic;
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
    public class StripeService(
        IConfiguration config,
        TravelTipsContext context,
        ITargetRulesService targetRulesService
    ) : IStripeService
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

        // preview invoices

        public async Task<StripeBillingCyclePreviewInvoiceResponse> PreviewBillingCycleInvoiceOnAdWeights(
            Ad ad
        )
        {
            if (ad.StripeSubscriptionId == null)
                throw new Exception(Messages.AdStripeSubIdMissing);

            var client = new StripeClient(_apiKey);
            var subscriptionService = new SubscriptionService(client);

            var previewOptions = new InvoiceCreatePreviewOptions
            {
                Subscription = ad.StripeSubscriptionId,
            };

            var invoiceService = new InvoiceService(client);
            Invoice previewInvoice = await invoiceService.CreatePreviewAsync(previewOptions);

            return new StripeBillingCyclePreviewInvoiceResponse
            {
                Currency = previewInvoice.Currency,
                NextBillingAmount = previewInvoice.AmountDue,
                NextBillingDate = previewInvoice.PeriodEnd,
            };
        }

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
            StripeAdWeightRequest request,
            AdTarget? adTarget
        )
        {
            if (user.StripeCustomerId == null)
                throw new Exception(Messages.UserStripeCustomerIdNotFound);

            if (ad.StripeItemId == null || ad.StripeSubscriptionId == null)
                throw new Exception(Messages.AdStripeSubIdMissing);

            var adTargets = context.AdTargets.Where(at => at.AdId == ad.Id).ToList();
            var totalWeight =
                adTargets.Sum(at => at.Weight) + request.Weight - (adTarget?.Weight ?? 0);

            var client = new StripeClient(_apiKey);

            var customerService = new CustomerService(client);
            var customer = await customerService.GetAsync(user.StripeCustomerId);

            // invoice
            var invoiceService = new InvoiceService(client);

            var options = new InvoiceCreatePreviewOptions
            {
                Customer = user.StripeCustomerId,
                Subscription = ad.StripeSubscriptionId,
                SubscriptionDetails = new InvoiceSubscriptionDetailsOptions
                {
                    Items = new List<InvoiceSubscriptionDetailsItemOptions>
                    {
                        new() { Id = ad.StripeItemId, Quantity = totalWeight },
                    },
                    ProrationDate = DateTime.UtcNow,
                    ProrationBehavior = "always_invoice",
                },
            };
            Invoice preview = await invoiceService.CreatePreviewAsync(options);

            // price
            var priceService = new PriceService(client);
            Price price = await priceService.GetAsync(Ad_Target_Weight_Unit_Price);

            // Access the unit amount (e.g., 1000 for $10.00)
            long unitAmount = price.UnitAmount ?? 0;

            return new StripePreviewInvoiceResponse
            {
                Currency = preview.Currency,
                AmountToPayNow = preview.AmountDue, // Total for the upgrade TODAY
                NextCycleTotal = unitAmount * totalWeight, // Total for the NEXT full month
                StartDate = preview.Created,
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
            var totalWeight =
                context.AdTargets.Where(at => at.AdId == ad.Id).Sum(at => at.Weight)
                + request.Weight
                - (adTarget?.Weight ?? 0);

            var itemOptions = new SubscriptionItemOptions
            {
                Id = ad.StripeItemId,
                Quantity = totalWeight,
            };

            var metaData = new Dictionary<string, string>
            {
                { StripeMetaData.TargetType, request.TargetType },
                { StripeMetaData.TargetValue, request.TargetValue },
                { StripeMetaData.AdWeight, request.Weight.ToString() },
            };

            // this distinguishes the subscription_update on existing ad target
            // or a new ad target on the same subscription
            // Note: passing an empty string to metadata clear the value
            metaData.Add(StripeMetaData.AdTargetId, adTarget?.Id.ToString() ?? "");

            // if weight increase, prorate and invoice immediately;
            // if weight decrease, do not prorate and do not invoice immediately
            var existingQuantity = adTarget?.Weight ?? 0;

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
                ProrationBehavior = isWeightIncrease ? "always_invoice" : "create_prorations",
            };

            var client = new StripeClient(_apiKey);
            var subscriptionService = new SubscriptionService(client);

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

            var totalWeight =
                context.AdTargets.Where(at => at.AdId == ad.Id).Sum(at => at.Weight)
                - adTarget.Weight;

            var itemOptions = new SubscriptionItemOptions
            {
                Id = ad.StripeItemId,
                Quantity = totalWeight,
            };

            var metaData = new Dictionary<string, string>
            {
                { StripeMetaData.AdTargetId, adTarget.Id.ToString() },
                {
                    StripeMetaData.SubscriptionUpdateType,
                    GetSubscriptionUpdateTypeStr(SubscriptionUpdateTypeEnum.AdTargetDelete)!
                },
            };

            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions> { itemOptions },
                Metadata = metaData,
                ProrationBehavior = "none",
            };

            var client = new StripeClient(_apiKey);
            var subscriptionService = new SubscriptionService(client);

            await subscriptionService.UpdateAsync(ad.StripeSubscriptionId, options);
        }

        // subscription status

        /// <summary>
        /// Update the subscription status (auto-renew or not) in Stripe
        /// </summary>
        /// <param name="subId">subscription id</param>
        /// <param name="cancelSub">cancel subscription status</param>
        /// <returns></returns>
        public async Task UpdateSubscriptionStatus(string subId, bool cancelSub)
        {
            var service = new SubscriptionService();
            var serviceOptions = GetRequestOptions();
            var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = cancelSub };

            await service.UpdateAsync(subId, options, serviceOptions);
        }
    }
}
