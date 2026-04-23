using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Ad
{
    public int Id { get; set; }

    public int CreatedBy { get; set; }

    public int BusinessId { get; set; }

    public int ImageId { get; set; }

    public string? StripeSubscriptionId { get; set; }

    public string? SubStatus { get; set; }

    public string Status { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Text { get; set; }

    public string? LinkLabel { get; set; }

    public string? Link { get; set; }

    public int TemplateId { get; set; }

    public string? StripeItemId { get; set; }

    public bool RenewSub { get; set; }

    public virtual ICollection<AdSubLog> AdSubLogs { get; set; } = new List<AdSubLog>();

    public virtual ICollection<AdTarget> AdTargets { get; set; } = new List<AdTarget>();

    public virtual Business Business { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Image Image { get; set; } = null!;
}
