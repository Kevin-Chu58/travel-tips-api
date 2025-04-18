using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class TripAttractionOrderRoute
{
    public int TripAttractionOrderId { get; set; }

    public int PreferRouteId { get; set; }

    public int Order { get; set; }

    public virtual PreferRoute PreferRoute { get; set; } = null!;

    public virtual TripAttractionOrder TripAttractionOrder { get; set; } = null!;
}
