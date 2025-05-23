using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Highlight
{
    public int Id { get; set; }

    public int AttractionId { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public int? LinkId { get; set; }

    public bool IsDeprecated { get; set; }

    public virtual Attraction Attraction { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Link? Link { get; set; }

    public virtual ICollection<TripAttractionOrder> TripAttractionOrders { get; set; } = new List<TripAttractionOrder>();
}
