using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TripAttractionOrder
{
    public int Id { get; set; }

    public int DayId { get; set; }

    public int? HighlightId { get; set; }

    public int CreatedBy { get; set; }

    public int AttractionId { get; set; }

    public TimeOnly Start { get; set; }

    public TimeOnly End { get; set; }

    public string? TransportMode { get; set; }

    public bool IsPrivate { get; set; }

    public virtual Attraction Attraction { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Day Day { get; set; } = null!;

    public virtual Highlight? Highlight { get; set; }
}
