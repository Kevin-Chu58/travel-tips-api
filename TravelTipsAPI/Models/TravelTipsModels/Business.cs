using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Business
{
    public int Id { get; set; }

    public int CreatedBy { get; set; }

    public string Name { get; set; } = null!;

    public string Website { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<Ad> Ads { get; set; } = new List<Ad>();

    public virtual User CreatedByNavigation { get; set; } = null!;
}
