using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class BannerStyling
{
    public int Id { get; set; }

    public string? Styling { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Banner> Banners { get; set; } = new List<Banner>();
}
