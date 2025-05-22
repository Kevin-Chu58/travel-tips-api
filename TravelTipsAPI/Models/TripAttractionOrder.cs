using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class TripAttractionOrder
{
    public int Id { get; set; }

    public int DayId { get; set; }

    public int Order { get; set; }

    public int EstimateTime { get; set; }

    public bool IsDrivePreferred { get; set; }

    public bool IsBikePreferred { get; set; }

    public bool IsOnFootPreferred { get; set; }

    public int CreatedBy { get; set; }

    public int HighlightId { get; set; }

    public int EstimateTravelTime { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Day Day { get; set; } = null!;

    public virtual Highlight Highlight { get; set; } = null!;

    public virtual ICollection<TripAttractionOrderRoute> TripAttractionOrderRoutes { get; set; } = new List<TripAttractionOrderRoute>();
}
