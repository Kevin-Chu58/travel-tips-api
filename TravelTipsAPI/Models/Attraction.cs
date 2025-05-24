using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Attraction
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public long OsmId { get; set; }

    public string OsmType { get; set; } = null!;

    public decimal Lat { get; set; }

    public decimal Lng { get; set; }

    public virtual ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();

    public virtual ICollection<PreferRoute> PreferRouteArrivalAttractions { get; set; } = new List<PreferRoute>();

    public virtual ICollection<PreferRoute> PreferRouteDepartAttractions { get; set; } = new List<PreferRoute>();
}
