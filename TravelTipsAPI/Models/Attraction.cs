﻿using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Attraction
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Address { get; set; } = null!;

    public int CreatedBy { get; set; }

    public int? LinkId { get; set; }

    public int OsmId { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Link? Link { get; set; }

    public virtual ICollection<TripAttractionOrder> TripAttractionOrders { get; set; } = new List<TripAttractionOrder>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
