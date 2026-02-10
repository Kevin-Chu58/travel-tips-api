using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Bookmark
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }

    public virtual Trip Trip { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
