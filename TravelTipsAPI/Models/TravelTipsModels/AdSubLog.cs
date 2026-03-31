using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class AdSubLog
{
    public int Id { get; set; }

    public int AdId { get; set; }

    public DateTimeOffset Time { get; set; }

    public string Note { get; set; } = null!;

    public int? OldValue { get; set; }

    public int? NewValue { get; set; }

    public virtual Ad Ad { get; set; } = null!;
}
