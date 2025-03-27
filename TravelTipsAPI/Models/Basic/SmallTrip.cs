using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.Basic;

public partial class SmallTrip
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int TripId { get; set; }

    public virtual Trip Trip { get; set; } = null!;
}
