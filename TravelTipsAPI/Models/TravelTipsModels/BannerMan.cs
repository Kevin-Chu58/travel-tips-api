using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class BannerMan
{
    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
