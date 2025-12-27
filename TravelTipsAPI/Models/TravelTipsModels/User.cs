using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class User
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool UserAgreement { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Day> Days { get; set; } = new List<Day>();

    public virtual ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<TripAttractionOrder> TripAttractionOrders { get; set; } = new List<TripAttractionOrder>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
