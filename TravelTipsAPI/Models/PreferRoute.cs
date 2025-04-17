using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class PreferRoute
{
    public int Id { get; set; }

    public int Type { get; set; }

    public string Ref { get; set; } = null!;

    public int CreatedBy { get; set; }

    public int DepartOsmId { get; set; }

    public int ArrivalOsmId { get; set; }

    public int EstimateTime { get; set; }

    public int? LinkId { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Link? Link { get; set; }

    public virtual ICollection<TripAttractionOrderRoute> TripAttractionOrderRoutes { get; set; } = new List<TripAttractionOrderRoute>();

    public virtual RouteType TypeNavigation { get; set; } = null!;
}
