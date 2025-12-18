using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Trip
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsPublic { get; set; }

    public bool IsHidden { get; set; }

    public string Title { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Day> Days { get; set; } = new List<Day>();

    public virtual ICollection<TripImage> TripImages { get; set; } = new List<TripImage>();
}
