﻿using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Trip
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public bool IsPublic { get; set; }

    public bool IsHidden { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Day> Days { get; set; } = new List<Day>();

    public virtual ICollection<SmallTrip> SmallTrips { get; set; } = new List<SmallTrip>();

    public virtual ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
}
