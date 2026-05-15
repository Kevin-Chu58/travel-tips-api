using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class Trip
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsPublic { get; set; }

    public bool IsHidden { get; set; }

    public int? RegionId { get; set; }

    public int? Budget { get; set; }

    public int BookmarkCount { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Day> Days { get; set; } = new List<Day>();

    public virtual Region? Region { get; set; }

    public virtual ICollection<TripFeed> TripFeeds { get; set; } = new List<TripFeed>();

    public virtual ICollection<TripImage> TripImages { get; set; } = new List<TripImage>();

    public virtual ICollection<TripShare> TripShares { get; set; } = new List<TripShare>();
}
