using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Region
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? ParentRegionId { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Region> InverseParentRegion { get; set; } = new List<Region>();

    public virtual Region? ParentRegion { get; set; }

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
