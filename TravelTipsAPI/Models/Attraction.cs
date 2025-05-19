using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Attraction
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public long OsmId { get; set; }
}
