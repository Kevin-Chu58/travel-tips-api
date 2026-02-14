using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Follower
{
    public int Id { get; set; }

    public int Followed { get; set; }

    public int Following { get; set; }

    public virtual User FollowedNavigation { get; set; } = null!;

    public virtual User FollowingNavigation { get; set; } = null!;
}
