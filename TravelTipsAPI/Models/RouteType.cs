using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class RouteType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PreferRoute> PreferRoutes { get; set; } = new List<PreferRoute>();
}
