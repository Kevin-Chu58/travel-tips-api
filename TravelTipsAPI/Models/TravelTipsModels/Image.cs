using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Image
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int CreatedBy { get; set; }

    public Guid Guid { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<Banner> Banners { get; set; } = new List<Banner>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<TripImage> TripImages { get; set; } = new List<TripImage>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
