using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TripFeed
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public string Category { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
