using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Writing
{
    public int Id { get; set; }

    public int CreatedBy { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public int? LabelId { get; set; }

    public DateOnly PublishAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual WritingLabel? Label { get; set; }
}
