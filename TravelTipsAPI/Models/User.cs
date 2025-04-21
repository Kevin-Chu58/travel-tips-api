using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();

    public virtual ICollection<Day> Days { get; set; } = new List<Day>();

    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    public virtual ICollection<PreferRoute> PreferRoutes { get; set; } = new List<PreferRoute>();

    public virtual ICollection<TripAttractionOrder> TripAttractionOrders { get; set; } = new List<TripAttractionOrder>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
