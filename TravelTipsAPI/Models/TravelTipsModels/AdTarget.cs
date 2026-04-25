using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class AdTarget
{
    public int Id { get; set; }

    public int AdId { get; set; }

    public string TargetType { get; set; } = null!;

    public string TargetValue { get; set; } = null!;

    public int Weight { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Ad Ad { get; set; } = null!;
}
