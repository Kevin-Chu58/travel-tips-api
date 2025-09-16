using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Day
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public int TripId { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;

    public virtual ICollection<TripAttractionOrder> TripAttractionOrders { get; set; } = new List<TripAttractionOrder>();
}
