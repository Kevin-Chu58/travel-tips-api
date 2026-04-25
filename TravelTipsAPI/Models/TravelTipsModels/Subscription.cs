using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Subscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PlanId { get; set; }

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset? End { get; set; }

    public int TotalAmount { get; set; }

    public string StripeSubscriptionId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTimeOffset? CanceledAt { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
