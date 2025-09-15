using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TripImage
{
    public int Id { get; set; }

    public int ImageId { get; set; }

    public int TripId { get; set; }

    public virtual Image Image { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
