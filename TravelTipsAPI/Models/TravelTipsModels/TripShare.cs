using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TripShare
{
    public int Id { get; set; }

    public int ShareWith { get; set; }

    public int TripId { get; set; }

    public virtual User ShareWithNavigation { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
