using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class ProcessedStripeEvent
{
    public string StripeEventId { get; set; } = null!;

    public DateTimeOffset ProcessedAt { get; set; }
}
