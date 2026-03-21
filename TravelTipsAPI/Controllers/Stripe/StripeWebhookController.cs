using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_plan;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;

namespace TravelTipsAPI.Controllers.Stripe
{
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class StripeWebhookController(
        TravelTipsContext context,
        IConfiguration config,
        IUsersService usersService,
        ISubscriptionsService subscriptionsService
    ) : TravelTipsControllerBase
    {
        private readonly string _webhookSecret =
            "whsec_49e137f14230b4b38cd624c02adb05d8320b4a437ebd63ce35d9c60bf5af9dec";
        private readonly string _apiKey =
            config["Stripe:ApiKey"] ?? throw new ArgumentException("Stripe:ApiKey not configured");

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

                if (
                    stripeEvent.Type == EventTypes.CheckoutSessionCompleted
                    || stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded
                )
                {
                    var session = stripeEvent.Data.Object as Session;

                    var userIdStr = session!.ClientReferenceId;
                    var userId = int.TryParse(userIdStr, out var id) ? id : (int?)null;

                    var subId = session.SubscriptionId;

                    var subService = new SubscriptionService();
                    var serviceOptions = new RequestOptions { ApiKey = _apiKey };

                    // Pass the options as the second or third parameter depending on the method
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

                    await usersService.UpdateUserAsync(
                        (int)userId!,
                        new UserPatchViewModel { StripeCustomerId = session.CustomerId }
                    );

                    await tx.CommitAsync();

                    if (!renewSub)
                    {
                        var service = new SubscriptionService();
                        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = true };

                        await service.UpdateAsync(subId, options);
                    }
                }
                else if (stripeEvent.Type == EventTypes.InvoicePaid)
                {
                    var invoice = stripeEvent.Data.Object as Invoice;

                    if (invoice?.BillingReason == "subscription_create")
                    {
                        return Ok();
                    }

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

                        var start = subscriptionLine.Period.Start;
                        var end = subscriptionLine.Period.End;

                        var subId = subscriptionLine.Parent.SubscriptionItemDetails.Subscription;
                        var stripeCustomerId = invoice.CustomerId;

                        var user = usersService.GetUserByStripeCustomerId(stripeCustomerId!);
                        if (user is null)
                        {
                            return BadRequest(Messages.UserNotFound);
                        }

                        var subscription = subscriptionsService.FindLastSubscriptionByUserId(
                            user.Id
                        );

                        var tx = await context.Database.BeginTransactionAsync();

                        var totalAmount = (int)invoice.AmountPaid;

                        if (subscription == null)
                        {
                            return BadRequest(Messages.SubscriptionNotFound);
                        }

                        if (subscription.Start < start && subId != null)
                        {
                            if (subscription.Status == "active")
                            {
                                await subscriptionsService.UpdateSubscription(
                                    subscription,
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
                                    PlanId = subscription.PlanId,
                                    Start = start,
                                    End = end,
                                    TotalAmount = totalAmount,
                                    Currency = invoice.Currency,
                                    StripeSubscriptionId = subId,
                                }
                            );
                        }

                        await tx.CommitAsync();
                    }
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
