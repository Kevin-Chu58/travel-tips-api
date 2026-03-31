using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_plan;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;

namespace TravelTipsAPI.Controllers.Stripe
{
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class StripeWebhookController(
        TravelTipsContext context,
        IUsersService usersService,
        IUserExtendsService userExtendsService,
        ISubscriptionsService subscriptionsService,
        IStripeService stripeService
    ) : TravelTipsControllerBase
    {
        private readonly string _webhookSecret =
            "whsec_49e137f14230b4b38cd624c02adb05d8320b4a437ebd63ce35d9c60bf5af9dec";

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                var session = stripeEvent.Data.Object as Session;

                if (
                    session != null
                    && (
                        (
                            stripeEvent.Type == EventTypes.CheckoutSessionCompleted
                            && session.PaymentStatus == "paid"
                        )
                        || stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded
                    )
                )
                {
                    var userIdStr = session.ClientReferenceId;
                    var userId = int.TryParse(userIdStr, out var id) ? id : (int?)null;

                    var subId = session.SubscriptionId;

                    var subService = new SubscriptionService();
                    var serviceOptions = stripeService.GetRequestOptions();

                    var subscription = await subService.GetAsync(subId, null, serviceOptions);

                    var subItem = subscription.Items.Data[0];
                    DateTime startDate = subItem.CurrentPeriodStart;
                    DateTime endDate = subItem.CurrentPeriodEnd;

                    session.Metadata.TryGetValue("subscription", out var subStr);
                    session.Metadata.TryGetValue("renew_subscription", out var renewSubStr);
                    var renewSub = bool.Parse(renewSubStr!);

                    var validSub = Enum.TryParse<StripeEnum.Subscription>(subStr, out var sub);
                    if (!validSub)
                    {
                        return BadRequest(Messages.SubscriptionTypeInvalid);
                    }

                    var tx = await context.Database.BeginTransactionAsync();

                    // create new subscription record
                    var newSub = new SubscriptionPostViewModel
                    {
                        UserId = (int)userId!,
                        PlanId = (int)sub,
                        Start = startDate,
                        End = endDate,
                        TotalAmount = (int)(session.AmountTotal ?? 0),
                        Currency = session.Currency ?? "usd",
                        StripeSubscriptionId = subId,
                    };

                    switch (sub)
                    {
                        case StripeEnum.Subscription.MonthlyMember:
                        case StripeEnum.Subscription.ThreeMonthMember:
                        case StripeEnum.Subscription.SixMonthMember:
                        case StripeEnum.Subscription.YearlyMember:
                            await subscriptionsService.AddSubscription(newSub);
                            break;
                    }

                    var user = usersService.GetUserById((int)userId!);

                    // if the user does not have a StripeCustomerId or it is different
                    // from the latest one in the session, update it for the user
                    if (
                        user.StripeCustomerId == null
                        || user.StripeCustomerId != session.CustomerId
                    )
                    {
                        await usersService.UpdateUserAsync(
                            (int)userId!,
                            new UserPatchViewModel { StripeCustomerId = session.CustomerId }
                        );
                    }

                    // update subscription extension cycle based on the new subscription period
                    var userSubExtend = userExtendsService.FindUserSubExtendByUserId((int)userId!);
                    await userExtendsService.UpdateSubExtendCycle(
                        userSubExtend,
                        startDate,
                        0,
                        (int)sub
                    );

                    await tx.CommitAsync();

                    // if user does not want to renew subscription
                    // set cancel_at_period_end to true
                    if (!renewSub)
                    {
                        await subscriptionsService.UpdateSubscriptionStatus(subId, cancelSub: true);
                    }
                }
                // handle subscription auto renewal and manual renewal via Stripe Customer Portal
                else if (stripeEvent.Type == EventTypes.InvoicePaid)
                {
                    var invoice = stripeEvent.Data.Object as Invoice;

                    // only handle subscription creation and renewal events,
                    // ignore other invoice events like one-time invoice or invoice for non-subscription products
                    if (invoice?.BillingReason == "subscription_create")
                    {
                        return Ok();
                    }

                    // subscription auto renewal OR manual renewal via Stripe Customer Portal
                    if (
                        invoice?.BillingReason == "subscription_cycle"
                        || invoice?.BillingReason == "subscription_update"
                    )
                    { // Find the line item that actually represents the subscription period
                        var subscriptionLine = invoice.Lines.Data.FirstOrDefault(l =>
                            l.Parent?.Type == "subscription_item_details"
                        );
                        if (subscriptionLine == null)
                            return Ok(); // Not a period-extending event

                        var subId = subscriptionLine.Parent.SubscriptionItemDetails.Subscription;
                        var stripeCustomerId = invoice.CustomerId;

                        var subService = new SubscriptionService();
                        var serviceOptions = stripeService.GetRequestOptions();

                        var subscription = await subService.GetAsync(subId, null, serviceOptions);

                        var subItem = subscription.Items.Data[0];
                        DateTime start = subItem.CurrentPeriodStart;
                        DateTime end = subItem.CurrentPeriodEnd;

                        if (stripeCustomerId == null)
                            return BadRequest(Messages.UserNotFound);

                        var user = usersService.GetUserByStripeCustomerId(stripeCustomerId);
                        if (user == null)
                            return BadRequest(Messages.UserNotFound);

                        var lastSubscription = subscriptionsService.FindLastSubscriptionByUserId(
                            user.Id
                        );

                        var tx = await context.Database.BeginTransactionAsync();

                        var totalAmount = (int)invoice.AmountPaid;

                        if (lastSubscription == null)
                            return BadRequest(Messages.SubscriptionNotFound);

                        // if the new subscription period starts after the last subscription period,
                        // create a new subscription record for the new period
                        if (lastSubscription.Start < start && subId != null)
                        {
                            if (subscription.Status == "active")
                            {
                                await subscriptionsService.UpdateSubscription(
                                    lastSubscription,
                                    new SubscriptionPatchViewModel
                                    {
                                        End = start,
                                        Status = "expired",
                                    }
                                );
                            }

                            await subscriptionsService.AddSubscription(
                                new SubscriptionPostViewModel
                                {
                                    UserId = user.Id,
                                    PlanId = lastSubscription.PlanId,
                                    Start = start,
                                    End = end,
                                    TotalAmount = totalAmount,
                                    Currency = invoice.Currency,
                                    StripeSubscriptionId = subId,
                                }
                            );

                            // update subscription extension cycle based on the new subscription period
                            var userSubExtend = userExtendsService.FindUserSubExtendByUserId(
                                user.Id
                            );
                            await userExtendsService.UpdateSubExtendCycle(
                                userSubExtend,
                                start,
                                0,
                                lastSubscription.PlanId
                            );
                        }

                        await tx.CommitAsync();
                    }
                }
                // cancel subscription immediately OR cancel at period end
                // OR automatic cancellation after too many failed payment attempts
                else if (stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted)
                {
                    var subscription = stripeEvent.Data.Object as global::Stripe.Subscription;
                    var stripeCustomerId = subscription?.CustomerId;

                    if (subscription == null || string.IsNullOrEmpty(stripeCustomerId))
                        return Ok();

                    var user = usersService.GetUserByStripeCustomerId(stripeCustomerId);
                    if (user == null)
                        return Ok();

                    var tx = await context.Database.BeginTransactionAsync();

                    // find the active subscription with the same StripeSubscriptionId and cancel it
                    await subscriptionsService.ExpireActiveSubscriptionByUserId(user.Id);

                    // update subscription extension cycle back to default (no active subscription)
                    var userSubExtend = userExtendsService.FindUserSubExtendByUserId(user.Id);
                    await userExtendsService.UpdateSubExtendCycle(userSubExtend, null, null);

                    await tx.CommitAsync();
                }
                else if (stripeEvent.Type == EventTypes.CustomerDeleted)
                {
                    var customer = stripeEvent.Data.Object as Customer;

                    var stripeCustomerId = customer.Id;
                    var user = usersService.GetUserByStripeCustomerId(stripeCustomerId);
                    if (user == null)
                        return Ok();

                    var tx = await context.Database.BeginTransactionAsync();

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

                    await tx.CommitAsync();
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
