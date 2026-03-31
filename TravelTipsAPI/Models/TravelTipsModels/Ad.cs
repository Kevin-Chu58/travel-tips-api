using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Ad
{
    public int Id { get; set; }

    public int CreatedBy { get; set; }

    public int BusinessId { get; set; }

    public int ImageId { get; set; }

    public string? StripSubscriptionId { get; set; }

    public string? SubStatus { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<AdSubLog> AdSubLogs { get; set; } = new List<AdSubLog>();

    public virtual ICollection<AdTarget> AdTargets { get; set; } = new List<AdTarget>();

    public virtual Business Business { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Image Image { get; set; } = null!;
}
