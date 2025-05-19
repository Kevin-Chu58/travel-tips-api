using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Link
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int CreatedBy { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();

    public virtual ICollection<PreferRoute> PreferRoutes { get; set; } = new List<PreferRoute>();
}
