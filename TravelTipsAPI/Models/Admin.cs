using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class Admin
{
    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
