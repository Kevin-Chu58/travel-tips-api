using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class SermonLabel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? ParentLabelId { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<SermonLabel> InverseParentLabel { get; set; } = new List<SermonLabel>();

    public virtual SermonLabel? ParentLabel { get; set; }

    public virtual ICollection<Sermon> Sermons { get; set; } = new List<Sermon>();
}
