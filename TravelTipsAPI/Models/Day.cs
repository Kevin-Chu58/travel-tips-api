﻿using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Day
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public TimeOnly Start { get; set; }

    public TimeOnly End { get; set; }

    public bool IsOverNight { get; set; }

    public int CreatedBy { get; set; }

    public int TripId { get; set; }
  
    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;

    public virtual ICollection<TripAttractionOrder> TripAttractionOrders { get; set; } = new List<TripAttractionOrder>();
}
