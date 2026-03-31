using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TargetRule
{
    public int Id { get; set; }

    public string TargetType { get; set; } = null!;

    public string TargetValue { get; set; } = null!;

    public int MinWeight { get; set; }
}
