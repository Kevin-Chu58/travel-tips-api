using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class WritingLabel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? ParentLabelId { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<WritingLabel> InverseParentLabel { get; set; } = new List<WritingLabel>();

    public virtual WritingLabel? ParentLabel { get; set; }

    public virtual ICollection<Writing> Writings { get; set; } = new List<Writing>();
}
