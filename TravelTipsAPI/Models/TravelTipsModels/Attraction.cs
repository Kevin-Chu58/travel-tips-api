using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Attraction
{
    public int Id { get; set; }

    public string HereId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Category { get; set; }

    public bool IsDeprecated { get; set; }

    public decimal Lng { get; set; }

    public decimal Lat { get; set; }

    public string Address { get; set; } = null!;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string ResultType { get; set; } = null!;

    public virtual ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();

    public virtual ICollection<PreferRoute> PreferRouteArrivalAttractions { get; set; } = new List<PreferRoute>();

    public virtual ICollection<PreferRoute> PreferRouteDepartAttractions { get; set; } = new List<PreferRoute>();
}
