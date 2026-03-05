using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Banner
{
    public int Id { get; set; }

    public int? StylingId { get; set; }

    public string Title { get; set; } = null!;

    public string Overview { get; set; } = null!;

    public int ImageId { get; set; }

    public string Link { get; set; } = null!;

    public DateOnly From { get; set; }

    public DateOnly? To { get; set; }

    public string? Label { get; set; }

    public string? SubLabel { get; set; }

    public virtual Image Image { get; set; } = null!;

    public virtual BannerStyling? Styling { get; set; }
}
