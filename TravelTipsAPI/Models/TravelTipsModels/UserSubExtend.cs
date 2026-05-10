using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class UserSubExtend
{
    public int UserId { get; set; }

    public DateTimeOffset? CycleStart { get; set; }

    public int? MonthIndex { get; set; }

    public int TripCount { get; set; }

    public int MaxTripCount { get; set; }

    public virtual User User { get; set; } = null!;
}
