using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_feed;
using TravelTipsAPI.ViewModels.db_plan;
using TravelTipsAPI.ViewModels.Stripe;
using static TravelTipsAPI.Constants.Enums.StripeEnum;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RecordsSchema;

namespace TravelTipsAPI.Services.StripeServices
{
    public class StripeWebhooksFulfillService(
        TravelTipsContext context,
        IUsersService usersService,
        IUserExtendsService userExtendsService,
        ISubscriptionsService subscriptionsService,
        IAdsService adsService,
        IAdTargetsService adTargetsService,
        IProcessedStripeEventsService processedStripeEventsService,
        IStripeService stripeService,
        ILogger<StripeWebhooksFulfillService> logger
    ) : IStripeWebhooksFulfillService
    {
        // fulfillment

        // this handles both checkout.session.completed when paid,
        // and checkout.session.async_payment_succeeded events
        public async Task FulfillSessionCompletedTaskAsync(string eventId, Session session)
        {
            // subscription id
            var subId = session.SubscriptionId;

            if (string.IsNullOrEmpty(subId))
            {
                logger.LogWarning(
                    "Checkout Session {SessionId} has no SubscriptionId. Skipping.",
                    session.Id
                );
                return;
            }

            // Stripe subscription info
            var subService = new SubscriptionService();
            var serviceOptions = stripeService.GetRequestOptions();
            var subscription = await subService.GetAsync(subId, null, serviceOptions);

            // subscription-wide metadata
            var subscriptionMetadata = subscription.Metadata;

            subscriptionMetadata.TryGetValue(StripeMetaData.UserId, out var userIdStr);
            if (!int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                logger.LogError("Metadata 'user_id' missing or invalid for Sub {SubId}", subId);
                return;
            }

            subscriptionMetadata.TryGetValue(StripeMetaData.ProductType, out var productType);

            switch (productType)
            {
                case "membership":
                    await MembershipSubscriptionSessionCompleted(
                        eventId,
                        userId,
                        session,
                        subscription
                    );
                    break;
                case "adWeight":
                    await AdWeightSessionCompleted(eventId, userId, session, subscription);
                    break;
                default:
                    logger.LogWarning(
                        "Session Completed - Unknown ProductType '{ProductType}' for Sub {SubId}",
                        productType,
                        subId
                    );
                    return;
            }
        }

        // this handles invoice.paid event for subscription billing reasons
        public async Task FulfillInvoicePaidTaskAsync(string eventId, Invoice invoice)
        {
            // Find the line item that actually represents the subscription period
            var subscriptionLine = invoice.Lines.Data.FirstOrDefault(l =>
                l.Parent?.Type == "subscription_item_details"
            );
            if (subscriptionLine == null)
                return; // Not a period-extending event

            var subId = subscriptionLine.Parent.SubscriptionItemDetails.Subscription;

            var subService = new SubscriptionService();
            var serviceOptions = stripeService.GetRequestOptions();

            var subscription = await subService.GetAsync(subId, null, serviceOptions);

            // subscription-wide metadata
            var subscriptionMetadata = subscription.Metadata;

            subscriptionMetadata.TryGetValue(StripeMetaData.UserId, out var userIdStr);
            if (!int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                logger.LogError("Metadata 'user_id' missing or invalid for Sub {SubId}", subId);
                return;
            }

            subscriptionMetadata.TryGetValue(StripeMetaData.ProductType, out var productType);

            // metadata for Ad Weight product type in Subscription.Updated && Invoice.Paid
            // with "subscription_update" billing reason,
            // whether it's a weight adjustment or a new subscription with more weight
            subscriptionMetadata.TryGetValue(StripeMetaData.AdTargetId, out var adTargetIdStr);
            _ = int.TryParse(adTargetIdStr, out int adTargetId);

            switch (productType)
            {
                case "membership":
                    if (invoice.BillingReason == "subscription_cycle")
                        await MembershipSubscriptionNewCycleInvoicePaid(
                            eventId,
                            invoice,
                            subscription
                        );
                    else if (invoice?.BillingReason == "subscription_update")
                    {
                        // DO NOTHING - the only subscription update user can do is to cancel the renew
                        // at the end of the current subscription period, which does not require any
                        // immediate action when invoice is paid
                    }
                    break;
                case "adWeight":
                    if (invoice.BillingReason == "subscription_cycle")
                        await AdSubscriptionNewCycleInvoicePaid(eventId, invoice, subscription);
                    else if (invoice?.BillingReason == "subscription_update")
                    {
                        if (adTargetId == 0)
                            await AdWeightNewInvoicePaid(eventId, invoice, subscription);
                        else
                            await AdWeightMoreInvoicePaid(
                                eventId,
                                adTargetId,
                                invoice,
                                subscription
                            );
                    }
                    break;
                default:
                    logger.LogWarning(
                        "Invoice.paid - Unknown ProductType '{ProductType}' for Sub {SubId}",
                        productType,
                        subId
                    );
                    return;
            }
        }

        public async Task FulfullCustomerSubscriptionUpdatedAsync(
            string eventId,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide metadata
            var subscriptionMetadata = subscription.Metadata;

            subscriptionMetadata.TryGetValue(StripeMetaData.UserId, out var userIdStr);
            if (!int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                logger.LogError(
                    "Metadata 'user_id' missing or invalid for Sub {SubId}",
                    subscription.Id
                );
                return;
            }

            subscriptionMetadata.TryGetValue(StripeMetaData.ProductType, out var productType);

            // has payment failed
            var isPaymentFailed =
                subscription.Status == "past_due" || subscription.Status == "unpaid";

            switch (productType)
            {
                case "membership":
                    if (isPaymentFailed)
                        await MembershipSubscriptionPaymentFailed(eventId, subscription);
                    break;
                case "adWeight":
                    if (isPaymentFailed)
                        await AdPaymentFailed(eventId, subscription);
                    else
                    {
                        // metadata for Ad Weight product type for certain event types
                        subscriptionMetadata.TryGetValue(
                            StripeMetaData.AdTargetId,
                            out var adTargetIdStr
                        );
                        _ = int.TryParse(adTargetIdStr, out int adTargetId);

                        subscriptionMetadata.TryGetValue(
                            StripeMetaData.SubscriptionUpdateType,
                            out var subscriptionUpdateType
                        );

                        switch (subscriptionUpdateType)
                        {
                            case "adTargetWeightDecrease":
                                await AdWeightLessSubscriptionUpdated(
                                    eventId,
                                    adTargetId,
                                    subscription
                                );
                                break;
                            case "adTargetDelete":
                                await AdWeightDeletedSubscriptionUpdated(eventId, adTargetId);
                                break;
                            default:
                                logger.LogWarning(
                                    "Subscription Updated - Unknown SubscriptionUpdateType '{SubscriptionUpdateType}' for Sub {SubId}",
                                    subscriptionUpdateType,
                                    subscription.Id
                                );
                                break;
                        }
                    }
                    break;
                default:
                    logger.LogWarning(
                        "Customer Subscription Updated - Unknown ProductType '{ProductType}' for Sub {SubId}",
                        productType,
                        subscription.Id
                    );
                    return;
            }
        }

        // this handles customer.subscription.deleted event when subscription
        // is deleted due to failed payment after retrying, or manually by admin in Stripe dashboard
        public async Task FulfillCustomerSubscriptionDeletedTaskAsync(
            string eventId,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide metadata
            var subscriptionMetadata = subscription.Metadata;

            subscriptionMetadata.TryGetValue(StripeMetaData.UserId, out var userIdStr);
            if (!int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                logger.LogError(
                    "Metadata 'user_id' missing or invalid for Sub {SubId}",
                    subscription.Id
                );
                return;
            }

            subscriptionMetadata.TryGetValue(StripeMetaData.ProductType, out var productType);

            switch (productType)
            {
                case "membership":
                    await MembershipSubscriptionDeleted(eventId, subscription);
                    break;
                case "adWeight":
                    await AdSubscriptionDeleted(eventId, subscription);
                    break;
                default:
                    logger.LogWarning(
                        "Customer Subscription Deleted - Unknown ProductType '{ProductType}' for Sub {SubId}",
                        productType,
                        subscription.Id
                    );
                    return;
            }
        }

        // this handles customer.deleted event when customer is deleted manually in Stripe dashboard,
        // which is not a common case but we should still handle it just in case
        public async Task FulfillCustomerDeletedTaskAsync(string eventId, Customer customer)
        {
            await CustomerDeleted(eventId, customer);
        }

        // detail fulfillment functions

        // - session completed
        public async Task MembershipSubscriptionSessionCompleted(
            string eventId,
            int userId,
            Session session,
            Stripe.Subscription subscription
        )
        {
            // metadata
            var metadata = session.Metadata;
            metadata.TryGetValue(StripeMetaData.Subscription, out var subscriptionStr);
            metadata.TryGetValue(StripeMetaData.RenewSubscription, out var renewSubscriptionStr);

            var isPlanValid = Enum.TryParse<SubscriptionEnum>(subscriptionStr, out var plan);
            var isRenewSubscriptionValid = bool.TryParse(
                renewSubscriptionStr,
                out var renewSubscription
            );

            // checks
            // - check if renew subscription is valid
            if (!isRenewSubscriptionValid)
                throw new Exception(Messages.StripeRenewSubscriptionInvalid);
            // - check if subscription type is valid
            if (!isPlanValid)
                throw new Exception(Messages.SubscriptionTypeInvalid);
            // - check if user already has an active subscription
            var activeSubscription = subscriptionsService.FindActiveSubscriptionByUserId(userId);
            if (activeSubscription != null)
                throw new Exception(Messages.SubscriptionAlreadyActive);

            // start date and end date
            var subPriceStr = GetSubscriptionPriceStr(plan);
            var subItem = subscription.Items.FirstOrDefault(item => item.Price.Id == subPriceStr);
            if (subItem == null)
                throw new Exception(Messages.StripeSubscriptionItemNotFound);

            DateTime startDate = subItem.CurrentPeriodStart;
            DateTime endDate = subItem.CurrentPeriodEnd;

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // create new subscription record
                var newSubscription = new SubscriptionPostViewModel
                {
                    UserId = userId,
                    PlanId = (int)plan,
                    Start = startDate,
                    End = endDate,
                    TotalAmount = (int)(session.AmountTotal ?? 0),
                    StripeSubscriptionId = subscription.Id,
                };
                await subscriptionsService.AddSubscription(newSubscription);

                // update user record with StripeCustomerId and stripeCurrency if cusomter id is not set
                var user = usersService.GetUserById(userId);

                if (user.StripeCustomerId == null)
                {
                    await usersService.UpdateUserAsync(
                        userId,
                        new UserPatchViewModel
                        {
                            StripeCustomerId = session.CustomerId,
                            StripeCurrency = session.Currency,
                        }
                    );
                }

                // update subscription extension cycle based on the new subscription period
                var userSubExtend = userExtendsService.FindUserSubExtendByUserId(userId);
                await userExtendsService.UpdateSubExtendCycle(userSubExtend, startDate, 0, plan);

                // cancel subscription at the end of current period if user does not want to renew subscription

                if (!renewSubscription)
                    await stripeService.UpdateSubscriptionStatus(subscription.Id, cancelSub: true);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        public async Task AdWeightSessionCompleted(
            string eventId,
            int userId,
            Session session,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide AdWeight metadata
            var subscriptionMetadata = subscription.Metadata;
            subscriptionMetadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);

            _ = int.TryParse(adIdStr, out int adId);

            // metadata
            var metadata = session.Metadata;
            metadata.TryGetValue(StripeMetaData.TargetType, out var targetType);
            metadata.TryGetValue(StripeMetaData.TargetValue, out var targetValue);

            // ad weight quantity
            var subPriceStr = StripeEnum.Ad_Target_Weight_Unit_Price;
            var subItem = subscription.Items.FirstOrDefault(item => item.Price.Id == subPriceStr);
            if (subItem == null)
                throw new Exception(Messages.StripeSubscriptionItemNotFound);

            var quantity = (int)subItem.Quantity;

            // ad
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // create new ad target record
                var adTarget = new AdTargetPostViewModel
                {
                    TargetType = targetType!,
                    TargetValue = targetValue!,
                    Weight = quantity,
                };

                await adTargetsService.PostNewAdTarget(adTarget, adId);

                // update the StripeSubscriptionId in the ad record to link the ad with the Stripe subscription
                await adsService.UpdateAdStripeSubInfo(ad, subscription.Id, subItem.Id);

                // update user record with StripeCustomerId and stripeCurrency if cusomter id is not set
                var user = usersService.GetUserById(userId);

                if (user.StripeCustomerId == null)
                {
                    await usersService.UpdateUserAsync(
                        userId,
                        new UserPatchViewModel
                        {
                            StripeCustomerId = session.CustomerId,
                            StripeCurrency = session.Currency,
                        }
                    );
                }

                await adsService.UpdateAdStripeSubStatus(ad, "active");

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        // - invoice paid -> subscription_cycle
        public async Task MembershipSubscriptionNewCycleInvoicePaid(
            string eventId,
            Invoice invoice,
            Stripe.Subscription subscription
        )
        {
            // check
            // - check if stripe customer id is valid
            var stripeCustomerId = invoice.CustomerId;
            if (stripeCustomerId == null)
                throw new Exception(Messages.UserNotFound);
            // - check if user exists with the stripe customer id
            var user = usersService.GetUserByStripeCustomerId(stripeCustomerId);
            if (user == null)
                throw new Exception(Messages.UserNotFound);

            // last subscription record for the user
            var lastSubscription = subscriptionsService.FindLastSubscriptionByUserId(user.Id);
            if (lastSubscription == null)
                throw new Exception(Messages.SubscriptionNotFound);

            // start date and end date
            var subPriceStr = GetSubscriptionPriceStr((SubscriptionEnum)lastSubscription.PlanId);
            var subItem = subscription.Items.FirstOrDefault(item => item.Price.Id == subPriceStr);
            if (subItem == null)
                throw new Exception(Messages.StripeSubscriptionItemNotFound);

            DateTime startDate = subItem.CurrentPeriodStart;
            DateTime endDate = subItem.CurrentPeriodEnd;

            // total amount paid
            var totalAmount = (int)invoice.AmountPaid;

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // create a new subscription record for the new subscription cycle
                // if the new subscription period starts after the last subscription start date
                if (lastSubscription.Start < startDate)
                {
                    if (lastSubscription.Status == "active")
                    {
                        await subscriptionsService.UpdateSubscription(
                            lastSubscription,
                            new SubscriptionPatchViewModel { End = startDate, Status = "expired" }
                        );
                    }

                    await subscriptionsService.AddSubscription(
                        new SubscriptionPostViewModel
                        {
                            UserId = user.Id,
                            PlanId = lastSubscription.PlanId,
                            Start = startDate,
                            End = endDate,
                            TotalAmount = totalAmount,
                            StripeSubscriptionId = subscription.Id,
                        }
                    );

                    // update subscription extension cycle based on the new subscription period
                    var userSubExtend = userExtendsService.FindUserSubExtendByUserId(user.Id);
                    await userExtendsService.UpdateSubExtendCycle(
                        userSubExtend,
                        startDate,
                        0,
                        (SubscriptionEnum)lastSubscription.PlanId
                    );
                }

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        public async Task AdSubscriptionNewCycleInvoicePaid(
            string eventId,
            Invoice invoice,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide AdWeight metadata
            var subscriptionMetadata = subscription.Metadata;
            subscriptionMetadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);

            _ = int.TryParse(adIdStr, out int adId);

            // ad with the subscription id
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // update the ad target cycle for the ad
                await adTargetsService.UpdateAdTargetCycleByAdId(adId);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        // - invoice paid -> subscription update
        public async Task AdWeightMoreInvoicePaid(
            string eventId,
            int adTargetId,
            Invoice invoice,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide AdWeight metadata
            var subscriptionMetadata = subscription.Metadata;
            subscriptionMetadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);

            _ = int.TryParse(adIdStr, out int adId);

            // ad with the subscription id
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // ad target
            var adTarget = adTargetsService.FindAdTargetById(adTargetId);
            if (adTarget == null)
                throw new Exception(Messages.AdTargetNotFound);

            // check
            // - whether ad target is bound to a stripe item,
            //   if the ad target is canceled before, the stripe item id is set to null,
            //   and since the stripe item is also deleted on Stripe,
            //   the user can no longer update the ad target anymore.
            var itemId = ad.StripeItemId;
            if (itemId == null)
                throw new Exception(Messages.AdTargetStripeItemIdMissing);

            // metadata
            var metadata = subscription.Metadata;
            metadata.TryGetValue(StripeMetaData.TargetType, out var targetType);
            metadata.TryGetValue(StripeMetaData.TargetValue, out var targetValue);

            metadata.TryGetValue(StripeMetaData.AdWeight, out var adWeightStr);
            _ = int.TryParse(adWeightStr, out int adWeight);

            var request = new StripeAdWeightRequest
            {
                TargetType = targetType!,
                TargetValue = targetValue!,
                Weight = adWeight,
            };

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // update the ad target weight with the new weight
                await adTargetsService.UpdateAdTarget(adTarget, request);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        public async Task AdWeightNewInvoicePaid(
            string eventId,
            Invoice invoice,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide AdWeight metadata
            var subscriptionMetadata = subscription.Metadata;
            subscriptionMetadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);

            _ = int.TryParse(adIdStr, out int adId);

            // ad with the subscription id
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // metadata
            var metadata = subscription.Metadata;
            metadata.TryGetValue(StripeMetaData.TargetType, out var targetType);
            metadata.TryGetValue(StripeMetaData.TargetValue, out var targetValue);

            metadata.TryGetValue(StripeMetaData.AdWeight, out var adWeightStr);
            _ = int.TryParse(adWeightStr, out int adWeight);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // create new ad target record
                var adTarget = new AdTargetPostViewModel
                {
                    TargetType = targetType!,
                    TargetValue = targetValue!,
                    Weight = adWeight,
                };

                await adTargetsService.PostNewAdTarget(adTarget, adId);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        // - subscription updated > payment failed
        public async Task MembershipSubscriptionPaymentFailed(
            string eventId,
            Stripe.Subscription subscription
        )
        {
            // membership subscription
            var sub = subscriptionsService.FindSubscriptionByStripeSubId(subscription.Id);
            if (sub == null)
                throw new Exception(Messages.SubscriptionNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                if (subscription.Status == "unpaid")
                {
                    // update the subscription status to past_due
                    await subscriptionsService.UpdateSubscription(
                        sub,
                        new SubscriptionPatchViewModel { Status = "past_due" }
                    );

                    // update subscription extension cycle back to default (no active subscription)
                    var userSubExtend = userExtendsService.FindUserSubExtendByUserId(sub.UserId);
                    await userExtendsService.UpdateSubExtendCycle(userSubExtend, null, null);
                }

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        public async Task AdPaymentFailed(string eventId, Stripe.Subscription subscription)
        {
            // subscription-wide metadata for Ad Weight product type,
            // whether it's a weight adjustment or a new subscription with more weight
            var metadata = subscription.Metadata;
            metadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);
            _ = int.TryParse(adIdStr, out int adId);

            // ad
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // update the ad target status to past_due
                if (subscription.Status == "unpaid")
                    await adsService.UpdateAdStripeSubStatus(ad, "past_due");

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        // - subscription updated > manual updates
        public async Task AdWeightLessSubscriptionUpdated(
            string eventId,
            int adTargetId,
            Stripe.Subscription subscription
        )
        {
            // subscription-wide AdWeight metadata
            var subscriptionMetadata = subscription.Metadata;
            subscriptionMetadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);

            _ = int.TryParse(adIdStr, out int adId);

            // ad with the subscription id
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // ad target
            var adTarget = adTargetsService.FindAdTargetById(adTargetId);
            if (adTarget == null)
                throw new Exception(Messages.AdTargetNotFound);

            // metadata
            var metadata = subscription.Metadata;
            metadata.TryGetValue(StripeMetaData.TargetType, out var targetType);
            metadata.TryGetValue(StripeMetaData.TargetValue, out var targetValue);

            metadata.TryGetValue(StripeMetaData.AdWeight, out var adWeightStr);
            _ = int.TryParse(adWeightStr, out int adWeight);

            var request = new StripeAdWeightRequest
            {
                TargetType = targetType!,
                TargetValue = targetValue!,
                Weight = adWeight,
            };

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // update the ad target weight with the new weight
                await adTargetsService.UpdateAdTarget(adTarget, request);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        public async Task AdWeightDeletedSubscriptionUpdated(string eventId, int adTargetId)
        {
            // ad target
            var adTarget = adTargetsService.FindAdTargetById(adTargetId);
            if (adTarget == null)
                throw new Exception(Messages.AdTargetNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // delete the ad target record
                await adTargetsService.DeleteAdTarget(adTarget);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        // - subscription deleted
        public async Task MembershipSubscriptionDeleted(
            string eventId,
            Stripe.Subscription subscription
        )
        {
            var stripeCustomerId = subscription.CustomerId;

            if (string.IsNullOrEmpty(stripeCustomerId))
                throw new Exception(Messages.UserNotFound);

            var user = usersService.GetUserByStripeCustomerId(stripeCustomerId);
            if (user == null)
                throw new Exception(Messages.UserNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // find the active subscription with the same StripeSubscriptionId and cancel it
                await subscriptionsService.ExpireActiveSubscriptionByUserId(user.Id);

                // update subscription extension cycle back to default (no active subscription)
                var userSubExtend = userExtendsService.FindUserSubExtendByUserId(user.Id);
                await userExtendsService.UpdateSubExtendCycle(userSubExtend, null, null);

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        public async Task AdSubscriptionDeleted(string eventId, Stripe.Subscription subscription)
        {
            // subscription-wide AdWeight metadata
            var subscriptionMetadata = subscription.Metadata;
            subscriptionMetadata.TryGetValue(StripeMetaData.AdId, out var adIdStr);

            _ = int.TryParse(adIdStr, out int adId);

            // ad with the subscription id
            var ad = adsService.FindAdById(adId);
            if (ad == null)
                throw new Exception(Messages.AdNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // update the ad target status to canceled
                await adsService.UpdateAdStripeSubStatus(ad, "canceled");

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }

        // - customer deleted
        public async Task CustomerDeleted(string eventId, Customer customer)
        {
            var stripeCustomerId = customer.Id;
            var user = usersService.GetUserByStripeCustomerId(stripeCustomerId);
            if (user == null)
                throw new Exception(Messages.UserNotFound);

            // TX Begins
            var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // set StripeCustomerId to null for the user
                // NOTE: only for Stripe sandbox testing purpose (mannual customer delete),
                //       in production we should not delete the Stripe customer
                //       but rather keep them
                //await usersService.RemoveUserStripeCustomerId(user.Id);

                // find the active subscription with the same StripeSubscriptionId and cancel it
                await subscriptionsService.ExpireActiveSubscriptionByUserId(user.Id);

                // reset the subscription extension cycle to default (all max counts to default)
                var userSubExtend = userExtendsService.FindUserSubExtendByUserId(user.Id);
                await userExtendsService.UpdateSubExtendCycle(userSubExtend, null, null);

                // reset the ads created by the user its sub status to "canceled"
                await context
                    .Ads.Where(a => a.CreatedBy == user.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.SubStatus, "canceled"));

                await context.SaveChangesAsync();

                // mark the Stripe event as processed
                await processedStripeEventsService.AddProcessedEvent(eventId);

                // TX Ends
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // Let background worker retry
            }
        }
    }
}
